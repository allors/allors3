// <copyright file="WorkEffortTotalRevenueDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Meta;
    using Allors.Database.Derivations;
    using Derivations.Rules;

    public class WorkEffortTotalRevenueRule : Rule
    {
        public WorkEffortTotalRevenueRule(MetaPopulation m) : base(m, new Guid("9d0acb9a-e517-40a9-9cd6-7e15b59a6c6b")) =>
            this.Patterns = new[]
            {
                m.WorkEffort.RolePattern(v => v.Customer),
                m.WorkEffort.RolePattern(v => v.ExecutedBy),
                m.WorkEffort.RolePattern(v => v.GrandTotal),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkEffort>())
            {
                @this.TotalRevenue = @this.ExistCustomer && @this.ExistExecutedBy && @this.Customer.Equals(@this.ExecutedBy) ? 0M : @this.GrandTotal;
            }
        }
    }
}
