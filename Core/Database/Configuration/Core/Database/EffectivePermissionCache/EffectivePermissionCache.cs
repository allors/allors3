// <copyright file="IBarcodeGenerator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Domain;

    public class EffectivePermissionCache : IEffectivePermissionCache
    {
        private readonly ConcurrentDictionary<long, ISet<long>> effectivePermissionIdsByAccessControlId;

        public EffectivePermissionCache() => this.effectivePermissionIdsByAccessControlId = new ConcurrentDictionary<long, ISet<long>>();

        public void Clear(long accessControlId) => this.effectivePermissionIdsByAccessControlId.TryRemove(accessControlId, out _);

        public ISet<long> Get(long accessControlId)
        {
            this.effectivePermissionIdsByAccessControlId.TryGetValue(accessControlId, out var effectivePermissionIds);
            return effectivePermissionIds;
        }

        public void Set(long accessControlId, ISet<long> effectivePermissionIds) => this.effectivePermissionIdsByAccessControlId[accessControlId] = effectivePermissionIds;
    }
}
