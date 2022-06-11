// <copyright file="IBarcodeGenerator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using Domain;

    public class VersionedGrants : IVersionedGrants
    {
        public VersionedGrants(long version, IEnumerable<Grant> grants)
        {
            this.Version = version;
            this.Set = new HashSet<long>(grants.Select(v => v.Id));
        }

        public long Version { get; }

        public ISet<long> Set { get; }
    }
}
