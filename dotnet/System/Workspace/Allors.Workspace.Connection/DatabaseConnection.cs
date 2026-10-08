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
    /// the permissions those name; with a persistence provider it restores from there first what
    /// the provider holds at the versions the pull advertises, and stores what the server sent
    /// before the pull returns. Every request names the workspace and the meta fingerprint the
    /// connection is built for; every response is checked against them before anything is
    /// stored, and the first response tells which database and user the connection is to, which
    /// no later response may change. Use it from one thread at a time.
    /// </summary>
    public sealed class DatabaseConnection : IDatabaseConnection
    {
        private readonly ITransport transport;
        private readonly ICache cache;
        private readonly IPersistenceProvider persistence;
        private readonly PushEncoder pushEncoder;

        private string faultReason;

        public DatabaseConnection(string workspaceName, IMetaPopulation metaPopulation, ITransport transport, IRanges<long> ranges, ICache cache = null, IPersistenceProvider persistence = null)
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
            this.persistence = persistence;
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
            await this.OnResponseAsync(pullResponse);
            return await this.OnPullAsync(pullResponse);
        }

        public async Task<PullResult> PullAsync(string name, object args)
        {
            this.ThrowIfFaulted();

            // The arguments are the route's own; they carry no envelope, which the server
            // tolerates.
            var pullResponse = await this.transport.PullAsync(name, args);
            await this.OnResponseAsync(pullResponse);
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
            await this.OnResponseAsync(pushResponse);
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
            await this.OnResponseAsync(invokeResponse);
            return new InvokeResult(this.MetaPopulation, invokeResponse);
        }

        public async Task ClearAsync()
        {
            var key = this.cache.Key;
            this.cache.Clear();

            if (this.persistence != null && key != null)
            {
                await this.persistence.ClearAsync(key);
            }
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
        private async Task OnResponseAsync(Response response)
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
                this.faultReason = $"The connection was to user {this.UserId} of database '{this.DatabaseId}' and the server now answers as user {response._u} of database '{response._db}': the sign-in changed under the connection. Its cache is cleared; sign in again with a new connection.";
                await this.ClearAsync();
                throw new InvalidOperationException(this.faultReason);
            }

            // Binds a cache that is not bound yet, new or cleared, and checks a bound one: a
            // shared cache refuses the connection of another user here.
            this.cache.Bind(new CacheKey(response._db, response._u.Value, this.WorkspaceName, this.MetaFingerprint));
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
            var key = this.cache.Key;
            var ctx = new ResponseContext(this.cache);
            var replaced = new List<IRecord>();
            var received = this.persistence != null ? new CacheEntries() : null;

            // The objects to bring up to date: absent from the cache, or held at another
            // version or with other grants or revocations than the pull advertises. The
            // provider's copy is taken when it is the one the pull advertises; the server is
            // asked for the rest.
            var staleObjectIds = this.StaleObjectIds(pullResponse);
            if (staleObjectIds.Length > 0 && this.persistence != null)
            {
                var loaded = await this.persistence.LoadAsync(key, new CacheEntryIds { Objects = staleObjectIds });
                if (loaded?.Objects != null)
                {
                    var advertisedById = pullResponse.p.ToDictionary(v => v.i);
                    var accepted = loaded.Objects.Where(v =>
                        advertisedById.TryGetValue(v.i, out var advertised) &&
                        v.v == advertised.v &&
                        this.Ranges.Load(v.g).Equals(this.Ranges.Load(advertised.g)) &&
                        this.Ranges.Load(v.r).Equals(this.Ranges.Load(advertised.r)));
                    this.StoreRecords(accepted, ctx, replaced);
                }

                staleObjectIds = this.StaleObjectIds(pullResponse);
            }

            if (staleObjectIds.Length > 0)
            {
                var syncResponse = await this.transport.SyncAsync(this.Address(new SyncRequest { o = staleObjectIds }));
                await this.OnResponseAsync(syncResponse);
                this.ThrowIfRefused(syncResponse, "sync");
                this.StoreRecords(syncResponse.o, ctx, replaced);

                if (received != null)
                {
                    received.Objects = syncResponse.o;
                }
            }

            // The grants and revocations to bring up to date: the ones the new records name
            // that the cache lacks, and the ones the pull advertises at another version than
            // the cache holds, or does not hold at all.
            this.CollectStaleAccess(pullResponse, ctx);

            if (ctx.MissingGrantIds.Count > 0 || ctx.MissingRevocationIds.Count > 0)
            {
                HashSet<long> missingPermissionIds = null;

                if (this.persistence != null)
                {
                    var loaded = await this.persistence.LoadAsync(key, new CacheEntryIds { Grants = ctx.MissingGrantIds.ToArray(), Revocations = ctx.MissingRevocationIds.ToArray() });

                    var versionByGrant = ToVersionById(pullResponse.g);
                    var versionByRevocation = ToVersionById(pullResponse.r);
                    var acceptedGrants = (loaded?.Grants ?? Array.Empty<AccessResponseGrant>()).Where(v => versionByGrant.TryGetValue(v.i, out var version) && version == v.v).ToArray();
                    var acceptedRevocations = (loaded?.Revocations ?? Array.Empty<AccessResponseRevocation>()).Where(v => versionByRevocation.TryGetValue(v.i, out var version) && version == v.v).ToArray();

                    this.StoreAccess(acceptedGrants, acceptedRevocations, ref missingPermissionIds);
                    ctx.MissingGrantIds.ExceptWith(acceptedGrants.Select(v => v.i));
                    ctx.MissingRevocationIds.ExceptWith(acceptedRevocations.Select(v => v.i));
                }

                if (ctx.MissingGrantIds.Count > 0 || ctx.MissingRevocationIds.Count > 0)
                {
                    var accessRequest = new AccessRequest
                    {
                        g = ctx.MissingGrantIds.ToArray(),
                        r = ctx.MissingRevocationIds.ToArray(),
                    };

                    var accessResponse = await this.transport.AccessAsync(this.Address(accessRequest));
                    await this.OnResponseAsync(accessResponse);
                    this.ThrowIfRefused(accessResponse, "access");
                    this.StoreAccess(accessResponse.g, accessResponse.r, ref missingPermissionIds);

                    if (received != null)
                    {
                        received.Grants = accessResponse.g;
                        received.Revocations = accessResponse.r;
                    }
                }

                if (missingPermissionIds != null)
                {
                    if (this.persistence != null)
                    {
                        var loaded = await this.persistence.LoadAsync(key, new CacheEntryIds { Permissions = missingPermissionIds.ToArray() });
                        if (loaded?.Permissions != null)
                        {
                            this.StorePermissions(loaded.Permissions);
                            missingPermissionIds.ExceptWith(loaded.Permissions.Select(v => v.i));
                        }
                    }

                    if (missingPermissionIds.Count > 0)
                    {
                        var permissionResponse = await this.transport.PermissionAsync(this.Address(new PermissionRequest { p = missingPermissionIds.ToArray() }));
                        await this.OnResponseAsync(permissionResponse);
                        this.ThrowIfRefused(permissionResponse, "permission");
                        this.StorePermissions(permissionResponse.p);

                        if (received != null)
                        {
                            received.Permissions = permissionResponse.p;
                        }
                    }
                }
            }

            // What the server sent is kept before the pull returns.
            if (received != null && !received.IsEmpty)
            {
                await this.persistence.StoreAsync(key, received);
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

        private static Dictionary<long, long> ToVersionById(long[][] pairs)
        {
            var versionById = new Dictionary<long, long>();
            if (pairs != null)
            {
                foreach (var pair in pairs)
                {
                    versionById[pair[0]] = pair[1];
                }
            }

            return versionById;
        }

        private long[] StaleObjectIds(PullResponse response) =>
            (response.p ?? Array.Empty<PullResponseObject>())
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
                .Select(v => v.i).ToArray();

        private void CollectStaleAccess(PullResponse pullResponse, ResponseContext ctx)
        {
            if (pullResponse.g != null)
            {
                foreach (var pair in pullResponse.g)
                {
                    var grant = this.cache.GetGrant(pair[0]);
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
                    var revocation = this.cache.GetRevocation(pair[0]);
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
                var record = new Record(this.cache, this.MetaPopulation, this.transport.UnitConvert, this.Ranges, ctx, syncResponseObject);
                var previous = this.cache.GetRecord(record.Id);

                if (this.cache.SetRecord(record) && previous != null)
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
                    this.cache.SetGrant(new Grant(accessResponseGrant.i, accessResponseGrant.v, permissionIds));
                    this.CollectMissingPermissions(permissionIds, ref missingPermissionIds);
                }
            }

            if (revocations != null)
            {
                foreach (var accessResponseRevocation in revocations)
                {
                    var permissionIds = this.Ranges.Load(accessResponseRevocation.p);
                    this.cache.SetRevocation(new Revocation(accessResponseRevocation.i, accessResponseRevocation.v, permissionIds));
                    this.CollectMissingPermissions(permissionIds, ref missingPermissionIds);
                }
            }
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

                this.cache.SetPermission(new Permission(permissionResponsePermission.i, @class, operandType, operation));
            }
        }
    }
}
