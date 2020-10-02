// <copyright file="PermissionsCache.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Services;

    public class PermissionsCache : IPermissionsCache
    {
        public PermissionsCache(IDatabaseState databaseState) => this.DatabaseState = databaseState;

        public IDatabaseState DatabaseState { get; }

        private Dictionary<Guid, IPermissionsCacheEntry> PermissionCacheEntryByClassId { get; set; }

        public IPermissionsCacheEntry Get(Guid classId)
        {
            var permissionCacheEntryByClassId = this.PermissionCacheEntryByClassId;
            if (permissionCacheEntryByClassId == null)
            {
                using var session = this.DatabaseState.Database.CreateSession();
                var permissions = new Permissions(session).Extent();
                session.Prefetch(this.DatabaseState.PrefetchPolicyCache.PermissionsWithClass, permissions);

                permissionCacheEntryByClassId = permissions
                    .GroupBy(v => v.ClassPointer)
                    .ToDictionary(
                        v => v.Key,
                        w => (IPermissionsCacheEntry)new PermissionsCacheEntry(w));

                this.PermissionCacheEntryByClassId = permissionCacheEntryByClassId;
            }

            permissionCacheEntryByClassId.TryGetValue(classId, out var permissionsCacheEntry);
            return permissionsCacheEntry;
        }

        public void Clear() => this.PermissionCacheEntryByClassId = null;
    }
}
