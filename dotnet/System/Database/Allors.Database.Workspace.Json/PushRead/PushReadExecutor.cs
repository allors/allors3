// <copyright file="PushReadExecutor.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using Adapters.Sql;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.PushRead;
    using Allors.Protocol.Json.Api.Read;
    using Allors.Protocol.Json.SystemTextJson;
    using Security;
    using Services;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using SqlDatabase = Adapters.Sql.Database;
    using WireScope = Allors.Protocol.Json.Api.Read.ReadScope;

    /// <summary>
    /// Commits one scope-admitted Push, then reads its requested results from one fresh snapshot.
    /// The mutation receipt survives every subsequent failure. No operation is retried.
    /// </summary>
    public sealed class PushReadExecutor
    {
        public const int MaxRequestBytes = ReadExecutor.MaxRequestBytes;
        public const int MaxResponseBytes = ReadExecutor.MaxResponseBytes;

        private readonly SqlDatabase database;
        private readonly ReadConfiguration configuration;
        private readonly ReadScopeFactory reads;
        private readonly ILogger<PushReadExecutor> logger;

        public PushReadExecutor(IDatabase database, ReadConfiguration configuration, ILogger<PushReadExecutor> logger = null)
        {
            this.database = database as SqlDatabase ?? throw new NotSupportedException("PushRead requires a supported SQL adapter with protected write and read scopes.");
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.reads = new ReadScopeFactory(database, configuration);
            this.logger = logger ?? NullLogger<PushReadExecutor>.Instance;
        }

        public string Execute(string rawRequest, string workspaceName, LocalReadIdentity identity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(identity);
            return this.Execute(rawRequest, workspaceName, transaction => transaction.Instantiate(identity.UserId) as IUser, identity.Population, cancellationToken);
        }

        public string Execute(string rawRequest, string workspaceName, Func<ITransaction, IUser> resolveUser, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(resolveUser);
            return this.Execute(rawRequest, workspaceName, resolveUser, null, cancellationToken);
        }

        private string Execute(string rawRequest, string workspaceName, Func<ITransaction, IUser> resolveUser, Guid? population, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (rawRequest != null && Encoding.UTF8.GetByteCount(rawRequest) > MaxRequestBytes)
            {
                throw new FormatException("Reduce the PushRead request to at most 1 MiB before sending it.");
            }

            // Strict decoding and capture precede transaction creation or identity resolution.
            var request = PushReadJson.DeserializeRequest(rawRequest);
            var response = new PushReadResponse { i = request.i };
            WriteScope write = null;
            PushResponse pushed = null;
            PushResponseNewObject[] mappings = null;
            Exception writeFailure = null;
            IUserService userService = null;
            var identityChanged = false;
            EventHandler onUserChanged = (_, _) => identityChanged = true;
            try
            {
                var revision = this.configuration.Revision(this.database.MetaPopulation, workspaceName);
                if (!Guid.TryParse(request.sc.p, out var expectedPopulation) || (population.HasValue && population != expectedPopulation))
                {
                    throw new ReadRequestException("The expected population does not match the authenticated Local identity. Retire the old connection and authenticate again before writing.");
                }

                write = this.database.OpenWrite(expectedPopulation);
                var transaction = write.Transaction;
                var user = resolveUser(transaction);
                cancellationToken.ThrowIfCancellationRequested();
                if (user == null || user.Id <= 0 || !ReferenceEquals(user.Strategy.Transaction, transaction)
                    || user.Strategy.IsDeleted || user.Strategy.IsNewInTransaction || !ReferenceEquals(transaction.Instantiate(user.Id), user))
                {
                    throw new ReadRequestException("The authenticated identity must resolve to an existing committed user in this write transaction. Authenticate again before writing.");
                }

                var scope = new WireScope { p = write.Population.Incarnation.ToString("D"), u = user.Id, w = workspaceName, r = revision, x = request.x };
                if (!SameScope(scope, request.sc))
                {
                    throw new ReadRequestException("The expected scope does not match the authenticated population, user, workspace, representation or context. Retire the old connection before writing.");
                }

                response.sc = scope;
                userService = transaction.Services.Get<IUserService>();
                userService.User = user;
                userService.UserChanged += onUserChanged;
                var convert = new UnitConvert();
                if (request.discover != null)
                {
                    // Validate inline declarations, metadata and typed parameters before effects.
                    ReadCompiler.Compile(transaction, workspaceName, request.discover, convert);
                }

                var api = new Api(transaction, workspaceName, cancellationToken, this.logger);
                cancellationToken.ThrowIfCancellationRequested();
                write.AllowWrites();
                foreach (var submitted in request.push.o ?? Array.Empty<PushRequestObject>())
                {
                    var value = transaction.Instantiate(submitted.d);
                    if (value != null) { write.IncludeSubmitted(value.Strategy); }
                }

                var changes = new PushRequest
                {
                    n = request.push.n, x = request.x,
                    o = request.push.o?.Select(value => new PushRequestObject
                    {
                        d = value.d, v = value.v, r = value.r ?? Array.Empty<PushRequestRole>(),
                    }).ToArray(),
                };
                // Tracing observes detached DTOs; hooks cannot rewrite the captured mutation.
                var trace = api.Sink?.OnPush(transaction, JsonSerializer.Deserialize<PushRequest>(JsonSerializer.Serialize(changes)));
                api.Sink?.OnBefore(trace);
                var builder = new PushResponseBuilder(transaction, api.Derive, api.MetaPopulation, api.AccessControl, api.AllowedClasses, api.Build, convert);
                pushed = builder.Apply(changes);
                cancellationToken.ThrowIfCancellationRequested();
                if (identityChanged || !ReferenceEquals(userService.User, user))
                {
                    throw new ReadRequestException("The authenticated user changed during PushRead. Discard this attempt and authenticate again before writing.");
                }

                if (!pushed.HasErrors)
                {
                    // Tracing receives the legacy mutable response. Detach and validate the receipt
                    // before commit so later hooks cannot change the acknowledged new IDs.
                    mappings = (pushed.n ?? Array.Empty<PushResponseNewObject>())
                        .Select(value => new PushResponseNewObject { w = value.w, d = value.d }).ToArray();
                    var receipt = new PushReadReceipt
                    {
                        i = request.i, sc = scope, push = new PushReadOutcome { k = "committed", n = mappings },
                    };
                    PushReadJson.DeserializeReceipt(JsonSerializer.Serialize(receipt), request);
                    write.Commit();
                }

                if (trace != null)
                {
                    trace.PushResponse = JsonSerializer.Deserialize<PushResponse>(JsonSerializer.Serialize(pushed));
                    api.Sink.OnAfter(trace);
                }
            }
            catch (Exception exception)
            {
                writeFailure = exception;
                this.Log(exception, "PushRead write execution failed; the recorded commit outcome is retained.");
            }
            finally
            {
                try
                {
                    write?.Dispose();
                }
                catch (Exception exception)
                {
                    writeFailure ??= exception;
                    this.Log(exception, "PushRead write cleanup failed; the recorded commit outcome is retained.");
                }
                finally
                {
                    if (userService != null) { userService.UserChanged -= onUserChanged; }
                }
            }

            var state = write?.CommitState ?? WriteCommitState.NotCommitted;
            if (state == WriteCommitState.Committed)
            {
                response.push = new PushReadOutcome { k = "committed", n = mappings };
                if (writeFailure != null)
                {
                    response.push.e = Errors("post-commit-failed", "The Push committed, but its completion failed. Use the receipt to reconcile; do not replay the mutation.");
                    FailReads(request, response, "The Push committed, but write completion failed. Request a new explicit read to refresh the saved objects.");
                }
                else if (request.returnChanged == true || request.discover != null)
                {
                    return this.Read(request, response, write.AffectedObjects.ToArray(), workspaceName, resolveUser, cancellationToken);
                }
            }
            else
            {
                response.push = state == WriteCommitState.Unknown
                    ? new PushReadOutcome { k = "unknown", e = Errors("push-outcome-unknown", "The database commit outcome is unknown. Reconcile the saved objects before another mutation; do not replay this request.") }
                    : new PushReadOutcome
                    {
                        k = "rejected", _e = pushed?._e, _v = pushed?._v, _a = pushed?._a, _m = pushed?._m, _d = pushed?._d,
                        e = pushed?.HasErrors == true && writeFailure == null ? null : Errors("push-rejected", writeFailure is ReadRequestException ? writeFailure.Message : "The Push was not committed. Check the scope, changes, cancellation and server diagnostics before making a new request."),
                    };
            }

            return Serialize(response, request);
        }

        private string Read(PushReadRequest request, PushReadResponse response, AffectedObject[] affected, string workspaceName,
            Func<ITransaction, IUser> resolveUser, CancellationToken cancellationToken)
        {
            try
            {
                using var context = this.reads.Open(workspaceName, resolveUser, request.x, response.sc, cancellationToken);
                if (request.returnChanged == true)
                {
                    response.changed = this.ReadPart(request, response.sc, () =>
                    {
                        var empty = new ReadPull { l = Array.Empty<Allors.Protocol.Json.Data.Pull>(), d = Array.Empty<Allors.Protocol.Json.Api.Pull.PullDependency>() };
                        var readRequest = new ReadRequest { v = 1, i = request.i, sc = response.sc, x = request.x, k = "read", p = empty };
                        var compilation = new ReadCompilation { Pulls = Array.Empty<Data.Pull>() };
                        return new ReadResponseBuilder(context, readRequest, compilation, new UnitConvert()).BuildAffected(affected);
                    });
                    context.EnsureValid();
                }

                if (request.discover != null)
                {
                    response.discovery = this.ReadPart(request, response.sc, () =>
                    {
                        var readRequest = new ReadRequest { v = 1, i = request.i, sc = response.sc, x = request.x, k = "discover", p = request.discover };
                        var convert = new UnitConvert();
                        var compilation = ReadCompiler.Compile(context.Transaction, workspaceName, request.discover, convert);
                        return new ReadResponseBuilder(context, readRequest, compilation, convert).Build();
                    });
                    context.EnsureValid();
                }

                // Validate and bound the entire detached envelope while its snapshot is still open.
                var json = Serialize(response, request);
                context.Complete();
                return json;
            }
            catch (Exception exception)
            {
                this.Log(exception, "PushRead post-commit snapshot failed; both read payloads are discarded.");
                FailReads(request, response, "The Push committed, but its protected refresh could not complete. Request a new explicit read; do not replay the mutation.");
                return Serialize(response, request);
            }
        }

        private object ReadPart(PushReadRequest request, WireScope scope, Func<object> read)
        {
            try
            {
                var result = read();
                if (Encoding.UTF8.GetByteCount(JsonSerializer.Serialize(result, result.GetType())) > MaxResponseBytes)
                {
                    throw new ReadRequestException("The requested refresh exceeds 16 MiB. Read a smaller selection after acknowledging the committed Push.");
                }

                return result;
            }
            catch (Exception exception)
            {
                this.Log(exception, "One PushRead refresh failed; the other result still requires a valid shared snapshot.");
                return Failure(request.i, scope, exception is ReadRequestException ? exception.Message : "The Push committed, but this refresh failed. Request a new explicit read; do not replay the mutation.");
            }
        }

        private static string Serialize(PushReadResponse response, PushReadRequest request)
        {
            var json = JsonSerializer.Serialize(response);
            if (Encoding.UTF8.GetByteCount(json) > MaxResponseBytes)
            {
                throw new ReadRequestException("The complete PushRead response exceeds 16 MiB. Request smaller reads after acknowledging the committed Push.");
            }

            PushReadJson.DeserializeResponse(json, request);
            return json;
        }

        private static bool SameScope(WireScope left, WireScope right) => left.p == right.p && left.u == right.u && left.w == right.w && left.r == right.r && left.x == right.x;

        private static ReadError[] Errors(string code, string message) => new[] { new ReadError { c = code, m = message } };

        private static ReadFailure Failure(string id, WireScope scope, string message) => new ReadFailure { i = id, sc = scope, k = "error", e = Errors("read-failed", message) };

        private static void FailReads(PushReadRequest request, PushReadResponse response, string message)
        {
            if (request.returnChanged == true) { response.changed = Failure(request.i, response.sc, message); }
            if (request.discover != null) { response.discovery = Failure(request.i, response.sc, message); }
        }

        private void Log(Exception exception, string message)
        {
            try { this.logger.LogWarning(exception, message); }
            catch { /* A logging failure cannot erase an observed database commit. */ }
        }
    }
}
