// <copyright file="IBarcodeGenerator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System.Collections.Concurrent;
    using Domain;
    using Ranges;

    public class GrantCache : IGrantCache
    {
        private readonly ConcurrentDictionary<long, IRange<long>> permissionIdsByAccessControlId;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<long, IRange<long>>> permissionIdsByAccessControlIdByWorkspaceName;

        public GrantCache()
        {
            this.permissionIdsByAccessControlId = new ConcurrentDictionary<long, IRange<long>>();
            this.permissionIdsByAccessControlIdByWorkspaceName = new ConcurrentDictionary<string, ConcurrentDictionary<long, IRange<long>>>();
        }

        public void Clear(long accessControlId)
        {
            this.permissionIdsByAccessControlId.TryRemove(accessControlId, out _);
            foreach (var kvp in this.permissionIdsByAccessControlIdByWorkspaceName)
            {
                kvp.Value.TryRemove(accessControlId, out _);
            }
        }

        public IRange<long> GetPermissions(long accessControlId)
        {
            this.permissionIdsByAccessControlId.TryGetValue(accessControlId, out var permissionIds);
            return permissionIds;
        }

        public void SetPermissions(long accessControlId, IRange<long> permissionIds) => this.permissionIdsByAccessControlId[accessControlId] = permissionIds;

        public IRange<long> GetPermissions(string workspaceName, long accessControlId)
        {
            this.EffectivePermissionIdsByAccessControlId(workspaceName).TryGetValue(accessControlId, out var permissionIds);
            return permissionIds;
        }

        public void SetPermissions(string workspaceName, long accessControlId, IRange<long> permissionIds) => this.EffectivePermissionIdsByAccessControlId(workspaceName)[accessControlId] = permissionIds;

        private ConcurrentDictionary<long, IRange<long>> EffectivePermissionIdsByAccessControlId(string workspaceName)
        {
            if (!this.permissionIdsByAccessControlIdByWorkspaceName.TryGetValue(workspaceName, out var dictionary))
            {
                dictionary = new ConcurrentDictionary<long, IRange<long>>();
                this.permissionIdsByAccessControlIdByWorkspaceName[workspaceName] = dictionary;
            }

            return dictionary;
        }
    }
}
