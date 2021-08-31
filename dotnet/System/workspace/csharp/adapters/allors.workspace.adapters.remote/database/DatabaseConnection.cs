// <copyright file="RemoteDatabase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Invoke;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.Security;
    using Allors.Protocol.Json.Api.Sync;
    using Meta;
    using Ranges;

    public abstract class DatabaseConnection : Adapters.DatabaseConnection
    {
        private readonly Dictionary<long, DatabaseRecord> recordsById;

        private readonly Func<IWorkspaceServices> servicesBuilder;

        private readonly Dictionary<IClass, Dictionary<IOperandType, long>> readPermissionByOperandTypeByClass;
        private readonly Dictionary<IClass, Dictionary<IOperandType, long>> writePermissionByOperandTypeByClass;
        private readonly Dictionary<IClass, Dictionary<IOperandType, long>> executePermissionByOperandTypeByClass;

        protected DatabaseConnection(Adapters.Configuration configuration, IdGenerator idGenerator, Func<IWorkspaceServices> servicesBuilder, IRanges<long> ranges) : base(configuration, idGenerator)
        {
            this.Ranges = ranges;
            this.servicesBuilder = servicesBuilder;

            this.recordsById = new Dictionary<long, DatabaseRecord>();

            this.AccessControlById = new Dictionary<long, AccessControl>();
            this.Permissions = new HashSet<long>();

            this.readPermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, long>>();
            this.writePermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, long>>();
            this.executePermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, long>>();
        }

        internal Dictionary<long, AccessControl> AccessControlById { get; }

        internal ISet<long> Permissions { get; }

        public abstract IUnitConvert UnitConvert { get; }

        internal IRanges<long> Ranges { get; }

        protected abstract string UserId { get; }

        public override IWorkspace CreateWorkspace() => new Workspace(this, this.servicesBuilder(), this.Ranges);

        internal SyncRequest OnPullResponse(PullResponse response) =>
            new SyncRequest
            {
                o = response.p
                    .Where(v =>
                    {
                        if (!this.recordsById.TryGetValue(v.i, out var @record))
                        {
                            return true;
                        }

                        if (!@record.Version.Equals(v.v))
                        {
                            return true;
                        }

                        if (!@record.AccessControlIds.Equals(this.Ranges.Load(v.a)))
                        {
                            return true;
                        }

                        if (!@record.DeniedPermissions.Equals(this.Ranges.Load(v.d)))
                        {
                            return true;
                        }

                        // TODO: Use smarter updates for DeniedPermissions

                        return false;
                    })
                    .Select(v => v.i).ToArray()
            };

        internal SecurityRequest OnSyncResponse(SyncResponse syncResponse)
        {
            var ctx = new ResponseContext(this);

            foreach (var syncResponseObject in syncResponse.o)
            {
                var databaseObjects = DatabaseRecord.FromResponse(this, ctx, syncResponseObject);
                this.recordsById[databaseObjects.Id] = databaseObjects;
            }

            if (ctx.MissingAccessControlIds.Count > 0 || ctx.MissingPermissionIds.Count > 0)
            {
                return new SecurityRequest
                {
                    a = ctx.MissingAccessControlIds.Select(v => v).ToArray(),
                    p = ctx.MissingPermissionIds.Select(v => v).ToArray()
                };
            }

            return null;
        }

        internal SecurityRequest SecurityResponse(SecurityResponse securityResponse)
        {
            if (securityResponse.p != null)
            {
                foreach (var syncResponsePermission in securityResponse.p)
                {
                    var id = syncResponsePermission[0];
                    var @class = (IClass)this.Configuration.MetaPopulation.FindByTag((int)syncResponsePermission[1]);
                    var metaObject = this.Configuration.MetaPopulation.FindByTag((int)syncResponsePermission[2]);
                    var operandType = (IOperandType)(metaObject as IRelationType)?.RoleType ?? (IMethodType)metaObject;
                    var operation = (Operations)syncResponsePermission[3];

                    this.Permissions.Add(id);

                    switch (operation)
                    {
                        case Operations.Read:
                            if (!this.readPermissionByOperandTypeByClass.TryGetValue(@class, out var readPermissionByOperandType))
                            {
                                readPermissionByOperandType = new Dictionary<IOperandType, long>();
                                this.readPermissionByOperandTypeByClass[@class] = readPermissionByOperandType;
                            }

                            readPermissionByOperandType[operandType] = id;

                            break;

                        case Operations.Write:
                            if (!this.writePermissionByOperandTypeByClass.TryGetValue(@class, out var writePermissionByOperandType))
                            {
                                writePermissionByOperandType = new Dictionary<IOperandType, long>();
                                this.writePermissionByOperandTypeByClass[@class] = writePermissionByOperandType;
                            }

                            writePermissionByOperandType[operandType] = id;

                            break;

                        case Operations.Execute:
                            if (!this.executePermissionByOperandTypeByClass.TryGetValue(@class, out var executePermissionByOperandType))
                            {
                                executePermissionByOperandType = new Dictionary<IOperandType, long>();
                                this.executePermissionByOperandTypeByClass[@class] = executePermissionByOperandType;
                            }

                            executePermissionByOperandType[operandType] = id;

                            break;
                        case Operations.Create:
                            throw new NotSupportedException("Create not supported");
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            HashSet<long> missingPermissionIds = null;
            if (securityResponse.a != null)
            {
                foreach (var syncResponseAccessControl in securityResponse.a)
                {
                    var id = syncResponseAccessControl.i;
                    var version = syncResponseAccessControl.v;
                    var permissionIds = this.Ranges.Load(syncResponseAccessControl.p);
                    this.AccessControlById[id] = new AccessControl { Version = version, PermissionIds = this.Ranges.Load(permissionIds) };

                    foreach (var permissionId in permissionIds)
                    {
                        if (this.Permissions.Contains(permissionId))
                        {
                            continue;
                        }

                        missingPermissionIds ??= new HashSet<long>();
                        missingPermissionIds.Add(permissionId);
                    }
                }
            }

            return missingPermissionIds != null ? new SecurityRequest { p = missingPermissionIds.ToArray() } : null;
        }

        public override long GetPermission(IClass @class, IOperandType operandType, Operations operation)
        {
            switch (operation)
            {
                case Operations.Read:
                    if (this.readPermissionByOperandTypeByClass.TryGetValue(@class,
                        out var readPermissionByOperandType) && readPermissionByOperandType.TryGetValue(operandType, out var readPermission))
                    {
                        return readPermission;
                    }

                    return 0;

                case Operations.Write:
                    if (this.writePermissionByOperandTypeByClass.TryGetValue(@class,
                        out var writePermissionByOperandType) && writePermissionByOperandType.TryGetValue(operandType, out var writePermission))
                    {
                        return writePermission;
                    }

                    return 0;

                case Operations.Execute:
                    if (this.executePermissionByOperandTypeByClass.TryGetValue(@class,
                        out var executePermissionByOperandType) && executePermissionByOperandType.TryGetValue(operandType, out var executePermission))
                    {
                        return executePermission;
                    }

                    return 0;

                case Operations.Create:
                    throw new NotSupportedException("Create is not supported");

                default:
                    throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        public override Adapters.DatabaseRecord GetRecord(long id)
        {
            this.recordsById.TryGetValue(id, out var databaseObjects);
            return databaseObjects;
        }

        public abstract Task<PullResponse> Pull(PullRequest pullRequest);

        public abstract Task<SyncResponse> Sync(SyncRequest syncRequest);

        public abstract Task<PushResponse> Push(PushRequest pushRequest);

        public abstract Task<InvokeResponse> Invoke(InvokeRequest invokeRequest);

        public abstract Task<SecurityResponse> Security(SecurityRequest securityRequest);
    }
}
