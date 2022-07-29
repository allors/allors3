// <copyright file="WorkEffortTotalOtherRevenueRule.cs" company="Allors bvba">
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

    public class WorkEffortTotalOtherRevenueRule : Rule
    {
        public WorkEffortTotalOtherRevenueRule(MetaPopulation m) : base(m, new Guid("32ca5f8e-2b89-4a7a-92c6-51f921c67579")) =>
            this.Patterns = new Pattern[]
            {
                m.WorkEffortInvoiceItemAssignment.RolePattern(v => v.Assignment, v => v.Assignment),
                m.WorkEffortInvoiceItem.RolePattern(v => v.Amount, v => v.WorkEffortInvoiceItemAssignmentWhereWorkEffortInvoiceItem.WorkEffortInvoiceItemAssignment.Assignment),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkEffort>())
            {
                @this.TotalOtherRevenue = Rounder.RoundDecimal(@this.WorkEffortInvoiceItemAssignmentsWhereAssignment
                    .Where(v => v.WorkEffortInvoiceItem.Amount.HasValue)
                    .Sum(v => v.WorkEffortInvoiceItem.Amount.Value), 2);
            }
        }
    }
}
