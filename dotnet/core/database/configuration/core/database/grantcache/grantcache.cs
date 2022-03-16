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
        private readonly ConcurrentDictionary<long, IRange<long>> permissionIdsByGrantId;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<long, IRange<long>>> permissionIdsByGrantIdByWorkspaceName;

        public GrantCache()
        {
            this.permissionIdsByGrantId = new ConcurrentDictionary<long, IRange<long>>();
            this.permissionIdsByGrantIdByWorkspaceName = new ConcurrentDictionary<string, ConcurrentDictionary<long, IRange<long>>>();
        }

        public void Clear(long accessControlId)
        {
            this.permissionIdsByGrantId.TryRemove(accessControlId, out _);
            foreach (var kvp in this.permissionIdsByGrantIdByWorkspaceName)
            {
                kvp.Value.TryRemove(accessControlId, out _);
            }
        }

        public IRange<long> GetPermissions(long accessControlId)
        {
            this.permissionIdsByGrantId.TryGetValue(accessControlId, out var permissionIds);
            return permissionIds;
        }

        public void SetPermissions(long accessControlId, IRange<long> permissionIds) => this.permissionIdsByGrantId[accessControlId] = permissionIds;

        public IRange<long> GetPermissions(string workspaceName, long accessControlId)
        {
            this.EffectivePermissionIdsByGrantId(workspaceName).TryGetValue(accessControlId, out var permissionIds);
            return permissionIds;
        }

        public void SetPermissions(string workspaceName, long accessControlId, IRange<long> permissionIds) => this.EffectivePermissionIdsByGrantId(workspaceName)[accessControlId] = permissionIds;

        private ConcurrentDictionary<long, IRange<long>> EffectivePermissionIdsByGrantId(string workspaceName)
        {
            if (!this.permissionIdsByGrantIdByWorkspaceName.TryGetValue(workspaceName, out var dictionary))
            {
                dictionary = new ConcurrentDictionary<long, IRange<long>>();
                this.permissionIdsByGrantIdByWorkspaceName[workspaceName] = dictionary;
            }

            return dictionary;
        }
    }
}
