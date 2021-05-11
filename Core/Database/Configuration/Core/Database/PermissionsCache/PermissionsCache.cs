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

    public class PermissionsCache : IPermissionsCache
    {
        public PermissionsCache(IDatabaseContext databaseContext) => this.DatabaseContext = databaseContext;

        public IDatabaseContext DatabaseContext { get; }

        private Dictionary<Guid, IPermissionsCacheEntry> PermissionCacheEntryByClassId { get; set; }

        public IPermissionsCacheEntry Create(IGrouping<Guid, Permission> permissions) => new PermissionsCacheEntry(permissions);

        public IPermissionsCacheEntry Get(Guid classId)
        {
            var permissionCacheEntryByClassId = this.PermissionCacheEntryByClassId;
            if (permissionCacheEntryByClassId == null)
            {
                var transaction = this.DatabaseContext.Database.CreateTransaction();
                try
                {
                    var permissions = new Permissions(transaction).Extent();
                    transaction.Prefetch(this.DatabaseContext.PrefetchPolicyCache.PermissionsWithClass, permissions);

                    permissionCacheEntryByClassId = permissions
                        .GroupBy(v => v.ClassPointer)
                        .ToDictionary(
                            v => v.Key,
                            w => (IPermissionsCacheEntry)new PermissionsCacheEntry(w));

                    this.PermissionCacheEntryByClassId = permissionCacheEntryByClassId;
                }
                finally
                {
                    if (this.DatabaseContext.Database.IsShared)
                    {
                        transaction.Dispose();
                    }
                }
            }

            _ = permissionCacheEntryByClassId.TryGetValue(classId, out var permissionsCacheEntry);
            return permissionsCacheEntry;
        }

        public void Clear() => this.PermissionCacheEntryByClassId = null;
    }
}
