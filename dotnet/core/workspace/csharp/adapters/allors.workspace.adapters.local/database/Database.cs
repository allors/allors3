// <copyright file="v.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Database;
    using Database.Domain;
    using Database.Security;
    using Meta;
    using Numbers;
    using AccessControl = Adapters.AccessControl;
    using IRoleType = Database.Meta.IRoleType;

    public class DatabaseConnection : Adapters.DatabaseConnection
    {
        private readonly Dictionary<long, AccessControl> accessControlById;
        private readonly IPermissionsCache permissionCache;
        private readonly ConcurrentDictionary<long, DatabaseRecord> recordsById;

        private readonly Func<IWorkspaceServices> servicesBuilder;
        private readonly Func<INumbers> numbersBuilder;

        public DatabaseConnection(Configuration configuration, IDatabase database, Func<IWorkspaceServices> servicesBuilder, Func<INumbers> numbersBuilder) : base(configuration)
        {
            this.Database = database;
            this.servicesBuilder = servicesBuilder;
            this.numbersBuilder = numbersBuilder;

            this.recordsById = new ConcurrentDictionary<long, DatabaseRecord>();
            this.permissionCache = this.Database.Services().PermissionsCache;
            this.accessControlById = new Dictionary<long, AccessControl>();
        }

        public long UserId { get; set; }

        internal IDatabase Database { get; }

        internal void Sync(IEnumerable<IObject> objects, IAccessControlLists accessControlLists)
        {
            // TODO: Prefetch objects
            static object GetRole(IObject @object, IRoleType roleType)
            {
                if (roleType.ObjectType.IsUnit)
                {
                    return @object.Strategy.GetUnitRole(roleType);
                }

                if (roleType.IsOne)
                {
                    return @object.Strategy.GetCompositeRole(roleType)?.Id;
                }

                var roles = @object.Strategy.GetCompositeRoles(roleType);
                return roles.Count > 0
                    ? @object.Strategy.GetCompositeRoles(roleType).Select(v => v.Id).ToArray()
                    : Array.Empty<long>();
            }

            foreach (var @object in objects)
            {
                var id = @object.Id;
                var databaseClass = @object.Strategy.Class;
                var roleTypes = databaseClass.DatabaseRoleTypes.Where(w => w.RelationType.WorkspaceNames.Length > 0);

                var workspaceClass = (IClass)this.Configuration.MetaPopulation.FindByTag(databaseClass.Tag);
                var roleByRoleType = roleTypes.ToDictionary(w =>
                        ((IRelationType)this.Configuration.MetaPopulation.FindByTag(w.RelationType.Tag)).RoleType,
                    w => GetRole(@object, w));

                var acl = accessControlLists[@object];

                var accessControls = acl.AccessControls
                    ?.Select(this.GetAccessControl)
                    .ToArray() ?? Array.Empty<AccessControl>();

                this.recordsById[id] = new DatabaseRecord(workspaceClass, id, @object.Strategy.ObjectVersion, roleByRoleType, acl.DeniedPermissionIds, accessControls);
            }
        }

        public override IWorkspace CreateWorkspace() => new Workspace(this, this.servicesBuilder(), this.numbersBuilder());

        public override Adapters.DatabaseRecord GetRecord(long id)
        {
            this.recordsById.TryGetValue(id, out var databaseObjects);
            return databaseObjects;
        }

        public override long GetPermission(IClass @class, IOperandType operandType, Operations operation)
        {
            var classId = this.Database.MetaPopulation.FindByTag(@class.Tag).Id;
            var operandId = this.Database.MetaPopulation.FindByTag(operandType.OperandTag).Id;

            long permission;
            var permissionCacheEntry = this.permissionCache.Get(classId);

            switch (operation)
            {
                case Operations.Read:
                    permissionCacheEntry.RoleReadPermissionIdByRelationTypeId.TryGetValue(operandId,
                        out permission);
                    break;
                case Operations.Write:
                    permissionCacheEntry.RoleWritePermissionIdByRelationTypeId.TryGetValue(operandId,
                        out permission);
                    break;
                default:
                    permissionCacheEntry.MethodExecutePermissionIdByMethodTypeId.TryGetValue(operandId,
                        out permission);
                    break;
            }

            return permission;
        }

        public override Adapters.DatabaseRecord OnDatabasePushResponse(IClass @class, long id)
        {
            var record = new DatabaseRecord(@class, id);
            this.recordsById[id] = record;
            return record;
        }

        internal IEnumerable<IObject> ObjectsToSync(Pull pull) =>
            pull.DatabaseObjects.Where(v =>
            {
                if (this.recordsById.TryGetValue(v.Id, out var databaseRoles))
                {
                    return v.Strategy.ObjectVersion != databaseRoles.Version;
                }

                return true;
            });

        private AccessControl GetAccessControl(IAccessControl accessControl)
        {
            if (!this.accessControlById.TryGetValue(accessControl.Strategy.ObjectId, out var acessControl))
            {
                acessControl = new AccessControl();
                this.accessControlById.Add(accessControl.Strategy.ObjectId, acessControl);
            }

            if (acessControl.Version == accessControl.Strategy.ObjectVersion)
            {
                return acessControl;
            }

            acessControl.Version = accessControl.Strategy.ObjectVersion;
            acessControl.PermissionIds = new HashSet<long>(accessControl.Permissions.Select(v => v.Id));

            return acessControl;
        }
    }
}
