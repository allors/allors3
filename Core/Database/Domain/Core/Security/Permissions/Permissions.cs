// <copyright file="Permissions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the role type.</summary>

namespace Allors.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors;
    using Allors.Meta;

    public partial class Permissions
    {
        // TODO: Cache permissions
        public Permission Get(Class @class, OperandType operandType, Operations operation)
        {
            var permissionCache = this.Session.GetCache<PermissionCache, PermissionCache>(() => new PermissionCache(this.Session));

            if (permissionCache.PermissionCacheEntryByClassId.TryGetValue(@class.Id, out var permissionCacheEntry))
            {
                long id = 0;
                switch (operation)
                {
                    case Operations.Read:
                        id = operandType is RoleType roleType ?
                            permissionCacheEntry.RoleReadPermissionIdByRelationTypeId[roleType.RelationType.Id] :
                            permissionCacheEntry.AssociationReadPermissionIdByRelationTypeId[((AssociationType)operandType).RelationType.Id];
                        break;

                    case Operations.Write:
                        id = permissionCacheEntry.RoleWritePermissionIdByRelationTypeId[((RoleType)operandType).RelationType.Id];
                        break;

                    default:
                        id = permissionCacheEntry.MethodExecutePermissionIdByMethodTypeId[((MethodType)operandType).Id];
                        break;
                }

                return (Permission)this.Session.Instantiate(id);
            }

            return null;
        }

        public void Sync()
        {
            var domain = (MetaPopulation)this.Session.Database.ObjectFactory.MetaPopulation;

            var permissionCache = new PermissionCache(this.Session);
            var permissionIds = new HashSet<long>();

            // Create new permissions
            foreach (var @class in domain.Classes)
            {
                if (permissionCache.PermissionCacheEntryByClassId.TryGetValue(@class.Id, out var permissionCacheEntry))
                {
                    // existing class
                    foreach (var roleType in @class.DatabaseRoleTypes)
                    {
                        var relationTypeId = roleType.RelationType.Id;
                        {
                            if (!permissionCacheEntry.RoleReadPermissionIdByRelationTypeId.TryGetValue(relationTypeId,
                                out var permissionId))
                            {
                                permissionId = new ReadPermissionBuilder(this.Session)
                                    .WithClassPointer(@class.Id)
                                    .WithRelationTypePointer(relationTypeId)
                                    .Build()
                                    .Id;
                            }

                            permissionIds.Add(permissionId);
                        }

                        {
                            if (!permissionCacheEntry.RoleWritePermissionIdByRelationTypeId.TryGetValue(relationTypeId, out var permissionId))
                            {
                                permissionId = new WritePermissionBuilder(this.Session)
                                    .WithClassPointer(@class.Id)
                                    .WithRelationTypePointer(relationTypeId)
                                    .Build()
                                    .Id;
                            }

                            permissionIds.Add(permissionId);
                        }
                    }

                    foreach (var methodType in @class.MethodTypes)
                    {
                        if (!permissionCacheEntry.MethodExecutePermissionIdByMethodTypeId.TryGetValue(methodType.Id, out var permissionId))
                        {
                            permissionId = new ExecutePermissionBuilder(this.Session)
                                .WithClassPointer(@class.Id)
                                .WithMethodTypePointer(methodType.Id)
                                .Build()
                                .Id;
                        }

                        permissionIds.Add(permissionId);
                    }
                }
                else
                {
                    // new class
                    foreach (var roleType in @class.DatabaseRoleTypes)
                    {
                        var relationTypeId = roleType.RelationType.Id;

                        permissionIds.Add(new ReadPermissionBuilder(this.Session).WithClassPointer(@class.Id).WithRelationTypePointer(relationTypeId).Build().Id);
                        permissionIds.Add(new WritePermissionBuilder(this.Session).WithClassPointer(@class.Id).WithRelationTypePointer(relationTypeId).Build().Id);
                    }

                    foreach (var methodType in @class.MethodTypes)
                    {
                        permissionIds.Add(new ExecutePermissionBuilder(this.Session).WithClassPointer(@class.Id).WithMethodTypePointer(methodType.Id).Build().Id);
                    }
                }
            }

            // Delete obsolete permissions
            foreach (var permissionToDelete in new Permissions(this.Session).Extent().Where(v => !permissionIds.Contains(v.Id)))
            {
                permissionToDelete.Delete();
            }
        }

        protected override void CoreSetup(Setup setup)
        {
            if (setup.Config.SetupSecurity)
            {
                this.Sync();
            }
        }
    }
}
