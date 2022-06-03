// <copyright file="PermissionsCache.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain;
    using Services;

    public class Permissions : IPermissions
    {
        private readonly IDatabase database;

        public Permissions(IDatabase database) => this.database = database;

        public void Sync()
        {
            //using (var transaction = database.CreateTransaction())
            //{
            //    var permissions = new Permissions(transaction).Extent();
            //    transaction.Prefetch(database.Services.Get<IPrefetchPolicyCache>().PermissionsWithClass, permissions);

            //    var permissionsCache = database.Services.Get<IPermissions>();

            //    var permissionCacheEntryByClassId = permissions
            //        .GroupBy(v => v.ClassPointer)
            //        .ToDictionary(
            //            v => v.Key,
            //            w => permissionsCache.Create(w));

            //    var permissionIds = new HashSet<long>();

            //    // Create new permissions
            //    foreach (var @class in database.Services.Get<MetaPopulation>().Classes)
            //    {
            //        if (permissionCacheEntryByClassId.TryGetValue(@class.Id, out var permissionCacheEntry))
            //        {
            //            // existing class
            //            foreach (var roleType in @class.DatabaseRoleTypes)
            //            {
            //                var relationTypeId = roleType.RelationType.Id;
            //                {
            //                    if (!permissionCacheEntry.RoleReadPermissionIdByRelationTypeId.TryGetValue(relationTypeId,
            //                        out var permissionId))
            //                    {
            //                        permissionId = new ReadPermissionBuilder(transaction)
            //                            .WithClassPointer(@class.Id)
            //                            .WithRelationTypePointer(relationTypeId)
            //                            .Build()
            //                            .Id;
            //                    }

            //                    permissionIds.Add(permissionId);
            //                }

            //                {
            //                    if (!permissionCacheEntry.RoleWritePermissionIdByRelationTypeId.TryGetValue(relationTypeId, out var permissionId))
            //                    {
            //                        permissionId = new WritePermissionBuilder(transaction)
            //                            .WithClassPointer(@class.Id)
            //                            .WithRelationTypePointer(relationTypeId)
            //                            .Build()
            //                            .Id;
            //                    }

            //                    permissionIds.Add(permissionId);
            //                }
            //            }

            //            foreach (var methodType in @class.MethodTypes)
            //            {
            //                if (!permissionCacheEntry.MethodExecutePermissionIdByMethodTypeId.TryGetValue(methodType.Id, out var permissionId))
            //                {
            //                    permissionId = new ExecutePermissionBuilder(transaction)
            //                        .WithClassPointer(@class.Id)
            //                        .WithMethodTypePointer(methodType.Id)
            //                        .Build()
            //                        .Id;
            //                }

            //                permissionIds.Add(permissionId);
            //            }
            //        }
            //        else
            //        {
            //            // new class
            //            foreach (var roleType in @class.DatabaseRoleTypes)
            //            {
            //                var relationTypeId = roleType.RelationType.Id;

            //                permissionIds.Add(new ReadPermissionBuilder(transaction).WithClassPointer(@class.Id).WithRelationTypePointer(relationTypeId).Build().Id);
            //                permissionIds.Add(new WritePermissionBuilder(transaction).WithClassPointer(@class.Id).WithRelationTypePointer(relationTypeId).Build().Id);
            //            }

            //            foreach (var methodType in @class.MethodTypes)
            //            {
            //                permissionIds.Add(new ExecutePermissionBuilder(transaction).WithClassPointer(@class.Id).WithMethodTypePointer(methodType.Id).Build().Id);
            //            }
            //        }
            //    }

            //    // Delete obsolete permissions
            //    foreach (var permissionToDelete in new Permissions(transaction).Extent().Where(v => !permissionIds.Contains(v.Id)))
            //    {
            //        permissionToDelete.Delete();
            //    }

            //    transaction.Derive();
            //    transaction.Commit();
            //    permissionsCache.Clear();
            //}
        }

        public void Load()
        {
            var transaction = this.database.CreateTransaction();
            try
            {
                var createPermissions = new CreatePermissions(transaction).Extent().ToArray();
                var readPermissions = new ReadPermissions(transaction).Extent().ToArray();
                var writePermissions = new WritePermissions(transaction).Extent().ToArray();
                var executePermissions = new ExecutePermissions(transaction).Extent().ToArray();

                transaction.Prefetch(this.database.Services.Get<IPrefetchPolicyCache>().PermissionsWithClass, createPermissions);
                transaction.Prefetch(this.database.Services.Get<IPrefetchPolicyCache>().PermissionsWithClass, readPermissions);
                transaction.Prefetch(this.database.Services.Get<IPrefetchPolicyCache>().PermissionsWithClass, writePermissions);
                transaction.Prefetch(this.database.Services.Get<IPrefetchPolicyCache>().PermissionsWithClass, executePermissions);

                var createPermissionsByClassId = createPermissions
                    .GroupBy(v => v.ClassPointer)
                    .ToDictionary(
                        v => v.Key,
                        w => w.ToArray());

                var readPermissionsByClassId = readPermissions
                    .GroupBy(v => v.ClassPointer)
                    .ToDictionary(
                        v => v.Key,
                        w => w.ToArray());

                var writePermissionsByClassId = writePermissions
                    .GroupBy(v => v.ClassPointer)
                    .ToDictionary(
                        v => v.Key,
                        w => w.ToArray());

                var executePermissionsByClassId = executePermissions
                    .GroupBy(v => v.ClassPointer)
                    .ToDictionary(
                        v => v.Key,
                        w => w.ToArray());


                var metaPopulation = this.database.MetaPopulation;

                foreach (var @class in metaPopulation.Classes)
                {
                    if (createPermissionsByClassId.TryGetValue(@class.Id, out var classCreatePermissions) && classCreatePermissions.Length == 1)
                    {
                        @class.CreatePermissionId = classCreatePermissions[0].Id;
                    }
                    else
                    {
                        @class.CreatePermissionId = 0;
                    }

                    var relationTypeIds = new HashSet<Guid>(@class.DatabaseRoleTypes.Select(v => v.RelationType.Id));

                    if (readPermissionsByClassId.TryGetValue(@class.Id, out var classReadPermissions))
                    {
                        @class.ReadPermissionIdByRelationTypeId = classReadPermissions
                            .Where(v => relationTypeIds.Contains(v.RelationTypePointer))
                            .ToDictionary(v => v.RelationTypePointer, v => v.Id);
                    }
                    else
                    {
                        @class.ReadPermissionIdByRelationTypeId = new Dictionary<Guid, long>();
                    }

                    if (writePermissionsByClassId.TryGetValue(@class.Id, out var classWritePermissions))
                    {
                        @class.WritePermissionIdByRelationTypeId = classWritePermissions
                            .Where(v => relationTypeIds.Contains(v.RelationTypePointer))
                            .ToDictionary(v => v.RelationTypePointer, v => v.Id);
                    }
                    else
                    {
                        @class.WritePermissionIdByRelationTypeId = new Dictionary<Guid, long>();
                    }

                    var methodTypeIds = new HashSet<Guid>(@class.MethodTypes.Select(v => v.Id));

                    if (executePermissionsByClassId.TryGetValue(@class.Id, out var classExecutePermissions))
                    {
                        @class.ExecutePermissionIdByMethodTypeId = classExecutePermissions
                            .Where(v => methodTypeIds.Contains(v.MethodTypePointer))
                            .ToDictionary(v => v.MethodTypePointer, v => v.Id);
                    }
                    else
                    {
                        @class.ExecutePermissionIdByMethodTypeId = new Dictionary<Guid, long>();
                    }
                }
            }
            finally
            {
                if (this.database.IsShared)
                {
                    transaction.Dispose();
                }
            }
        }
    }
}
