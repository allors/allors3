// <copyright file="Permissions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the role type.</summary>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using Database.Services;
    using Meta;

    public partial class Permissions
    {
        public Permission Get(Class @class, IRoleType roleType, Operations operation)
        {
            var permissionCacheEntry = this.Transaction.Database.Services.Get<IPermissionsCache>().Get(@class.Id);
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
            var permissionCacheEntry = this.Transaction.Database.Services.Get<IPermissionsCache>().Get(@class.Id);
            if (permissionCacheEntry != null)
            {
                var id = permissionCacheEntry.MethodExecutePermissionIdByMethodTypeId[methodType.Id];
                return (Permission)this.Transaction.Instantiate(id);
            }

            return null;
        }

        public static void Sync(IDatabase database)
        {
            using (var transaction = database.CreateTransaction())
            {
                var permissions = new Permissions(transaction).Extent();
                transaction.Prefetch(transaction.Database.Services.Get<IPrefetchPolicyCache>().PermissionsWithClass, permissions);

                var permissionsCache = transaction.Database.Services.Get<IPermissionsCache>();

                var permissionCacheEntryByClassId = permissions
                    .GroupBy(v => v.ClassPointer)
                    .ToDictionary(
                        v => v.Key,
                        w => permissionsCache.Create(w));

                var permissionIds = new HashSet<long>();

                // Create new permissions
                foreach (var @class in transaction.Database.Services.Get<MetaPopulation>().Classes)
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
                                    permissionId = new ReadPermissionBuilder(transaction)
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
                                    permissionId = new WritePermissionBuilder(transaction)
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
                                permissionId = new ExecutePermissionBuilder(transaction)
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

                            permissionIds.Add(new ReadPermissionBuilder(transaction).WithClassPointer(@class.Id).WithRelationTypePointer(relationTypeId).Build().Id);
                            permissionIds.Add(new WritePermissionBuilder(transaction).WithClassPointer(@class.Id).WithRelationTypePointer(relationTypeId).Build().Id);
                        }

                        foreach (var methodType in @class.MethodTypes)
                        {
                            permissionIds.Add(new ExecutePermissionBuilder(transaction).WithClassPointer(@class.Id).WithMethodTypePointer(methodType.Id).Build().Id);
                        }
                    }
                }

                // Delete obsolete permissions
                foreach (var permissionToDelete in new Permissions(transaction).Extent().Where(v => !permissionIds.Contains(v.Id)))
                {
                    permissionToDelete.Delete();
                }

                transaction.Derive();
                transaction.Commit();
                permissionsCache.Clear();
            }

        }
    }
}
