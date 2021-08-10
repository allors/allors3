// <copyright file="Permissions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the role type.</summary>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors;
    using Database.Services;
    using Meta;

    public partial class Permissions
    {
        public Permission Get(Class @class, IRoleType roleType, Operations operation)
        {
            var permissionCacheEntry = ((IDatabaseServices)this.Transaction.Database.Services).Get<IPermissionsCache>().Get(@class.Id);
            if (permissionCacheEntry != null)
            {
                long id = 0;
                switch (operation)
                {
                    case Operations.Read:
                        id = permissionCacheEntry.RoleReadPermissionIdByRelationTypeId[roleType.RelationType.Id];
                        break;

                    case Operations.Write:
                        id = permissionCacheEntry.RoleWritePermissionIdByRelationTypeId[roleType.RelationType.Id];
                        break;
                }

                return (Permission)this.Transaction.Instantiate(id);
            }

            return null;
        }

        public Permission Get(Class @class, IMethodType methodType)
        {
            var permissionCacheEntry = ((IDatabaseServices)this.Transaction.Database.Services).Get<IPermissionsCache>().Get(@class.Id);
            if (permissionCacheEntry != null)
            {
                var id = permissionCacheEntry.MethodExecutePermissionIdByMethodTypeId[methodType.Id];
                return (Permission)this.Transaction.Instantiate(id);
            }

            return null;
        }

        public void Sync()
        {
            var permissions = new Permissions(this.Transaction).Extent();
            this.Transaction.Prefetch(((IDatabaseServices)((IObjects)this).Transaction.Database.Services).Get<IPrefetchPolicyCache>().PermissionsWithClass, permissions);

            var permissionsCache = ((IDatabaseServices)this.Transaction.Database.Services).Get<IPermissionsCache>();

            var permissionCacheEntryByClassId = permissions
                .GroupBy(v => v.ClassPointer)
                .ToDictionary(
                    v => v.Key,
                    w => permissionsCache.Create(w));

            var permissionIds = new HashSet<long>();

            // Create new permissions
            foreach (var @class in ((IDatabaseServices)((IObjects)this).Transaction.Database.Services).Get<Allors.Database.Meta.MetaPopulation>().Classes)
            {
                if (permissionCacheEntryByClassId.TryGetValue(@class.Id, out var permissionCacheEntry))
                {
                    // existing class
                    foreach (var roleType in @class.DatabaseRoleTypes)
                    {
                        var relationTypeId = roleType.RelationType.Id;
                        {
                            if (!permissionCacheEntry.RoleReadPermissionIdByRelationTypeId.TryGetValue(relationTypeId,
                                out var permissionId))
                            {
                                permissionId = new ReadPermissionBuilder(this.Transaction)
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
                                permissionId = new WritePermissionBuilder(this.Transaction)
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
                            permissionId = new ExecutePermissionBuilder(this.Transaction)
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

                        permissionIds.Add(new ReadPermissionBuilder(this.Transaction).WithClassPointer(@class.Id).WithRelationTypePointer(relationTypeId).Build().Id);
                        permissionIds.Add(new WritePermissionBuilder(this.Transaction).WithClassPointer(@class.Id).WithRelationTypePointer(relationTypeId).Build().Id);
                    }

                    foreach (var methodType in @class.MethodTypes)
                    {
                        permissionIds.Add(new ExecutePermissionBuilder(this.Transaction).WithClassPointer(@class.Id).WithMethodTypePointer(methodType.Id).Build().Id);
                    }
                }
            }

            // Delete obsolete permissions
            foreach (var permissionToDelete in new Permissions(this.Transaction).Extent().Where(v => !permissionIds.Contains(v.Id)))
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
