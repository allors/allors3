// <copyright file="IBarcodeGenerator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System.Collections.Generic;
    using Domain;
    using Shared.Ranges;

    public class VersionedRevocation : IVersionedRevocation
    {
        public VersionedRevocation(long id, long version, IEnumerable<long> permissions)
        {
            this.Id = id;
            this.Version = version;
            this.PermissionRange = ValueRange<long>.Import(permissions);
            this.PermissionSet = new HashSet<long>(this.PermissionRange);
        }

        public long Id { get; }

        public long Version { get; }

        public ISet<long> PermissionSet { get; }

        public ValueRange<long> PermissionRange { get; }
    }
}
