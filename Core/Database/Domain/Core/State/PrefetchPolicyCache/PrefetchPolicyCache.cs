// <copyright file="IFetchService.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Services
{
    public partial class PrefetchPolicyCache : IPrefetchPolicyCache
    {
        public PrefetchPolicyCache(IDatabaseInstance databaseInstance)
        {
            this.DatabaseInstance = databaseInstance;

            var m = this.DatabaseInstance.M;

            this.PermissionsWithClass = new PrefetchPolicyBuilder()
                    .WithRule(m.Permission.ClassPointer)
                    .Build();
        }

        public IDatabaseInstance DatabaseInstance { get; }

        public PrefetchPolicy PermissionsWithClass { get; }
    }
}
