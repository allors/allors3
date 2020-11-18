// <copyright file="IBarcodeGenerator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Domain;

    public class WorkspaceEffectivePermissionCache : IWorkspaceEffectivePermissionCache
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<long, ISet<long>>> effectivePermissionIdsByAccessControlIdByWorkspaceName;

        public WorkspaceEffectivePermissionCache() => this.effectivePermissionIdsByAccessControlIdByWorkspaceName = new ConcurrentDictionary<string, ConcurrentDictionary<long, ISet<long>>>();

        public void Clear(long accessControlId)
        {
            foreach (var effectivePermissionIdsByAccessControlId in this.effectivePermissionIdsByAccessControlIdByWorkspaceName.Values.ToArray())
            {
                effectivePermissionIdsByAccessControlId.TryRemove(accessControlId, out _);
            }
        }

        public ISet<long> Get(string workspaceName, long accessControlId)
        {
            var effectivePermissionIdsByAccessControlId = this.EffectivePermissionIdsByAccessControlId(workspaceName);
            effectivePermissionIdsByAccessControlId.TryGetValue(accessControlId, out var effectivePermissionIds);
            return effectivePermissionIds;
        }

        public void Set(string workspaceName, long accessControlId, ISet<long> effectivePermissionIds)
        {
            var effectivePermissionIdsByAccessControlId = this.EffectivePermissionIdsByAccessControlId(workspaceName);
            effectivePermissionIdsByAccessControlId[accessControlId] = effectivePermissionIds;
        }

        private ConcurrentDictionary<long, ISet<long>> EffectivePermissionIdsByAccessControlId(string workspaceName)
        {
            if (!this.effectivePermissionIdsByAccessControlIdByWorkspaceName.TryGetValue(workspaceName, out var effectivePermissionIdsByAccessControlId))
            {
                effectivePermissionIdsByAccessControlId = new ConcurrentDictionary<long, ISet<long>>();
                this.effectivePermissionIdsByAccessControlIdByWorkspaceName[workspaceName] = effectivePermissionIdsByAccessControlId;
            }

            return effectivePermissionIdsByAccessControlId;
        }
    }
}
