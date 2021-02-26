// <copyright file="RemoteResponseContext.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json.Api;

    public class RemoteResponseContext
    {
        private readonly Dictionary<long, RemoteAccessControl> accessControlById;
        private readonly Dictionary<long, RemotePermission> permissionById;

        public RemoteResponseContext(Dictionary<long, RemoteAccessControl> accessControlById, Dictionary<long, RemotePermission> permissionById)
        {
            this.accessControlById = accessControlById;
            this.permissionById = permissionById;

            this.MissingAccessControlIds = new HashSet<long>();
            this.MissingPermissionIds = new HashSet<long>();
        }

        internal HashSet<long> MissingAccessControlIds { get; }

        internal HashSet<long> MissingPermissionIds { get; }

        internal string ReadSortedAccessControlIds(string value)
        {
            if (value != null)
            {
                foreach (var accessControlId in value
                    .Split(Encoding.SeparatorChar)
                    .Select(long.Parse)
                    .Where(v => !this.accessControlById.ContainsKey(v)))
                {
                    this.MissingAccessControlIds.Add(accessControlId);
                }
            }

            return value;
        }

        internal string ReadSortedDeniedPermissionIds(string value)
        {
            if (value != null)
            {
                foreach (var permissionId in value
                    .Split(Encoding.SeparatorChar)
                    .Select(long.Parse)
                    .Where(v => !this.permissionById.ContainsKey(v)))
                {
                    this.MissingPermissionIds.Add(permissionId);
                }
            }

            return value;
        }
    }
}
