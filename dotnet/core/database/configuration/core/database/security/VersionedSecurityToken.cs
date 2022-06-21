// <copyright file="IBarcodeGenerator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System.Collections.Generic;
    using Domain;
    using Ranges;

    public class VersionedSecurityToken : IVersionedSecurityToken
    {
        public VersionedSecurityToken(IRanges<long> ranges, long id, long version, IDictionary<long, long> versionByGrant)
        {
            this.Id = id;
            this.Version = version;
            this.VersionByGrant = versionByGrant;
        }

        public long Id { get; }

        public long Version { get; }

        public IDictionary<long, long> VersionByGrant { get; }
    }
}
