// <copyright file="SessionObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Collections.Generic;
    using System.Linq;
    using Protocol.Database;

    public class ResponseContext
    {
        private readonly Dictionary<long, AccessControl> accessControlById;
        private readonly Dictionary<long, Permission> permissionById;

        public ResponseContext(InternalWorkspace internalWorkspace, Dictionary<long, AccessControl> accessControlById, Dictionary<long, Permission> permissionById)
        {
            this.InternalWorkspace = internalWorkspace;
            this.accessControlById = accessControlById;
            this.permissionById = permissionById;

            this.MissingAccessControlIds = new HashSet<long>();
            this.MissingPermissionIds = new HashSet<long>();
        }

        internal HashSet<long> MissingAccessControlIds { get; }

        internal HashSet<long> MissingPermissionIds { get; }

        internal InternalWorkspace InternalWorkspace { get; }

        internal string ReadSortedAccessControlIds(string value)
        {
            if (value != null)
            {
                foreach (var accessControlId in value
                    .Split(Encoding.SeparatorChar)
                    .Select(v => long.Parse(v))
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
                    .Select(v => long.Parse(v))
                    .Where(v => !this.permissionById.ContainsKey(v)))
                {
                    this.MissingPermissionIds.Add(permissionId);
                }
            }

            return value;
        }
    }
}
