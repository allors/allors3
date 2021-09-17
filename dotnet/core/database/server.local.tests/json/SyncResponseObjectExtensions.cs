// <copyright file="DomainTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Tests
{
    using System.Linq;
    using Allors.Database.Meta;
    using Allors.Protocol.Json.Api.Sync;

    public static class SyncResponseObjectExtensions
    {
        public static SyncResponseRole GetRole(this SyncResponseObject @this, IRoleType roletype) => @this.ro.FirstOrDefault(v => v.t.Equals(roletype.RelationType.Tag));
    }
}
