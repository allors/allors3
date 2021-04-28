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
    using IMetaPopulation = Meta.IMetaPopulation;
    using IRoleType = Database.Meta.IRoleType;

    internal class DatabaseAdapter
    {
        private readonly IPermissionsCache permissionCache;

        internal DatabaseAdapter(IMetaPopulation metaPopulation, Database.Meta.IMetaPopulation databaseMetaPopulation, Identities identities, IPermissionsCache permissionCache)
        {
            this.MetaPopulation = metaPopulation;
            this.DatabaseMetaPopulation = databaseMetaPopulation;
            this.ObjectsById = new ConcurrentDictionary<long, DatabaseObject>();
            this.Identities = identities;

            this.permissionCache = permissionCache;
            this.AccessControlById = new Dictionary<long, AccessControl>();
        }

        public IMetaPopulation MetaPopulation { get; }
        public Database.Meta.IMetaPopulation DatabaseMetaPopulation { get; }

        public ConcurrentDictionary<long, DatabaseObject> ObjectsById { get; }

        internal Identities Identities { get; }

        internal Dictionary<long, AccessControl> AccessControlById { get; set; }

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

                this.ObjectsById[id] = new DatabaseObject(this, id, workspaceClass, @object.Strategy.ObjectVersion, roleByRoleType, deniedPermissions, accessControls);
            }
        }

        internal DatabaseObject Get(long identity)
        {
            this.ObjectsById.TryGetValue(identity, out var databaseObjects);
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
                    permissionCache.RoleReadPermissionIdByRelationTypeId.TryGetValue(operandId, out permission);
                    break;
                case Operations.Write:
                    permissionCache.RoleWritePermissionIdByRelationTypeId.TryGetValue(operandId, out permission);
                    break;
                default:
                    permissionCache.MethodExecutePermissionIdByMethodTypeId.TryGetValue(operandId, out permission);
                    break;
            }

            return permission;
        }

        internal DatabaseObject PushResponse(long identity, IClass @class)
        {
            var databaseObject = new DatabaseObject(this, identity, @class);
            this.ObjectsById[identity] = databaseObject;
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
