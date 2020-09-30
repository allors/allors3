// <copyright file="PermissionCache.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class PermissionCache
    {
        public PermissionCache(ISession session)
        {
            var permissions = new Permissions(session).Extent();
            session.Prefetch(this.PrefetchPolicy(session), permissions);

            this.PermissionCacheEntryByClassId = permissions
                .GroupBy(v => v.ConcreteClassPointer)
                .ToDictionary(
                    v => v.Key,
                    w => new PermissionCacheEntry(w));
        }

        private PrefetchPolicy PrefetchPolicy(ISession session)
        {
            // TODO: Cache
            var m = session.Database.Scope().M;
            return new PrefetchPolicyBuilder()
                .WithRule(m.Permission.ConcreteClassPointer)
                .Build();
        }

        public Dictionary<Guid, PermissionCacheEntry> PermissionCacheEntryByClassId { get; }
    }
}
