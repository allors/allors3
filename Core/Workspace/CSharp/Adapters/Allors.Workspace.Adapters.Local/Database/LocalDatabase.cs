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
    using System.Net.Http.Headers;
    using Database;
    using Database.Domain;
    using Database.Security;
    using Meta;
    using IMetaPopulation = Meta.IMetaPopulation;
    using IRoleType = Database.Meta.IRoleType;
    using Version = Allors.Version;

    internal class LocalDatabase
    {
        private readonly IPermissionsCache permissionCache;

        internal LocalDatabase(IMetaPopulation metaPopulation, Identities identities, IPermissionsCache permissionCache)
        {
            this.MetaPopulation = metaPopulation;
            this.ObjectsById = new ConcurrentDictionary<long, LocalDatabaseObject>();
            this.Identities = identities;

            this.permissionCache = permissionCache;
            this.AccessControlById = new Dictionary<long, LocalAccessControl>();
        }

        public IMetaPopulation MetaPopulation { get; }

        public ConcurrentDictionary<long, LocalDatabaseObject> ObjectsById { get; }

        internal Identities Identities { get; }

        internal Dictionary<long, LocalAccessControl> AccessControlById { get; set; }

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

                var workspaceClass = (IClass)this.MetaPopulation.Find(databaseClass.Id);
                var roleByRoleType = roleTypes.ToDictionary(w =>
                        ((Meta.IRelationType)this.MetaPopulation.Find(w.RelationType.Id)).RoleType,
                    w => GetRole(@object, w));

                var acl = acls[@object];

                var deniedPermissions = acl.DeniedPermissionIds?.ToArray() ?? Array.Empty<long>();
                var accessControls = acl.AccessControls
                    ?.Select(this.GetAccessControl)
                    .ToArray() ?? Array.Empty<LocalAccessControl>();

                this.ObjectsById[id] = new LocalDatabaseObject(this, id, workspaceClass, @object.Strategy.ObjectVersion, roleByRoleType, deniedPermissions, accessControls);
            }
        }

        internal LocalDatabaseObject Get(long identity)
        {
            this.ObjectsById.TryGetValue(identity, out var databaseObjects);
            return databaseObjects;
        }

        internal long GetPermission(IClass @class, IOperandType operandType, Operations operation)
        {
            long permission;
            var permissionCache = this.permissionCache.Get(@class.Id);

            switch (operation)
            {
                case Operations.Read:
                    permissionCache.RoleReadPermissionIdByRelationTypeId.TryGetValue(operandType.OperandId, out permission);
                    break;
                case Operations.Write:
                    permissionCache.RoleWritePermissionIdByRelationTypeId.TryGetValue(operandType.OperandId, out permission);
                    break;
                default:
                    permissionCache.MethodExecutePermissionIdByMethodTypeId.TryGetValue(operandType.OperandId, out permission);
                    break;
            }

            return permission;
        }

        internal LocalDatabaseObject PushResponse(long identity, IClass @class)
        {
            var databaseObject = new LocalDatabaseObject(this, identity, @class);
            this.ObjectsById[identity] = databaseObject;
            return databaseObject;
        }

        public IEnumerable<IObject> ObjectsToSync(LocalPullResult pullResult) =>
            pullResult.DatabaseObjects.Where(v =>
            {
                if (this.ObjectsById.TryGetValue(v.Id, out var databaseRoles))
                {
                    return v.Strategy.ObjectVersion != databaseRoles.Version;
                }

                return true;
            });

        private LocalAccessControl GetAccessControl(IAccessControl accessControl)
        {
            if (!this.AccessControlById.TryGetValue(accessControl.Strategy.ObjectId, out var localAccessControl))
            {
                localAccessControl = new LocalAccessControl(accessControl.Strategy.ObjectId);
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
