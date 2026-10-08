// <copyright file="ResponseContext.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Collects, while a sync response is stored, the grants and revocations the records name
    /// that the cache does not hold yet.
    /// </summary>
    internal sealed class ResponseContext
    {
        private readonly ICache cache;

        internal ResponseContext(ICache cache)
        {
            this.cache = cache;

            this.MissingGrantIds = new HashSet<long>();
            this.MissingRevocationIds = new HashSet<long>();
        }

        internal HashSet<long> MissingGrantIds { get; }

        internal HashSet<long> MissingRevocationIds { get; }

        internal long[] CheckForMissingGrants(long[] value)
        {
            if (value == null)
            {
                return null;
            }

            foreach (var grantId in value.Where(v => this.cache.GetGrant(v) == null))
            {
                this.MissingGrantIds.Add(grantId);
            }

            return value;
        }

        internal long[] CheckForMissingRevocations(long[] value)
        {
            if (value == null)
            {
                return null;
            }

            foreach (var revocationId in value.Where(v => this.cache.GetRevocation(v) == null))
            {
                this.MissingRevocationIds.Add(revocationId);
            }

            return value;
        }
    }
}
