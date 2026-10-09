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
    /// holds, requests the grants and revocations it lacks or holds at another version, and then
    /// the permissions those name. The records and access information belong to the connection.
    /// Every request names the workspace and the meta fingerprint the connection is built for;
    /// every response is checked against them before anything is
    /// stored, and the first response tells which database and user the connection is to, which
    /// no later response may change. Start the next call after the previous call's task completes.
    /// </summary>
    public sealed class DatabaseConnection : IDatabaseConnection
    {
        private readonly ITransport transport;
        private readonly PushEncoder pushEncoder;
        private readonly Dictionary<long, IRecord> recordById = new Dictionary<long, IRecord>();
        private readonly Dictionary<long, Grant> grantById = new Dictionary<long, Grant>();
        private readonly Dictionary<long, Revocation> revocationById = new Dictionary<long, Revocation>();
        private readonly Dictionary<long, Permission> permissionById = new Dictionary<long, Permission>();
        private readonly Dictionary<(IClass Class, IOperandType OperandType, Operations Operation), long> permissionIdByOperation = new Dictionary<(IClass, IOperandType, Operations), long>();

        private string faultReason;

        public DatabaseConnection(string workspaceName, IMetaPopulation metaPopulation, ITransport transport, IRanges<long> ranges)
        {
            this.WorkspaceName = workspaceName ?? throw new ArgumentNullException(nameof(workspaceName));
            this.MetaPopulation = metaPopulation ?? throw new ArgumentNullException(nameof(metaPopulation));
            this.transport = transport ?? throw new ArgumentNullException(nameof(transport));
            this.Ranges = ranges ?? throw new ArgumentNullException(nameof(ranges));

            this.MetaFingerprint = metaPopulation.Fingerprint();
            this.pushEncoder = new PushEncoder(this, transport.UnitConvert, ranges);
        }

        public event EventHandler<RecordChangedEventArgs> RecordChanged;

        public string WorkspaceName { get; }

        public IMetaPopulation MetaPopulation { get; }

        public string MetaFingerprint { get; }

        public IRanges<long> Ranges { get; }

        public string DatabaseId { get; private set; }

        public long? UserId { get; private set; }

        public IRecord GetRecord(long id) => this.recordById.TryGetValue(id, out var record) ? record : null;

        public long GetPermission(IClass @class, IOperandType operandType, Operations operation) =>
            this.permissionIdByOperation.TryGetValue((@class, operandType, operation), out var permissionId) ? permissionId : 0;

        internal Grant GetGrant(long id) => this.grantById.TryGetValue(id, out var grant) ? grant : null;

        internal Revocation GetRevocation(long id) => this.revocationById.TryGetValue(id, out var revocation) ? revocation : null;

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

        private void ClearRecords()
        {
            this.recordById.Clear();
            this.grantById.Clear();
            this.revocationById.Clear();
            this.permissionById.Clear();
            this.permissionIdByOperation.Clear();
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
                this.DatabaseId = response._db;
                this.UserId = response._u;
            }
            else if (!string.Equals(response._db, this.DatabaseId, StringComparison.Ordinal) || response._u != this.UserId)
            {
                this.faultReason = $"The connection was to user {this.UserId} of database '{this.DatabaseId}' and the server now answers as user {response._u} of database '{response._db}': the sign-in changed under the connection. Its records are cleared; sign in again with a new connection.";
                this.ClearRecords();
                throw new InvalidOperationException(this.faultReason);
            }
        }

        private string ServerSaid(Response response) => string.IsNullOrWhiteSpace(response._e) ? string.Empty : $" The server said: {response._e}";

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
            var ctx = new ResponseContext(this);
            var replaced = new List<IRecord>();

            // The objects to bring up to date: absent from the connection, or held at another
            // version or with other grants or revocations than the pull advertises.
            var staleObjectIds = this.StaleObjectIds(pullResponse);
            if (staleObjectIds.Length > 0)
            {
                var syncResponse = await this.transport.SyncAsync(this.Address(new SyncRequest { o = staleObjectIds }));
                this.OnResponse(syncResponse);
                this.ThrowIfRefused(syncResponse, "sync");
                this.StoreRecords(syncResponse.o, ctx, replaced);
            }

            // The grants and revocations to bring up to date: the ones the new records name
            // that the connection lacks, and the ones the pull advertises at another version
            // than the connection holds, or does not hold at all.
            this.CollectStaleAccess(pullResponse, ctx);

            if (ctx.MissingGrantIds.Count > 0 || ctx.MissingRevocationIds.Count > 0)
            {
                HashSet<long> missingPermissionIds = null;

                var accessRequest = new AccessRequest
                {
                    g = ctx.MissingGrantIds.ToArray(),
                    r = ctx.MissingRevocationIds.ToArray(),
                };

                var accessResponse = await this.transport.AccessAsync(this.Address(accessRequest));
                this.OnResponse(accessResponse);
                this.ThrowIfRefused(accessResponse, "access");
                this.StoreAccess(accessResponse.g, accessResponse.r, ref missingPermissionIds);

                if (missingPermissionIds != null)
                {
                    var permissionResponse = await this.transport.PermissionAsync(this.Address(new PermissionRequest { p = missingPermissionIds.ToArray() }));
                    this.OnResponse(permissionResponse);
                    this.ThrowIfRefused(permissionResponse, "permission");
                    this.StorePermissions(permissionResponse.p);
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

        private long[] StaleObjectIds(PullResponse response) =>
            (response.p ?? Array.Empty<PullResponseObject>())
                .Where(v =>
                {
                    var record = this.GetRecord(v.i);
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
                .Select(v => v.i).ToArray();

        private void CollectStaleAccess(PullResponse pullResponse, ResponseContext ctx)
        {
            if (pullResponse.g != null)
            {
                foreach (var pair in pullResponse.g)
                {
                    var grant = this.GetGrant(pair[0]);
                    if (grant == null || grant.Version != pair[1])
                    {
                        ctx.MissingGrantIds.Add(pair[0]);
                    }
                }
            }

            if (pullResponse.r != null)
            {
                foreach (var pair in pullResponse.r)
                {
                    var revocation = this.GetRevocation(pair[0]);
                    if (revocation == null || revocation.Version != pair[1])
                    {
                        ctx.MissingRevocationIds.Add(pair[0]);
                    }
                }
            }
        }

        private void StoreRecords(IEnumerable<SyncResponseObject> syncResponseObjects, ResponseContext ctx, List<IRecord> replaced)
        {
            if (syncResponseObjects == null)
            {
                return;
            }

            foreach (var syncResponseObject in syncResponseObjects)
            {
                var record = new Record(this, this.MetaPopulation, this.transport.UnitConvert, this.Ranges, ctx, syncResponseObject);
                var previous = this.GetRecord(record.Id);

                if (previous != null && record.Version < previous.Version)
                {
                    continue;
                }

                this.recordById[record.Id] = record;
                if (previous != null)
                {
                    replaced.Add(record);
                }
            }
        }

        private void StoreAccess(IEnumerable<AccessResponseGrant> grants, IEnumerable<AccessResponseRevocation> revocations, ref HashSet<long> missingPermissionIds)
        {
            if (grants != null)
            {
                foreach (var accessResponseGrant in grants)
                {
                    var permissionIds = this.Ranges.Load(accessResponseGrant.p);
                    var previous = this.GetGrant(accessResponseGrant.i);
                    if (previous == null || accessResponseGrant.v >= previous.Version)
                    {
                        this.grantById[accessResponseGrant.i] = new Grant(accessResponseGrant.i, accessResponseGrant.v, permissionIds);
                    }

                    this.CollectMissingPermissions(permissionIds, ref missingPermissionIds);
                }
            }

            if (revocations != null)
            {
                foreach (var accessResponseRevocation in revocations)
                {
                    var permissionIds = this.Ranges.Load(accessResponseRevocation.p);
                    var previous = this.GetRevocation(accessResponseRevocation.i);
                    if (previous == null || accessResponseRevocation.v >= previous.Version)
                    {
                        this.revocationById[accessResponseRevocation.i] = new Revocation(accessResponseRevocation.i, accessResponseRevocation.v, permissionIds);
                    }

                    this.CollectMissingPermissions(permissionIds, ref missingPermissionIds);
                }
            }
        }

        private void CollectMissingPermissions(IRange<long> permissionIds, ref HashSet<long> missingPermissionIds)
        {
            foreach (var permissionId in permissionIds)
            {
                if (this.permissionById.ContainsKey(permissionId))
                {
                    continue;
                }

                missingPermissionIds ??= new HashSet<long>();
                missingPermissionIds.Add(permissionId);
            }
        }

        private void StorePermissions(IEnumerable<PermissionResponsePermission> permissions)
        {
            if (permissions == null)
            {
                return;
            }

            foreach (var permissionResponsePermission in permissions)
            {
                var @class = (IClass)this.MetaPopulation.FindByTag(permissionResponsePermission.c);
                var metaObject = this.MetaPopulation.FindByTag(permissionResponsePermission.t);
                var operandType = (IOperandType)(metaObject as IRelationType)?.RoleType ?? (IMethodType)metaObject;
                var operation = (Operations)permissionResponsePermission.o;

                var permission = new Permission(permissionResponsePermission.i, @class, operandType, operation);
                this.permissionById[permission.Id] = permission;
                this.permissionIdByOperation[(permission.Class, permission.OperandType, permission.Operation)] = permission.Id;
            }
        }
    }
}
