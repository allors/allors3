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
    /// Use it from one thread at a time.
    /// </summary>
    public sealed class DatabaseConnection : IDatabaseConnection
    {
        private readonly ITransport transport;
        private readonly ICache cache;
        private readonly PushEncoder pushEncoder;

        public DatabaseConnection(string workspaceName, IMetaPopulation metaPopulation, ITransport transport, IRanges<long> ranges, ICache cache = null)
        {
            this.WorkspaceName = workspaceName ?? throw new ArgumentNullException(nameof(workspaceName));
            this.MetaPopulation = metaPopulation ?? throw new ArgumentNullException(nameof(metaPopulation));
            this.transport = transport ?? throw new ArgumentNullException(nameof(transport));
            this.Ranges = ranges ?? throw new ArgumentNullException(nameof(ranges));
            this.cache = cache ?? new Cache();
            this.pushEncoder = new PushEncoder(this.cache, transport.UnitConvert, ranges);
        }

        public event EventHandler<RecordChangedEventArgs> RecordChanged;

        public string WorkspaceName { get; }

        public IMetaPopulation MetaPopulation { get; }

        public IRanges<long> Ranges { get; }

        public long? DatabaseId { get; private set; }

        public long? UserId { get; private set; }

        public IRecord GetRecord(long id) => this.cache.GetRecord(id);

        public long GetPermission(IClass @class, IOperandType operandType, Operations operation) => this.cache.GetPermission(@class, operandType, operation);

        public async Task<PullResult> PullAsync(Pull[] pulls, Procedure procedure = null)
        {
            pulls ??= Array.Empty<Pull>();

            foreach (var pull in pulls)
            {
                if (pull.ObjectId < 0 || pull.Object?.Id < 0)
                {
                    throw new ArgumentException("Id is not in the database");
                }
            }

            var unitConvert = this.transport.UnitConvert;
            var pullRequest = new PullRequest
            {
                p = procedure?.ToJson(unitConvert),
                l = pulls.Select(v => v.ToJson(unitConvert)).ToArray(),
            };

            var pullResponse = await this.transport.PullAsync(pullRequest);
            return await this.OnPullAsync(pullResponse);
        }

        public async Task<PullResult> PullAsync(string name, object args)
        {
            var pullResponse = await this.transport.PullAsync(name, args);
            return await this.OnPullAsync(pullResponse);
        }

        public async Task<PushResult> PushAsync(PushNewObject[] newObjects, PushChangedObject[] changedObjects)
        {
            var pushRequest = new PushRequest
            {
                n = newObjects?.Select(this.pushEncoder.ToJson).ToArray(),
                o = changedObjects?.Select(this.pushEncoder.ToJson).ToArray(),
            };

            var pushResponse = await this.transport.PushAsync(pushRequest);
            return new PushResult(this.MetaPopulation, pushResponse);
        }

        public async Task<InvokeResult> InvokeAsync(Invocation[] invocations, InvokeOptions options = null)
        {
            var invokeRequest = new JsonInvokeRequest
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
            };

            var invokeResponse = await this.transport.InvokeAsync(invokeRequest);
            return new InvokeResult(this.MetaPopulation, invokeResponse);
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

            var syncResponse = await this.transport.SyncAsync(syncRequest);
            var (accessRequest, replaced) = this.OnSyncResponse(syncResponse);

            if (accessRequest != null)
            {
                var accessResponse = await this.transport.AccessAsync(accessRequest);
                var permissionRequest = this.OnAccessResponse(accessResponse);
                if (permissionRequest != null)
                {
                    var permissionResponse = await this.transport.PermissionAsync(permissionRequest);
                    this.OnPermissionResponse(permissionResponse);
                }
            }

            foreach (var record in replaced)
            {
                this.RecordChanged?.Invoke(this, new RecordChangedEventArgs(record));
            }
        }

        private SyncRequest OnPullResponse(PullResponse response) =>
            new SyncRequest
            {
                o = (response.p ?? Array.Empty<PullResponseObject>())
                    .Where(v =>
                    {
                        if (!(this.cache.GetRecord(v.i) is Record record))
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
                this.cache.SetRecord(record);

                if (previous != null)
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
