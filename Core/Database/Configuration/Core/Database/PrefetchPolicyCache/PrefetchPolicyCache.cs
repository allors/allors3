// <copyright file="IPreparedFetches.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using Database;
    using Domain;

    public partial class PrefetchPolicyCache : IPrefetchPolicyCache
    {
        public PrefetchPolicyCache(IDatabaseContext databaseContext)
        {
            this.DatabaseContext = databaseContext;

            var m = this.DatabaseContext.M;

            this.PermissionsWithClass = new PrefetchPolicyBuilder()
                    .WithRule(m.Permission.ClassPointer)
                    .Build();
        }

        public IDatabaseContext DatabaseContext { get; }

        public PrefetchPolicy PermissionsWithClass { get; }
    }
}
