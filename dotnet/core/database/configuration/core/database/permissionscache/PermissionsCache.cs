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
    using Security;
    using Services;

    public class PermissionsCache : IPermissionsCache
    {
        public PermissionsCache(IDomainDatabaseServices domainDatabaseServices, IDatabase database)
        {
            this.DomainDatabaseServices = domainDatabaseServices;
            this.Database = database;
        }

        public IDomainDatabaseServices DomainDatabaseServices { get; }

        public IDatabase Database { get; }

        private Dictionary<Guid, IPermissionsCacheEntry> PermissionCacheEntryByClassId { get; set; }

        public IPermissionsCacheEntry Create(IGrouping<Guid, IPermission> permissions) => new PermissionsCacheEntry(permissions);

        public IPermissionsCacheEntry Get(Guid classId)
        {
            var permissionCacheEntryByClassId = this.PermissionCacheEntryByClassId;
            if (permissionCacheEntryByClassId == null)
            {
                var transaction = this.Database.CreateTransaction();
                try
                {
                    var permissions = new Permissions(transaction).Extent();
                    transaction.Prefetch(this.DomainDatabaseServices.Get<IPrefetchPolicyCache>().PermissionsWithClass, permissions);

                    permissionCacheEntryByClassId = permissions
                        .GroupBy(v => v.ClassPointer)
                        .ToDictionary(
                            v => v.Key,
                            w => (IPermissionsCacheEntry)new PermissionsCacheEntry(w));

                    this.PermissionCacheEntryByClassId = permissionCacheEntryByClassId;
                }
                finally
                {
                    if (this.Database.IsShared)
                    {
                        transaction.Dispose();
                    }
                }
            }

            permissionCacheEntryByClassId.TryGetValue(classId, out var permissionsCacheEntry);
            return permissionsCacheEntry;
        }

        public void Clear() => this.PermissionCacheEntryByClassId = null;
    }
}
