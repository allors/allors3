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
    using Allors.Database;
    using Allors.Database.Domain;
    using Allors.Database.Security;
    using Meta;
    using Numbers;
    using IRoleType = Allors.Database.Meta.IRoleType;

    internal class Database : Adapters.Database
    {
        private readonly Dictionary<long, Adapters.AccessControl> accessControlById;
        private readonly IPermissionsCache permissionCache;
        private readonly ConcurrentDictionary<long, DatabaseRecord> recordsById;

        internal Database(IMetaPopulation metaPopulation, IDatabase wrappedDatabase, INumbers numbers) : base(metaPopulation)
        {
            this.WrappedDatabase = wrappedDatabase;
            this.Numbers = numbers;
            this.recordsById = new ConcurrentDictionary<long, DatabaseRecord>();

            this.permissionCache = this.WrappedDatabase.Context().PermissionsCache;
            this.accessControlById = new Dictionary<long, Adapters.AccessControl>();
        }

        internal IDatabase WrappedDatabase { get; }

        internal INumbers Numbers { get; }

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

                var workspaceClass = (IClass)this.MetaPopulation.FindByTag(databaseClass.Tag);
                var roleByRoleType = roleTypes.ToDictionary(w =>
                        ((IRelationType)this.MetaPopulation.FindByTag(w.RelationType.Tag)).RoleType,
                    w => GetRole(@object, w));

                var acl = accessControlLists[@object];

                var deniedPermissionNumbers = this.Numbers.From(acl.DeniedPermissionIds);
                var accessControls = acl.AccessControls
                    ?.Select(this.GetAccessControl)
                    .ToArray() ?? Array.Empty<Adapters.AccessControl>();

                this.recordsById[id] = new DatabaseRecord(this, workspaceClass, id, @object.Strategy.ObjectVersion, roleByRoleType, deniedPermissionNumbers, accessControls);
            }
        }

        public override Adapters.DatabaseRecord GetRecord(long id)
        {
            _ = this.recordsById.TryGetValue(id, out var databaseObjects);
            return databaseObjects;
        }

        public override long GetPermission(IClass @class, IOperandType operandType, Operations operation)
        {
            var classId = this.WrappedDatabase.MetaPopulation.FindByTag(@class.Tag).Id;
            var operandId = this.WrappedDatabase.MetaPopulation.FindByTag(operandType.OperandTag).Id;

            long permission;
            var permissionCacheEntry = this.permissionCache.Get(classId);

            switch (operation)
            {
                case Operations.Read:
                    _ = permissionCacheEntry.RoleReadPermissionIdByRelationTypeId.TryGetValue(operandId,
                        out permission);
                    break;
                case Operations.Write:
                    _ = permissionCacheEntry.RoleWritePermissionIdByRelationTypeId.TryGetValue(operandId,
                        out permission);
                    break;
                default:
                    _ = permissionCacheEntry.MethodExecutePermissionIdByMethodTypeId.TryGetValue(operandId,
                        out permission);
                    break;
            }

            return permission;
        }

        internal DatabaseRecord OnPushed(long id, IClass @class)
        {
            var record = new DatabaseRecord(this, @class, id);
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

        private Adapters.AccessControl GetAccessControl(IAccessControl accessControl)
        {
            if (!this.accessControlById.TryGetValue(accessControl.Strategy.ObjectId, out var acessControl))
            {
                acessControl = new Adapters.AccessControl(accessControl.Strategy.ObjectId);
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
