// <copyright file="IBarcodeGenerator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using Domain;
    using Ranges;

    public class VersionedPermissions : IVersionedPermissions
    {
        private readonly IRanges<long> ranges;

        public VersionedPermissions(IRanges<long> ranges, long id, long version, IEnumerable<Permission> permissions)
        {
            this.Id = id;
            this.ranges = ranges;
            this.Version = version;
            this.Range = this.ranges.Import(permissions.Select(v => v.Id));
            this.Set = new HashSet<long>(this.Range);
        }

        public long Id { get; }

        public long Version { get; }

        public IRange<long> Range { get; }

        public ISet<long> Set { get; }
    }
}
