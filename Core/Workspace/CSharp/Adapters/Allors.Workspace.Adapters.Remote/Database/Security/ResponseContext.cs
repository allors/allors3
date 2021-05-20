// <copyright file="RemoteResponseContext.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Collections.Generic;
    using System.Linq;

    internal class ResponseContext
    {
        private readonly Dictionary<long, AccessControl> accessControlById;
        private readonly Dictionary<long, Permission> permissionById;

        internal ResponseContext(Dictionary<long, AccessControl> accessControlById,
            Dictionary<long, Permission> permissionById)
        {
            this.accessControlById = accessControlById;
            this.permissionById = permissionById;

            this.MissingAccessControlIds = new HashSet<long>();
            this.MissingPermissionIds = new HashSet<long>();
        }

        internal HashSet<long> MissingAccessControlIds { get; }

        internal HashSet<long> MissingPermissionIds { get; }

        internal long[] CheckForMissingAccessControls(long[] value)
        {
            if (value == null)
            {
                return null;
            }

            foreach (var accessControlId in value.Where(v => !this.accessControlById.ContainsKey(v)))
            {
                _ = this.MissingAccessControlIds.Add(accessControlId);
            }

            return value;
        }

        internal long[] CheckForMissingPermissions(long[] value)
        {
            if (value == null)
            {
                return null;
            }

            foreach (var permissionId in value.Where(v => !this.permissionById.ContainsKey(v)))
            {
                _ = this.MissingPermissionIds.Add(permissionId);
            }

            return value;
        }
    }
}
