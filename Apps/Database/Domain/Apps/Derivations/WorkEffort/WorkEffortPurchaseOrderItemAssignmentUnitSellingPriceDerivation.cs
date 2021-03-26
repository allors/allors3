// <copyright file="WorkEffortPurchaseOrderItemAssignmentUnitSellingPriceDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class WorkEffortPurchaseOrderItemAssignmentUnitSellingPriceDerivation : DomainDerivation
    {
        public WorkEffortPurchaseOrderItemAssignmentUnitSellingPriceDerivation(M m) : base(m, new Guid("077db1a1-e1ae-4af1-be2f-5bd2c25c1df1")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.WorkEffortPurchaseOrderItemAssignment, m.WorkEffortPurchaseOrderItemAssignment.Assignment),
            new RolePattern(m.WorkEffortPurchaseOrderItemAssignment, m.WorkEffortPurchaseOrderItemAssignment.PurchaseOrderItem),
            new RolePattern(m.WorkEffortPurchaseOrderItemAssignment, m.WorkEffortPurchaseOrderItemAssignment.AssignedUnitSellingPrice),
            new RolePattern(m.WorkEffort, m.WorkEffort.TakenBy) { Steps = new IPropertyType[] { m.WorkEffort.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment } },
            new RolePattern(m.PriceComponent, m.PriceComponent.PricedBy) { Steps = new IPropertyType[] { m.PriceComponent.PricedBy, m.Organisation.WorkEffortsWhereTakenBy, m.WorkEffort.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment } },
            new RolePattern(m.PriceComponent, m.PriceComponent.FromDate) { Steps = new IPropertyType[] { m.PriceComponent.PricedBy, m.Organisation.WorkEffortsWhereTakenBy, m.WorkEffort.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment } },
            new RolePattern(m.PriceComponent, m.PriceComponent.ThroughDate) { Steps = new IPropertyType[] { m.PriceComponent.PricedBy, m.Organisation.WorkEffortsWhereTakenBy, m.WorkEffort.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment } },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortPurchaseOrderItemAssignment>())
            {
                if (@this.AssignedUnitSellingPrice.HasValue)
                {
                    @this.UnitSellingPrice = @this.AssignedUnitSellingPrice.Value;
                }
                else
                {
                    var part = @this.PurchaseOrderItem.Part;

                    var currentPriceComponents = @this.Assignment?.TakenBy?.PriceComponentsWherePricedBy
                        .Where(v => v.FromDate <= @this.Assignment.ScheduledStart && (!v.ExistThroughDate || v.ThroughDate >= @this.Assignment.ScheduledStart))
                        .ToArray();

                    if (currentPriceComponents != null)
                    {
                        var currentPartPriceComponents = part.GetPriceComponents(currentPriceComponents);

                        var price = currentPartPriceComponents.OfType<BasePrice>().Max(v => v.Price);
                        @this.UnitSellingPrice = price ?? 0M;
                    }
                    else
                    {
                        @this.UnitSellingPrice = 0M;
                    }
                }
            }
        }
    }
}
