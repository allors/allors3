// <copyright file="RemoteResponseContext.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Collections.Generic;

    internal class ResponseContext
    {
        internal ResponseContext()
        {
            this.GrantIdsToRefresh = new HashSet<long>();
            this.RevocationIdsToRefresh = new HashSet<long>();
        }

        internal HashSet<long> GrantIdsToRefresh { get; }

        internal HashSet<long> RevocationIdsToRefresh { get; }

        internal long[] CollectGrants(long[] value)
        {
            if (value != null)
            {
                // Sync carries access-record ids without their versions. Refresh known ids too,
                // because their permissions may have changed since the previous synchronization.
                this.GrantIdsToRefresh.UnionWith(value);
            }

            return value;
        }

        internal long[] CollectRevocations(long[] value)
        {
            if (value != null)
            {
                this.RevocationIdsToRefresh.UnionWith(value);
            }

            return value;
        }
    }
}
