// <copyright file="DomainTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Tests
{
    using System.Linq;
    using Allors.Meta;
    using Allors.Protocol.Database.Sync;

    public static class SyncResponseObjectExtensions
    {
        public static SyncResponseRole GetRole(this SyncResponseObject @this, RoleType roletype) => @this.R.FirstOrDefault(v => v.T.Equals(roletype.RelationType.IdAsString));
    }
}
