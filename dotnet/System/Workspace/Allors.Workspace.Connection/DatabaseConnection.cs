// <copyright file="DatabaseConnection.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Protocol.Json.Api;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.Security;
    using Allors.Protocol.Json.Api.Sync;
    using Data;
    using Json;
    using Meta;
    using Ranges;
    using JsonInvocation = Allors.Protocol.Json.Api.Invoke.Invocation;
    using JsonInvokeRequest = Allors.Protocol.Json.Api.Invoke.InvokeRequest;
    using JsonInvokeOptions = Allors.Protocol.Json.Api.Invoke.InvokeOptions;

    /// <summary>
    /// The connection over a transport. After a pull it brings the records of the pulled objects
    /// up to date: it syncs the objects whose version, grants or revocations differ from what it
    /// holds, requests the grants and revocations it lacks and then the permissions those name.
    /// Every request names the workspace and the meta fingerprint the connection is built for;
    /// every response is checked against them before anything is stored, and the first response
    /// tells which database and user the connection is to, which no later response may change.
    /// Use it from one thread at a time.
    /// </summary>
    public sealed class DatabaseConnection : IDatabaseConnection
    {
        private readonly ITransport transport;
        private readonly ICache cache;
        private readonly PushEncoder pushEncoder;

        private string faultReason;

        public DatabaseConnection(string workspaceName, IMetaPopulation metaPopulation, ITransport transport, IRanges<long> ranges, ICache cache = null)
        {
            this.WorkspaceName = workspaceName ?? throw new ArgumentNullException(nameof(workspaceName));
            this.MetaPopulation = metaPopulation ?? throw new ArgumentNullException(nameof(metaPopulation));
            this.transport = transport ?? throw new ArgumentNullException(nameof(transport));
            this.Ranges = ranges ?? throw new ArgumentNullException(nameof(ranges));

            if (cache != null)
            {
                if (!string.Equals(cache.WorkspaceName, workspaceName, StringComparison.Ordinal))
                {
                    throw new ArgumentException($"The cache holds the records of workspace '{cache.WorkspaceName}' and cannot serve a connection to workspace '{workspaceName}': the connections that share a cache are to one workspace name. Give this connection a cache of its own.", nameof(cache));
                }

                if (!ReferenceEquals(cache.MetaPopulation, metaPopulation))
                {
                    throw new ArgumentException("The cache types its records by another meta population than this connection: the connections that share a cache are built on one meta population instance. Give this connection a cache of its own, or build it on the cache's meta population.", nameof(cache));
                }
            }

            this.MetaFingerprint = metaPopulation.Fingerprint();
            this.cache = cache ?? new MemoryCache(workspaceName, metaPopulation);
            this.pushEncoder = new PushEncoder(this.cache, transport.UnitConvert, ranges);
        }

        public event EventHandler<RecordChangedEventArgs> RecordChanged;

        public string WorkspaceName { get; }

        public IMetaPopulation MetaPopulation { get; }

        public string MetaFingerprint { get; }

        public IRanges<long> Ranges { get; }

        public ICache Cache => this.cache;

        public string DatabaseId { get; private set; }

        public long? UserId { get; private set; }

        public IRecord GetRecord(long id) => this.cache.GetRecord(id);

        public long GetPermission(IClass @class, IOperandType operandType, Operations operation) => this.cache.GetPermission(@class, operandType, operation);

        public async Task<PullResult> PullAsync(Pull[] pulls, Procedure procedure = null)
        {
            this.ThrowIfFaulted();

            pulls ??= Array.Empty<Pull>();

            foreach (var pull in pulls)
            {
                if (pull.ObjectId < 0 || pull.Object?.Id < 0)
                {
                    throw new ArgumentException("Id is not in the database");
                }
            }

            var unitConvert = this.transport.UnitConvert;
            var pullRequest = this.Address(new PullRequest
            {
                p = procedure?.ToJson(unitConvert),
                l = pulls.Select(v => v.ToJson(unitConvert)).ToArray(),
            });

            var pullResponse = await this.transport.PullAsync(pullRequest);
            this.OnResponse(pullResponse);
            return await this.OnPullAsync(pullResponse);
        }

        public async Task<PullResult> PullAsync(string name, object args)
        {
            this.ThrowIfFaulted();

            // The arguments are the route's own; they carry no envelope, which the server
            // tolerates.
            var pullResponse = await this.transport.PullAsync(name, args);
            this.OnResponse(pullResponse);
            return await this.OnPullAsync(pullResponse);
        }

        public async Task<PushResult> PushAsync(PushNewObject[] newObjects, PushChangedObject[] changedObjects)
        {
            this.ThrowIfFaulted();

            var pushRequest = this.Address(new PushRequest
            {
                n = newObjects?.Select(this.pushEncoder.ToJson).ToArray(),
                o = changedObjects?.Select(this.pushEncoder.ToJson).ToArray(),
            });

            var pushResponse = await this.transport.PushAsync(pushRequest);
            this.OnResponse(pushResponse);
            return new PushResult(this.MetaPopulation, pushResponse);
        }

        public async Task<InvokeResult> InvokeAsync(Invocation[] invocations, InvokeOptions options = null)
        {
            this.ThrowIfFaulted();

            var invokeRequest = this.Address(new JsonInvokeRequest
            {
                l = invocations.Select(v => new JsonInvocation
                {
                    i = v.Id,
                    v = v.Version,
                    m = v.MethodType.Tag,
                }).ToArray(),
                o = options != null
                    ? new JsonInvokeOptions
                    {
                        c = options.ContinueOnError,
                        i = options.Isolated,
                    }
                    : null,
            });

            var invokeResponse = await this.transport.InvokeAsync(invokeRequest);
            this.OnResponse(invokeResponse);
            return new InvokeResult(this.MetaPopulation, invokeResponse);
        }

        public Task ClearAsync()
        {
            this.cache.Clear();
            return Task.CompletedTask;
        }

        private void ThrowIfFaulted()
        {
            if (this.faultReason != null)
            {
                throw new InvalidOperationException(this.faultReason);
            }
        }

        private T Address<T>(T request)
            where T : Request
        {
            request._w = this.WorkspaceName;
            request._f = this.MetaFingerprint;
            return request;
        }

        /// <summary>
        /// Checks the envelope of a response before anything of it is stored: the server must
        /// identify itself, serve this connection's workspace and meta, and stay the database
        /// and the user of the first response.
        /// </summary>
        private void OnResponse(Response response)
        {
            if (response == null)
            {
                throw new InvalidOperationException("The server sent no response.");
            }

            if (response._db == null || response._u == null || response._w == null || response._f == null)
            {
                throw new InvalidOperationException("The server did not identify itself: its response carries no database id, user id, workspace name or meta fingerprint. This connection needs a server that sends them with every response; upgrade the server.");
            }

            if (!string.Equals(response._w, this.WorkspaceName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"The server serves workspace '{response._w}' where this connection is for workspace '{this.WorkspaceName}'. Connect to the host that serves '{this.WorkspaceName}', or build the connection for '{response._w}'.{this.ServerSaid(response)}");
            }

            if (!string.Equals(response._f, this.MetaFingerprint, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"The server's workspace '{response._w}' has meta fingerprint {response._f} where this connection's meta population has {this.MetaFingerprint}: the client's workspace meta was generated from another version of the domain. Regenerate the workspace meta from the server's repository and rebuild the client.{this.ServerSaid(response)}");
            }

            if (this.DatabaseId == null)
            {
                this.cache.Bind(new CacheKey(response._db, response._u.Value, this.WorkspaceName, this.MetaFingerprint));
                this.DatabaseId = response._db;
                this.UserId = response._u;
                return;
            }

            if (!string.Equals(response._db, this.DatabaseId, StringComparison.Ordinal) || response._u != this.UserId)
            {
                this.Fault($"The connection was to user {this.UserId} of database '{this.DatabaseId}' and the server now answers as user {response._u} of database '{response._db}': the sign-in changed under the connection. Its cache is cleared; sign in again with a new connection.");
            }
        }

        private string ServerSaid(Response response) => string.IsNullOrWhiteSpace(response._e) ? string.Empty : $" The server said: {response._e}";

        private void Fault(string reason)
        {
            this.faultReason = reason;
            this.cache.Clear();
            throw new InvalidOperationException(reason);
        }

        private async Task<PullResult> OnPullAsync(PullResponse pullResponse)
        {
            if (!pullResponse.HasErrors)
            {
                await this.SyncAsync(pullResponse);
            }

            return new PullResult(this.MetaPopulation, this.transport.UnitConvert, pullResponse);
        }

        private async Task SyncAsync(PullResponse pullResponse)
        {
            var syncRequest = this.OnPullResponse(pullResponse);
            if (syncRequest.o.Length == 0)
            {
                return;
            }

            var syncResponse = await this.transport.SyncAsync(this.Address(syncRequest));
            this.OnResponse(syncResponse);
            this.ThrowIfRefused(syncResponse, "sync");
            var (accessRequest, replaced) = this.OnSyncResponse(syncResponse);

            if (accessRequest != null)
            {
                var accessResponse = await this.transport.AccessAsync(this.Address(accessRequest));
                this.OnResponse(accessResponse);
                this.ThrowIfRefused(accessResponse, "access");
                var permissionRequest = this.OnAccessResponse(accessResponse);
                if (permissionRequest != null)
                {
                    var permissionResponse = await this.transport.PermissionAsync(this.Address(permissionRequest));
                    this.OnResponse(permissionResponse);
                    this.ThrowIfRefused(permissionResponse, "permission");
                    this.OnPermissionResponse(permissionResponse);
                }
            }

            foreach (var record in replaced)
            {
                this.RecordChanged?.Invoke(this, new RecordChangedEventArgs(record));
            }
        }

        // A sync, access or permission response has no error channel of its own: an error
        // message on one is the server refusing the request.
        private void ThrowIfRefused(Response response, string call)
        {
            if (response.HasErrors)
            {
                throw new InvalidOperationException($"The server refused the {call} request: {response._e}");
            }
        }

        private SyncRequest OnPullResponse(PullResponse response) =>
            new SyncRequest
            {
                o = (response.p ?? Array.Empty<PullResponseObject>())
                    .Where(v =>
                    {
                        var record = this.cache.GetRecord(v.i);
                        if (record == null)
                        {
                            return true;
                        }

                        if (!record.Version.Equals(v.v))
                        {
                            return true;
                        }

                        if (!record.GrantIds.Equals(this.Ranges.Load(v.g)))
                        {
                            return true;
                        }

                        if (!record.RevocationIds.Equals(this.Ranges.Load(v.r)))
                        {
                            return true;
                        }

                        return false;
                    })
                    .Select(v => v.i).ToArray(),
            };

        private (AccessRequest AccessRequest, List<IRecord> Replaced) OnSyncResponse(SyncResponse syncResponse)
        {
            var ctx = new ResponseContext(this.cache);
            var replaced = new List<IRecord>();

            foreach (var syncResponseObject in syncResponse.o)
            {
                var record = new Record(this.cache, this.MetaPopulation, this.transport.UnitConvert, this.Ranges, ctx, syncResponseObject);
                var previous = this.cache.GetRecord(record.Id);

                if (this.cache.SetRecord(record) && previous != null)
                {
                    replaced.Add(record);
                }
            }

            if (ctx.MissingGrantIds.Count > 0 || ctx.MissingRevocationIds.Count > 0)
            {
                var accessRequest = new AccessRequest
                {
                    g = ctx.MissingGrantIds.ToArray(),
                    r = ctx.MissingRevocationIds.ToArray(),
                };

                return (accessRequest, replaced);
            }

            return (null, replaced);
        }

        private PermissionRequest OnAccessResponse(AccessResponse accessResponse)
        {
            HashSet<long> missingPermissionIds = null;

            if (accessResponse.g != null)
            {
                foreach (var accessResponseGrant in accessResponse.g)
                {
                    var permissionIds = this.Ranges.Load(accessResponseGrant.p);
                    this.cache.SetGrant(new Grant(accessResponseGrant.i, accessResponseGrant.v, permissionIds));
                    this.CollectMissingPermissions(permissionIds, ref missingPermissionIds);
                }
            }

            if (accessResponse.r != null)
            {
                foreach (var accessResponseRevocation in accessResponse.r)
                {
                    var permissionIds = this.Ranges.Load(accessResponseRevocation.p);
                    this.cache.SetRevocation(new Revocation(accessResponseRevocation.i, accessResponseRevocation.v, permissionIds));
                    this.CollectMissingPermissions(permissionIds, ref missingPermissionIds);
                }
            }

            return missingPermissionIds != null ? new PermissionRequest { p = missingPermissionIds.ToArray() } : null;
        }

        private void CollectMissingPermissions(IRange<long> permissionIds, ref HashSet<long> missingPermissionIds)
        {
            foreach (var permissionId in permissionIds)
            {
                if (this.cache.HasPermission(permissionId))
                {
                    continue;
                }

                missingPermissionIds ??= new HashSet<long>();
                missingPermissionIds.Add(permissionId);
            }
        }

        private void OnPermissionResponse(PermissionResponse permissionResponse)
        {
            if (permissionResponse.p == null)
            {
                return;
            }

            foreach (var permissionResponsePermission in permissionResponse.p)
            {
                var @class = (IClass)this.MetaPopulation.FindByTag(permissionResponsePermission.c);
                var metaObject = this.MetaPopulation.FindByTag(permissionResponsePermission.t);
                var operandType = (IOperandType)(metaObject as IRelationType)?.RoleType ?? (IMethodType)metaObject;
                var operation = (Operations)permissionResponsePermission.o;

                this.cache.SetPermission(new Permission(permissionResponsePermission.i, @class, operandType, operation));
            }
        }
    }
}
