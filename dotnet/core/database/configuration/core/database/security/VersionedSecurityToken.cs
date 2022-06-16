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
        public VersionedSecurityToken(IRanges<long> ranges, long id, long version, IEnumerable<long> users, IEnumerable<long> permissions)
        {
            this.Id = id;
            this.Version = version;
            this.UserSet = new HashSet<long>(users);
            this.PermissionRange = ranges.Import(permissions);
            this.PermissionSet = new HashSet<long>(this.PermissionRange);
        }

        public long Id { get; }

        public long Version { get; }

        public ISet<long> UserSet { get; }

        public ISet<long> PermissionSet { get; }

        public IRange<long> PermissionRange { get; }
    }
}
