// <copyright file="PermissionsCache.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Domain;
    using Services;

    public class Permissions : IPermissions
    {

        private bool loaded;

        public Permissions() => this.loaded = false;

        public void Sync(ITransaction transaction)
        {
            var createPermissions = new CreatePermissions(transaction).Extent().ToArray();
            var readPermissions = new ReadPermissions(transaction).Extent().ToArray();
            var writePermissions = new WritePermissions(transaction).Extent().ToArray();
            var executePermissions = new ExecutePermissions(transaction).Extent().ToArray();

            var database = transaction.Database;

            transaction.Prefetch(database.Services.Get<IPrefetchPolicyCache>().PermissionsWithClass, createPermissions);
            transaction.Prefetch(database.Services.Get<IPrefetchPolicyCache>().PermissionsWithClass, readPermissions);
            transaction.Prefetch(database.Services.Get<IPrefetchPolicyCache>().PermissionsWithClass, writePermissions);
            transaction.Prefetch(database.Services.Get<IPrefetchPolicyCache>().PermissionsWithClass, executePermissions);

            foreach (var permission in createPermissions.Where(v => !v.ExistClass))
            {
                permission.Strategy.Delete();
            }

            foreach (var permission in readPermissions.Cast<Permission>().Union(writePermissions).Union(executePermissions).Where(v => !v.ExistClass || !v.ExistOperandType))
            {
                permission.Strategy.Delete();
            }

            var createPermissionsByClassId = createPermissions
                .Where(v => !v.Strategy.IsDeleted)
                .GroupBy(v => v.ClassPointer)
                .ToDictionary(
                    v => v.Key,
                    w => w.ToArray());

            var readPermissionsByClassId = readPermissions
                .Where(v => !v.Strategy.IsDeleted)
                .GroupBy(v => v.ClassPointer)
                .ToDictionary(
                    v => v.Key,
                    w => w.ToArray());

            var writePermissionsByClassId = writePermissions
                .Where(v => !v.Strategy.IsDeleted)
                .GroupBy(v => v.ClassPointer)
                .ToDictionary(
                    v => v.Key,
                    w => w.ToArray());

            var executePermissionsByClassId = executePermissions
                .Where(v => !v.Strategy.IsDeleted)
                .GroupBy(v => v.ClassPointer)
                .ToDictionary(
                    v => v.Key,
                    w => w.ToArray());

            var metaPopulation = database.MetaPopulation;

            foreach (var @class in metaPopulation.Classes)
            {
                // Create
                if (!createPermissionsByClassId.ContainsKey(@class.Id))
                {
                    new CreatePermissionBuilder(transaction).WithClassPointer(@class.Id).Build();
                }

                var relationTypeIds = new HashSet<Guid>(@class.DatabaseRoleTypes.Select(v => v.RelationType.Id));

                // Read
                if (readPermissionsByClassId.TryGetValue(@class.Id, out var classReadPermissions))
                {
                    var removedPermissions = classReadPermissions
                        .Where(v => !relationTypeIds.Contains(v.RelationTypePointer))
                        .ToArray();

                    var existingRelationTypeIds = new HashSet<Guid>(classReadPermissions
                        .Except(removedPermissions)
                        .Select(v => v.RelationType.Id));

                    foreach (var removedPermission in removedPermissions)
                    {
                        removedPermission.Strategy.Delete();
                    }

                    foreach (var relationTypeId in relationTypeIds.Where(v => !existingRelationTypeIds.Contains(v)))
                    {
                        new ReadPermissionBuilder(transaction)
                            .WithClassPointer(@class.Id)
                            .WithRelationTypePointer(relationTypeId).Build();
                    }
                }
                else
                {
                    foreach (var relationTypeId in relationTypeIds)
                    {
                        new ReadPermissionBuilder(transaction)
                            .WithClassPointer(@class.Id)
                            .WithRelationTypePointer(relationTypeId).Build();
                    }
                }

                // Write
                if (writePermissionsByClassId.TryGetValue(@class.Id, out var classWritePermissions))
                {
                    var removedPermissions = classWritePermissions
                        .Where(v => !relationTypeIds.Contains(v.RelationTypePointer))
                        .ToArray();

                    var existingRelationTypeIds = new HashSet<Guid>(classWritePermissions
                        .Except(removedPermissions)
                        .Select(v => v.RelationType.Id));

                    foreach (var removedPermission in removedPermissions)
                    {
                        removedPermission.Strategy.Delete();
                    }

                    foreach (var relationTypeId in relationTypeIds.Where(v => !existingRelationTypeIds.Contains(v)))
                    {
                        new WritePermissionBuilder(transaction)
                            .WithClassPointer(@class.Id)
                            .WithRelationTypePointer(relationTypeId).Build();
                    }
                }
                else
                {
                    foreach (var relationTypeId in relationTypeIds)
                    {
                        new WritePermissionBuilder(transaction)
                            .WithClassPointer(@class.Id)
                            .WithRelationTypePointer(relationTypeId).Build();
                    }
                }

                var methodTypeIds = new HashSet<Guid>(@class.MethodTypes.Select(v => v.Id));

                // Execute
                if (executePermissionsByClassId.TryGetValue(@class.Id, out var classExecutePermissions))
                {
                    var removedPermissions = classExecutePermissions
                        .Where(v => !methodTypeIds.Contains(v.MethodTypePointer))
                        .ToArray();

                    var existingRelationTypeIds = new HashSet<Guid>(classExecutePermissions
                        .Except(removedPermissions)
                        .Select(v => v.MethodType.Id));

                    foreach (var removedPermission in removedPermissions)
                    {
                        removedPermission.Strategy.Delete();
                    }

                    foreach (var methodTypeId in methodTypeIds.Where(v => !existingRelationTypeIds.Contains(v)))
                    {
                        new ExecutePermissionBuilder(transaction)
                            .WithClassPointer(@class.Id)
                            .WithMethodTypePointer(methodTypeId).Build();
                    }
                }
                else
                {
                    foreach (var methodTypeId in methodTypeIds)
                    {
                        new ExecutePermissionBuilder(transaction)
                            .WithClassPointer(@class.Id)
                            .WithMethodTypePointer(methodTypeId).Build();
                    }
                }
            }

            transaction.Derive();

            this.ToMeta(transaction);
        }

        public void Load(ITransaction transaction)
        {
            if (!this.loaded)
            {
                this.ToMeta(transaction);
                this.loaded = true;
            }
        }

        private void ToMeta(ITransaction transaction)
        {
            var createPermissions = new CreatePermissions(transaction).Extent().ToArray();
            var readPermissions = new ReadPermissions(transaction).Extent().ToArray();
            var writePermissions = new WritePermissions(transaction).Extent().ToArray();
            var executePermissions = new ExecutePermissions(transaction).Extent().ToArray();

            var database = transaction.Database;
            transaction.Prefetch(database.Services.Get<IPrefetchPolicyCache>().PermissionsWithClass, createPermissions);
            transaction.Prefetch(database.Services.Get<IPrefetchPolicyCache>().PermissionsWithClass, readPermissions);
            transaction.Prefetch(database.Services.Get<IPrefetchPolicyCache>().PermissionsWithClass, writePermissions);
            transaction.Prefetch(database.Services.Get<IPrefetchPolicyCache>().PermissionsWithClass, executePermissions);

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


            var metaPopulation = database.MetaPopulation;

            foreach (var @class in metaPopulation.Classes)
            {
                if (createPermissionsByClassId.TryGetValue(@class.Id, out var classCreatePermissions) &&
                    classCreatePermissions.Length == 1)
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
    }
}
