// <copyright file="ReadStoredSecurity.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class ReadStoredSecurity
    {
        public ReadStoredSecurity(ReadStoreScope scope, ReadSecurityKey key, IEnumerable<long> permissionIds)
        {
            this.Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            this.Key = key ?? throw new ArgumentNullException(nameof(key));
            this.PermissionIds = Array.AsReadOnly((permissionIds ?? throw new ArgumentNullException(nameof(permissionIds)))
                .Select(v => ReadStoreValues.Id(v, nameof(permissionIds))).Distinct().OrderBy(v => v).ToArray());
        }

        public ReadStoreScope Scope { get; }

        public ReadSecurityKey Key { get; }

        public IReadOnlyList<long> PermissionIds { get; }

        internal bool SameBody(ReadStoredSecurity other) => this.PermissionIds.SequenceEqual(other.PermissionIds);
    }
}
