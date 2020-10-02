// <copyright file="IFetchService.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Services
{
    public partial class PrefetchPolicyCache : IPrefetchPolicyCache
    {
        public PrefetchPolicyCache(IDatabaseState databaseState)
        {
            this.DatabaseState = databaseState;

            var m = this.DatabaseState.M;

            this.PermissionsWithClass = new PrefetchPolicyBuilder()
                    .WithRule(m.Permission.ClassPointer)
                    .Build();
        }

        public IDatabaseState DatabaseState { get; }

        public PrefetchPolicy PermissionsWithClass { get; }
    }
}
