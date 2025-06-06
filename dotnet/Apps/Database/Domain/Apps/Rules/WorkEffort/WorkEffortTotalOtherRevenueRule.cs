// <copyright file="WorkEffortTotalOtherRevenueRule.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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
                m.WorkEffortInvoiceItem.RolePattern(v => v.Amount, v => v.WorkEffortInvoiceItemAssignmentWhereWorkEffortInvoiceItem.ObjectType.Assignment),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffort>())
            {
                @this.DeriveWorkEffortTotalOtherRevenue(validation);
            }
        }
    }

    public static class WorkEffortTotalOtherRevenueRuleExtensions
    {
        public static void DeriveWorkEffortTotalOtherRevenue(this WorkEffort @this, IValidation validation) => @this.TotalOtherRevenue = Rounder.RoundDecimal(@this.WorkEffortInvoiceItemAssignmentsWhereAssignment
                    .Where(v => v.ExistWorkEffortInvoiceItem
                            && !v.WorkEffortInvoiceItem.InvoiceItemType.IsDiscount
                            && v.WorkEffortInvoiceItem.Amount.HasValue)
            .Sum(v => v.WorkEffortInvoiceItem.Amount.Value), 2);
    }
}
