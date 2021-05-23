// <copyright file="IBarcodeGenerator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Domain;

    public class AccessControlCache : IAccessControlCache
    {
        private readonly ConcurrentDictionary<long, ISet<long>> permissionIdsByAccessControlId;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<long, ISet<long>>> permissionIdsByAccessControlIdByWorkspaceName;

        public AccessControlCache()
        {
            this.permissionIdsByAccessControlId = new ConcurrentDictionary<long, ISet<long>>();
            this.permissionIdsByAccessControlIdByWorkspaceName = new ConcurrentDictionary<string, ConcurrentDictionary<long, ISet<long>>>();
        }

        public void Clear(long accessControlId)
        {
            _ = this.permissionIdsByAccessControlId.TryRemove(accessControlId, out _);
            foreach (var kvp in this.permissionIdsByAccessControlIdByWorkspaceName)
            {
                _ = kvp.Value.TryRemove(accessControlId, out _);
            }
        }

        public ISet<long> GetPermissions(long accessControlId)
        {
            _ = this.permissionIdsByAccessControlId.TryGetValue(accessControlId, out var permissionIds);
            return permissionIds;
        }

        public void SetPermissions(long accessControlId, ISet<long> permissionIds) => this.permissionIdsByAccessControlId[accessControlId] = permissionIds;
        
        public ISet<long> GetPermissions(string workspaceName, long accessControlId)
        {
            _ = this.EffectivePermissionIdsByAccessControlId(workspaceName).TryGetValue(accessControlId, out var permissionIds);
            return permissionIds;
        }

        public void SetPermissions(string workspaceName, long accessControlId, ISet<long> permissionIds) => this.EffectivePermissionIdsByAccessControlId(workspaceName)[accessControlId] = permissionIds;

        private ConcurrentDictionary<long, ISet<long>> EffectivePermissionIdsByAccessControlId(string workspaceName)
        {
            if (!this.permissionIdsByAccessControlIdByWorkspaceName.TryGetValue(workspaceName, out var dictionary))
            {
                dictionary = new ConcurrentDictionary<long, ISet<long>>();
                this.permissionIdsByAccessControlIdByWorkspaceName[workspaceName] = dictionary;
            }

            return dictionary;
        }
    }
}
