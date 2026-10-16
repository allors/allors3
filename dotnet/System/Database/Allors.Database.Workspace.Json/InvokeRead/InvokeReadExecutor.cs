// <copyright file="InvokeReadExecutor.cs" company="Allors bv">
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
    using Allors.Protocol.Json.Api.Invoke;
    using Allors.Protocol.Json.Api.InvokeRead;
    using Allors.Protocol.Json.Api.Read;
    using Allors.Protocol.Json.SystemTextJson;
    using Security;
    using Services;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using SqlDatabase = Adapters.Sql.Database;
    using WireScope = Allors.Protocol.Json.Api.Read.ReadScope;

    /// <summary>Commits one admitted atomic method batch, then reads affected objects from a fresh snapshot.</summary>
    public sealed class InvokeReadExecutor
    {
        public const int MaxRequestBytes = ReadExecutor.MaxRequestBytes;
        public const int MaxResponseBytes = ReadExecutor.MaxResponseBytes;
        private const string RefreshFailed = "The Invoke committed, but its protected refresh could not complete. Request a new explicit read; do not replay the mutation.";
        private readonly SqlDatabase database;
        private readonly ReadConfiguration configuration;
        private readonly ReadScopeFactory reads;
        private readonly ILogger<InvokeReadExecutor> logger;

        public InvokeReadExecutor(IDatabase database, ReadConfiguration configuration, ILogger<InvokeReadExecutor> logger = null)
        {
            this.database = database as SqlDatabase ?? throw new NotSupportedException("InvokeRead requires a supported SQL adapter with protected write and read scopes.");
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.reads = new ReadScopeFactory(database, configuration);
            this.logger = logger ?? NullLogger<InvokeReadExecutor>.Instance;
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
                throw new FormatException("Reduce the InvokeRead request to at most 1 MiB before sending it.");
            }

            // Capture validated data before opening a write or resolving the authenticated identity.
            var request = InvokeReadJson.DeserializeRequest(rawRequest);
            var response = new InvokeReadResponse { i = request.i };
            WriteScope write = null;
            InvokeResponse invoked = null;
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
                void EnsureValid()
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (identityChanged || !ReferenceEquals(userService.User, user))
                    {
                        throw new ReadRequestException("The authenticated user changed during InvokeRead. Discard this attempt and authenticate again before writing.");
                    }
                }

                var api = new Api(transaction, workspaceName, cancellationToken, this.logger);
                var batch = new InvokeRequest { l = request.l, x = request.x };
                var builder = new InvokeResponseBuilder(transaction, api.Derive, api.AccessControl, api.AllowedClasses);
                var prepared = builder.Prepare(batch, workspaceName, out invoked);
                EnsureValid();
                if (!invoked.HasErrors)
                {
                    write.AllowWrites();
                    foreach (var entry in prepared) write.IncludeSubmitted(entry.Target.Strategy);
                    // Observers receive detached DTOs and cannot rewrite the captured plan or receipt.
                    var trace = api.Sink?.OnInvoke(transaction, JsonSerializer.Deserialize<InvokeRequest>(JsonSerializer.Serialize(batch)));
                    api.Sink?.OnBefore(trace);
                    EnsureValid();
                    builder.Apply(prepared, invoked, EnsureValid);
                    EnsureValid();
                    if (!invoked.HasErrors)
                    {
                        var candidate = new InvokeReadReceipt { i = request.i, sc = scope, invoke = new InvokeReadOutcome { k = "committed" } };
                        var candidateJson = JsonSerializer.Serialize(candidate);
                        if (Encoding.UTF8.GetByteCount(candidateJson) > MaxResponseBytes) throw new ReadRequestException("Reduce the invocation receipt metadata before writing.");
                        InvokeReadJson.DeserializeReceipt(candidateJson, request);
                        EnsureValid();
                        // The candidate is not commit evidence. Only the observed provider state below is.
                        write.Commit();
                    }

                    if (trace != null)
                    {
                        trace.InvokeResponse = JsonSerializer.Deserialize<InvokeResponse>(JsonSerializer.Serialize(invoked));
                        api.Sink.OnAfter(trace);
                    }
                }
            }
            catch (Exception exception)
            {
                writeFailure = exception;
                this.Log(exception, "InvokeRead execution failed; the recorded commit outcome is retained.");
            }
            finally
            {
                try { write?.Dispose(); }
                catch (Exception exception)
                {
                    writeFailure ??= exception;
                    this.Log(exception, "InvokeRead write cleanup failed; the recorded commit outcome is retained.");
                }
                finally
                {
                    if (identityChanged) writeFailure ??= new ReadRequestException("The authenticated identity changed during write completion.");
                    if (userService != null) userService.UserChanged -= onUserChanged;
                }
            }

            var state = write?.CommitState ?? WriteCommitState.NotCommitted;
            if (state == WriteCommitState.Committed)
            {
                response.invoke = new InvokeReadOutcome { k = "committed" };
                if (writeFailure != null)
                {
                    response.invoke.e = Errors("post-commit-failed", "The Invoke committed, but its completion failed. Reconcile from the receipt; do not replay this mutation.");
                    if (request.refresh) response.changed = Failure(request.i, response.sc);
                }
                else if (request.refresh)
                {
                    return this.Read(request, response, write.AffectedObjects.ToArray(), workspaceName, resolveUser, cancellationToken);
                }
            }
            else
            {
                response.invoke = state == WriteCommitState.Unknown
                    ? new InvokeReadOutcome { k = "unknown", e = Errors("invoke-outcome-unknown", "The database commit outcome is unknown. Reconcile before another mutation; do not replay this request.") }
                    : new InvokeReadOutcome
                    {
                        k = "rejected", _e = invoked?._e, _v = invoked?._v, _a = invoked?._a, _m = invoked?._m, _d = invoked?._d,
                        e = invoked?.HasErrors == true && writeFailure == null ? null : Errors("invoke-rejected", writeFailure is ReadRequestException ? writeFailure.Message : "The Invoke was not committed. Check the scope, targets, permissions, cancellation and server diagnostics before making a new request."),
                    };
            }

            return this.Serialize(response, request);
        }

        private string Read(InvokeReadRequest request, InvokeReadResponse response, AffectedObject[] affected, string workspaceName,
            Func<ITransaction, IUser> resolveUser, CancellationToken cancellationToken)
        {
            try
            {
                using var context = this.reads.Open(workspaceName, resolveUser, request.x, response.sc, cancellationToken);
                var empty = new ReadPull { l = Array.Empty<Allors.Protocol.Json.Data.Pull>(), d = Array.Empty<Allors.Protocol.Json.Api.Pull.PullDependency>() };
                var readRequest = new ReadRequest { v = 1, i = request.i, sc = response.sc, x = request.x, k = "read", p = empty };
                var compilation = new ReadCompilation { Pulls = Array.Empty<Data.Pull>() };
                response.changed = new ReadResponseBuilder(context, readRequest, compilation, new UnitConvert()).BuildAffected(affected);
                context.EnsureValid();
                var json = this.Serialize(response, request);
                context.Complete();
                return json;
            }
            catch (Exception exception)
            {
                this.Log(exception, "InvokeRead protected refresh failed; its entire affected payload is discarded.");
                response.changed = Failure(request.i, response.sc);
                return this.Serialize(response, request);
            }
        }

        private string Serialize(InvokeReadResponse response, InvokeReadRequest request)
        {
            try { return SerializeValidated(response, request); }
            catch (Exception exception)
            {
                this.Log(exception, "InvokeRead response validation or size failed; returning the bounded independent receipt.");
                // Never truncate affected records, expose unbounded exception text, or replace an observed commit.
                var kind = response.invoke.k;
                response.invoke = new InvokeReadOutcome
                {
                    k = kind,
                    e = kind == "committed" ? null : Errors("invoke-diagnostics-unavailable", "The Invoke outcome is recorded, but its detailed diagnostics could not be returned. Reconcile an unknown outcome; inspect server diagnostics for a rejection."),
                };
                response.changed = kind == "committed" && request.refresh ? Failure(request.i, response.sc) : null;
                return SerializeValidated(response, request);
            }
        }

        private static string SerializeValidated(InvokeReadResponse response, InvokeReadRequest request)
        {
            var json = JsonSerializer.Serialize(response);
            if (Encoding.UTF8.GetByteCount(json) > MaxResponseBytes) throw new ReadRequestException("The complete InvokeRead response exceeds 16 MiB. Request smaller reads after the committed mutation.");
            InvokeReadJson.DeserializeResponse(json, request);
            return json;
        }

        private static bool SameScope(WireScope left, WireScope right) => left.p == right.p && left.u == right.u && left.w == right.w && left.r == right.r && left.x == right.x;
        private static ReadError[] Errors(string code, string message) => new[] { new ReadError { c = code, m = message } };
        private static ReadFailure Failure(string id, WireScope scope) => new ReadFailure { i = id, sc = scope, k = "error", e = Errors("read-failed", RefreshFailed) };
        private void Log(Exception exception, string message)
        {
            try { this.logger.LogWarning(exception, message); }
            catch { /* Logging must preserve the observed commit outcome. */ }
        }
    }
}
