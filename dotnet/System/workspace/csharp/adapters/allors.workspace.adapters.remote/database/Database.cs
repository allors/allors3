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
    using Collections;
    using Meta;
    using Numbers;

    public abstract class DatabaseConnection : Adapters.DatabaseConnection
    {
        private readonly Dictionary<long, DatabaseRecord> recordsById;

        private readonly Dictionary<IClass, Dictionary<IOperandType, long>> readPermissionByOperandTypeByClass;
        private readonly Dictionary<IClass, Dictionary<IOperandType, long>> writePermissionByOperandTypeByClass;
        private readonly Dictionary<IClass, Dictionary<IOperandType, long>> executePermissionByOperandTypeByClass;

        private readonly WorkspaceIdGenerator workspaceIdGenerator;
        private readonly Func<IWorkspaceServices> servicesBuilder;

        protected DatabaseConnection(Adapters.Configuration configuration, Func<IWorkspaceServices> servicesBuilder, WorkspaceIdGenerator workspaceIdGenerator, INumbers numbers) : base(configuration)
        {
            this.Numbers = numbers;
            this.workspaceIdGenerator = workspaceIdGenerator;
            this.servicesBuilder = servicesBuilder;

            this.recordsById = new Dictionary<long, DatabaseRecord>();

            this.AccessControlById = new Dictionary<long, AccessControl>();
            this.Permissions = new HashSet<long>();

            this.readPermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, long>>();
            this.writePermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, long>>();
            this.executePermissionByOperandTypeByClass = new Dictionary<IClass, Dictionary<IOperandType, long>>();
        }

        public abstract IUnitConvert UnitConvert { get; }

        internal INumbers Numbers { get; }

        public string UserId { get; protected set; }

        internal ISet<long> Permissions { get; }

        internal Dictionary<long, AccessControl> AccessControlById { get; }

        public override Adapters.DatabaseRecord OnPushResponse(IClass @class, long id)
        {
            var record = new DatabaseRecord(this, @class, id);
            this.recordsById[record.Id] = record;
            return record;
        }

        internal SecurityRequest OnSyncResponse(SyncResponse syncResponse)
        {
            var ctx = new ResponseContext(this);
            foreach (var syncResponseObject in syncResponse.o)
            {
                var databaseObjects = new DatabaseRecord(this, ctx, syncResponseObject);
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
                            if (!this.readPermissionByOperandTypeByClass.TryGetValue(@class,
                                out var readPermissionByOperandType))
                            {
                                readPermissionByOperandType = new Dictionary<IOperandType, long>();
                                this.readPermissionByOperandTypeByClass[@class] = readPermissionByOperandType;
                            }

                            readPermissionByOperandType[operandType] = id;

                            break;

                        case Operations.Write:
                            if (!this.writePermissionByOperandTypeByClass.TryGetValue(@class,
                                out var writePermissionByOperandType))
                            {
                                writePermissionByOperandType = new Dictionary<IOperandType, long>();
                                this.writePermissionByOperandTypeByClass[@class] = writePermissionByOperandType;
                            }

                            writePermissionByOperandType[operandType] = id;

                            break;

                        case Operations.Execute:
                            if (!this.executePermissionByOperandTypeByClass.TryGetValue(@class,
                                out var executePermissionByOperandType))
                            {
                                executePermissionByOperandType = new Dictionary<IOperandType, long>();
                                this.executePermissionByOperandTypeByClass[@class] = executePermissionByOperandType;
                            }

                            executePermissionByOperandType[operandType] = id;

                            break;
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
                    var permissionsIds = syncResponseAccessControl.p
                        ?.Select(v =>
                        {
                            if (this.Permissions.Contains(v))
                            {
                                return v;
                            }

                            (missingPermissionIds ??= new HashSet<long>()).Add(v);

                            return v;
                        });

                    var permissionIdSet = permissionsIds != null
                        ? (ISet<long>)new HashSet<long>(permissionsIds)
                        : EmptySet<long>.Instance;

                    this.AccessControlById[id] = new AccessControl { Version = version, PermissionIds = permissionIdSet };
                }
            }

            if (missingPermissionIds != null)
            {
                return new SecurityRequest { p = missingPermissionIds.ToArray() };
            }

            return null;
        }

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

                        if (!this.Numbers.AreEqual(@record.AccessControlIds, v.a))
                        {
                            return true;
                        }

                        if (!this.Numbers.AreEqual(@record.DeniedPermissions, v.d))
                        {
                            return true;
                        }

                        // TODO: Use smarter updates for DeniedPermissions

                        return false;
                    })
                    .Select(v => v.i).ToArray()
            };

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

                default:
                    if (this.executePermissionByOperandTypeByClass.TryGetValue(@class,
                        out var executePermissionByOperandType) && executePermissionByOperandType.TryGetValue(operandType, out var executePermission))
                    {
                        return executePermission;
                    }

                    return 0;
            }
        }

        public override IWorkspace CreateWorkspace() => new Workspace(this, this.servicesBuilder(), this.Numbers, this.workspaceIdGenerator);

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
