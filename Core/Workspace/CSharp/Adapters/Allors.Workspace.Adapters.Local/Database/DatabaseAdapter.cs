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
    using IRoleType = Database.Meta.IRoleType;

    internal class DatabaseAdapter
    {
        private readonly IPermissionsCache permissionCache;

        internal DatabaseAdapter(IMetaPopulation metaPopulation, IDatabase database)
        {
            this.MetaPopulation = metaPopulation;
            this.Database = database;
            this.ObjectsById = new ConcurrentDictionary<long, DatabaseRecord>();
            this.WorkspaceIdGenerator = new WorkspaceIdGenerator();
            this.permissionCache = this.Database.Context().PermissionsCache;
            this.AccessControlById = new Dictionary<long, AccessControl>();
        }

        private IMetaPopulation MetaPopulation { get; }

        private Database.Meta.IMetaPopulation DatabaseMetaPopulation => this.Database.MetaPopulation;

        public IDatabase Database { get; }

        private ConcurrentDictionary<long, DatabaseRecord> ObjectsById { get; }

        internal WorkspaceIdGenerator WorkspaceIdGenerator { get; }

        private Dictionary<long, AccessControl> AccessControlById { get; set; }

        internal void Sync(IEnumerable<IObject> objects, IAccessControlLists acls)
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
                return roles.Count > 0 ? @object.Strategy.GetCompositeRoles(roleType).Select(v => v.Id).ToArray() : Array.Empty<long>();
            }

            foreach (var @object in objects)
            {
                var id = @object.Id;
                var databaseClass = @object.Strategy.Class;
                var roleTypes = databaseClass.DatabaseRoleTypes.Where(w => w.RelationType.WorkspaceNames.Length > 0);

                var workspaceClass = (IClass)this.MetaPopulation.FindByTag(databaseClass.Tag);
                var roleByRoleType = roleTypes.ToDictionary(w =>
                        ((IRelationType)this.MetaPopulation.FindByTag(w.RelationType.Tag)).RoleType,
                    w => GetRole(@object, w));

                var acl = acls[@object];

                var deniedPermissions = acl.DeniedPermissionIds?.ToArray() ?? Array.Empty<long>();
                var accessControls = acl.AccessControls
                    ?.Select(this.GetAccessControl)
                    .ToArray() ?? Array.Empty<AccessControl>();

                this.ObjectsById[id] = new DatabaseRecord(this, id, workspaceClass, @object.Strategy.ObjectVersion, roleByRoleType, deniedPermissions, accessControls);
            }
        }

        internal DatabaseRecord GetRecord(long id)
        {
            _ = this.ObjectsById.TryGetValue(id, out var databaseObjects);
            return databaseObjects;
        }

        internal long GetPermission(IClass @class, IOperandType operandType, Operations operation)
        {
            var classId = this.DatabaseMetaPopulation.FindByTag(@class.Tag).Id;
            var operandId = this.DatabaseMetaPopulation.FindByTag(operandType.OperandTag).Id;

            long permission;
            var permissionCache = this.permissionCache.Get(classId);

            switch (operation)
            {
                case Operations.Read:
                    _ = permissionCache.RoleReadPermissionIdByRelationTypeId.TryGetValue(operandId, out permission);
                    break;
                case Operations.Write:
                    _ = permissionCache.RoleWritePermissionIdByRelationTypeId.TryGetValue(operandId, out permission);
                    break;
                default:
                    _ = permissionCache.MethodExecutePermissionIdByMethodTypeId.TryGetValue(operandId, out permission);
                    break;
            }

            return permission;
        }

        internal DatabaseRecord PushResponse(long id, IClass @class)
        {
            var databaseObject = new DatabaseRecord(this, id, @class);
            this.ObjectsById[id] = databaseObject;
            return databaseObject;
        }

        public IEnumerable<IObject> ObjectsToSync(PullResult pullResult) =>
            pullResult.DatabaseObjects.Where(v =>
            {
                if (this.ObjectsById.TryGetValue(v.Id, out var databaseRoles))
                {
                    return v.Strategy.ObjectVersion != databaseRoles.Version;
                }

                return true;
            });

        private AccessControl GetAccessControl(IAccessControl accessControl)
        {
            if (!this.AccessControlById.TryGetValue(accessControl.Strategy.ObjectId, out var localAccessControl))
            {
                localAccessControl = new AccessControl(accessControl.Strategy.ObjectId);
            }

            if (localAccessControl.Version == accessControl.Strategy.ObjectVersion)
            {
                return localAccessControl;
            }

            localAccessControl.Version = accessControl.Strategy.ObjectVersion;
            localAccessControl.PermissionIds = new HashSet<long>(accessControl.Permissions.Select(v => v.Id));

            return localAccessControl;
        }
    }
}
