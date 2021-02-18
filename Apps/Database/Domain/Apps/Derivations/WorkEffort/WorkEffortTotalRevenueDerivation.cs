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

    public class WorkEffortTotalRevenueDerivation : DomainDerivation
    {
        public WorkEffortTotalRevenueDerivation(M m) : base(m, new Guid("9d0acb9a-e517-40a9-9cd6-7e15b59a6c6b")) =>
            this.Patterns = new[]
            {
                new ChangedPattern(m.WorkEffort.Customer),
                new ChangedPattern(m.TimeEntry.WorkEffort) { Steps = new IPropertyType[] { m.ServiceEntry.WorkEffort } },
                new ChangedPattern(m.TimeEntry.BillingAmount) { Steps = new IPropertyType[] { m.ServiceEntry.WorkEffort } },
                new ChangedPattern(m.TimeEntry.IsBillable) { Steps = new IPropertyType[] { m.ServiceEntry.WorkEffort } },
                new ChangedPattern(m.TimeEntry.AmountOfTime) { Steps = new IPropertyType[] { m.ServiceEntry.WorkEffort } },
                new ChangedPattern(m.TimeEntry.BillableAmountOfTime) { Steps = new IPropertyType[] { m.ServiceEntry.WorkEffort } },
                new ChangedPattern(m.WorkEffortInventoryAssignment.Assignment) { Steps = new IPropertyType[] { m.WorkEffortInventoryAssignment.Assignment } },
                new ChangedPattern(m.WorkEffortInventoryAssignment.AssignedBillableQuantity) { Steps = new IPropertyType[] { m.WorkEffortInventoryAssignment.Assignment } },
                new ChangedPattern(m.WorkEffortInventoryAssignment.Quantity) { Steps = new IPropertyType[] { m.WorkEffortInventoryAssignment.Assignment } },
                new ChangedPattern(m.WorkEffortInventoryAssignment.UnitSellingPrice) { Steps = new IPropertyType[] { m.WorkEffortInventoryAssignment.Assignment } },
                new ChangedPattern(m.WorkEffortPurchaseOrderItemAssignment.Assignment) { Steps = new IPropertyType[] { m.WorkEffortPurchaseOrderItemAssignment.Assignment } },
                new ChangedPattern(m.WorkEffortPurchaseOrderItemAssignment.Quantity) { Steps = new IPropertyType[] { m.WorkEffortPurchaseOrderItemAssignment.Assignment } },
                new ChangedPattern(m.WorkEffortPurchaseOrderItemAssignment.UnitSellingPrice) { Steps = new IPropertyType[] { m.WorkEffortPurchaseOrderItemAssignment.Assignment } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkEffort>())
            {
                @this.TotalLabourRevenue = Math.Round(@this.BillableTimeEntries().Sum(v => v.BillingAmount), 2);
                @this.TotalMaterialRevenue = Math.Round(@this.WorkEffortInventoryAssignmentsWhereAssignment.Where(v => v.DerivedBillableQuantity > 0).Sum(v => v.DerivedBillableQuantity * v.UnitSellingPrice), 2);
                @this.TotalSubContractedRevenue = Math.Round(@this.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment.Sum(v => v.Quantity * v.UnitSellingPrice), 2);
                var totalRevenue = Math.Round(@this.TotalLabourRevenue + @this.TotalMaterialRevenue + @this.TotalSubContractedRevenue, 2);

                @this.GrandTotal = totalRevenue;
                @this.TotalRevenue = @this.ExistCustomer && @this.ExistExecutedBy && @this.Customer.Equals(@this.ExecutedBy) ? 0M : totalRevenue;
            }
        }
    }
}
