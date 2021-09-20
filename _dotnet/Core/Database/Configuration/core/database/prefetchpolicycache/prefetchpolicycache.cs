// <copyright file="IPreparedSelects.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using Database;
    using Domain;
    using Meta;

    public partial class PrefetchPolicyCache : IPrefetchPolicyCache
    {
        public PrefetchPolicyCache(IDatabase database)
        {
            var m = database.Services.Get<MetaPopulation>();

            this.PermissionsWithClass = new PrefetchPolicyBuilder()
                    .WithRule(m.Permission.ClassPointer)
                    .Build();
        }

        public PrefetchPolicy PermissionsWithClass { get; }
    }
}
