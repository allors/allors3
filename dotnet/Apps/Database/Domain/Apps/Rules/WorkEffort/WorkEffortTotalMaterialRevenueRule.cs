// <copyright file="WorkEffortTotalMaterialRevenueDerivation.cs" company="Allors bvba">
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

    public class WorkEffortTotalMaterialRevenueRule : Rule
    {
        public WorkEffortTotalMaterialRevenueRule(MetaPopulation m) : base(m, new Guid("ccc168df-8e92-4635-8449-7b375d2bfb94")) =>
            this.Patterns = new Pattern[]
            {
                m.WorkEffort.RolePattern(v => v.DerivationTrigger),
                m.WorkEffortInventoryAssignment.RolePattern(v => v.Assignment, v => v.Assignment),
                m.WorkEffortInventoryAssignment.RolePattern(v => v.AssignedBillableQuantity, v => v.Assignment),
                m.WorkEffortInventoryAssignment.RolePattern(v => v.Quantity, v => v.Assignment),
                m.WorkEffortInventoryAssignment.RolePattern(v => v.UnitSellingPrice, v => v.Assignment),
                m.WorkTask.AssociationPattern(v => v.WorkEffortInventoryAssignmentsWhereAssignment),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffort>())
            {   
                @this.DeriveWorkEffortTotalMaterialRevenueRule(validation);
            }
        }
    }

    public static class WorkEffortTotalMaterialRevenueRuleExtensions
    {
        public static void DeriveWorkEffortTotalMaterialRevenueRule(this WorkEffort @this, IValidation validation)
        {
            @this.TotalMaterialRevenue = Rounder.RoundDecimal(@this.WorkEffortInventoryAssignmentsWhereAssignment.Where(v => v.DerivedBillableQuantity > 0).Sum(v => v.DerivedBillableQuantity * v.UnitSellingPrice), 2);
        }
    }
}
