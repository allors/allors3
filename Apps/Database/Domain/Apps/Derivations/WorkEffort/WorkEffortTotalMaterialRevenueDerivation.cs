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

    public class WorkEffortTotalMaterialRevenueDerivation : DomainDerivation
    {
        public WorkEffortTotalMaterialRevenueDerivation(M m) : base(m, new Guid("ccc168df-8e92-4635-8449-7b375d2bfb94")) =>
            this.Patterns = new[]
            {
                new RolePattern(m.WorkEffortInventoryAssignment, m.WorkEffortInventoryAssignment.Assignment) { Steps = new IPropertyType[] { m.WorkEffortInventoryAssignment.Assignment } },
                new RolePattern(m.WorkEffortInventoryAssignment, m.WorkEffortInventoryAssignment.AssignedBillableQuantity) { Steps = new IPropertyType[] { m.WorkEffortInventoryAssignment.Assignment } },
                new RolePattern(m.WorkEffortInventoryAssignment, m.WorkEffortInventoryAssignment.Quantity) { Steps = new IPropertyType[] { m.WorkEffortInventoryAssignment.Assignment } },
                new RolePattern(m.WorkEffortInventoryAssignment, m.WorkEffortInventoryAssignment.UnitSellingPrice) { Steps = new IPropertyType[] { m.WorkEffortInventoryAssignment.Assignment } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkEffort>())
            {
                @this.TotalMaterialRevenue = Math.Round(@this.WorkEffortInventoryAssignmentsWhereAssignment.Where(v => v.DerivedBillableQuantity > 0).Sum(v => v.DerivedBillableQuantity * v.UnitSellingPrice), 2);
            }
        }
    }
}
