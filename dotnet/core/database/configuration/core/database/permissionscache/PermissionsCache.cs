// <copyright file="PermissionsCache.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Domain;
    using Security;
    using Services;

    public class PermissionsCache : IPermissionsCache
    {
        public PermissionsCache(IDatabase database) => this.Database = database;

        public IDatabase Database { get; }

        private Dictionary<Guid, IPermissionsCacheEntry> permissionCacheEntryByClassId { get; set; }

        public IPermissionsCacheEntry Create(IGrouping<Guid, IPermission> permissions) => new PermissionsCacheEntry(permissions);

        public IPermissionsCacheEntry Get(Guid classId)
        {
            if (this.permissionCacheEntryByClassId == null)
            {
                var transaction = this.Database.CreateTransaction();
                try
                {
                    var permissions = new Permissions(transaction).Extent();
                    transaction.Prefetch(this.Database.Services.Get<IPrefetchPolicyCache>().PermissionsWithClass, permissions);

                    if (permissions.Count == 0)
                    {
                        Debugger.Break();
                    }

                    this.permissionCacheEntryByClassId = permissions
                        .GroupBy(v => v.ClassPointer)
                        .ToDictionary(
                            v => v.Key,
                            w => (IPermissionsCacheEntry)new PermissionsCacheEntry(w));
                }
                finally
                {
                    if (this.Database.IsShared)
                    {
                        transaction.Dispose();
                    }
                }
            }

            this.permissionCacheEntryByClassId.TryGetValue(classId, out var permissionsCacheEntry);
            return permissionsCacheEntry;
        }

        public void Clear() => this.permissionCacheEntryByClassId = null;
    }
}
