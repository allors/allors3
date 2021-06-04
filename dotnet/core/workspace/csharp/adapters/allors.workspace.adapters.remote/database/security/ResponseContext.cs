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
        private readonly DatabaseConnection database;

        internal ResponseContext(DatabaseConnection database)
        {
            this.database = database;

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

            foreach (var accessControlId in value.Where(v => !this.database.AccessControlById.ContainsKey(v)))
            {
                this.MissingAccessControlIds.Add(accessControlId);
            }

            return value;
        }

        internal long[] CheckForMissingPermissions(long[] value)
        {
            if (value == null)
            {
                return null;
            }

            foreach (var permissionId in value.Where(v => !this.database.Permissions.Contains(v)))
            {
                this.MissingPermissionIds.Add(permissionId);
            }

            return value;
        }
    }
}
