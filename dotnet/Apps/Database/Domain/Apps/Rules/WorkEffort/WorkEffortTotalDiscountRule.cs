// <copyright file="WorkEffortTotalDiscountRule.cs" company="Allors bvba">
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

    public class WorkEffortTotalDiscountRule : Rule
    {
        public WorkEffortTotalDiscountRule(MetaPopulation m) : base(m, new Guid("9d95a40c-a53b-4265-aa0d-11846b6b4a50")) =>
            this.Patterns = new Pattern[]
            {
                m.WorkEffortInvoiceItemAssignment.RolePattern(v => v.Assignment, v => v.Assignment),
                m.WorkEffortInvoiceItem.RolePattern(v => v.Amount, v => v.WorkEffortInvoiceItemAssignmentWhereWorkEffortInvoiceItem.WorkEffortInvoiceItemAssignment.Assignment),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffort>())
            {
                @this.DeriveWorkEffortTotalDiscount(validation);
            }
        }
    }

    public static class WorkEffortTotalDiscountRuleExtensions
    {
        public static void DeriveWorkEffortTotalDiscount(this WorkEffort @this, IValidation validation) => @this.TotalDiscount = Rounder.RoundDecimal(@this.WorkEffortInvoiceItemAssignmentsWhereAssignment
                .Where(v => v.ExistWorkEffortInvoiceItem
                            && v.WorkEffortInvoiceItem.InvoiceItemType.IsDiscount
                            && v.WorkEffortInvoiceItem.Amount.HasValue)
                .Sum(v => v.WorkEffortInvoiceItem.Amount.Value), 2);
    }
}
