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
    using Derivations.Rules;

    public class WorkEffortPurchaseOrderItemAssignmentUnitSellingRule : Rule
    {
        public WorkEffortPurchaseOrderItemAssignmentUnitSellingRule(MetaPopulation m) : base(m, new Guid("9c6d9950-de23-42de-b70e-84ef275eabc8")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkEffortPurchaseOrderItemAssignment.RolePattern(v => v.Assignment),
            m.WorkEffortPurchaseOrderItemAssignment.RolePattern(v => v.PurchaseOrderItem),
            m.WorkEffortPurchaseOrderItemAssignment.RolePattern(v => v.AssignedUnitSellingPrice),
            m.WorkEffort.RolePattern(v => v.TakenBy, v => v.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment),
            m.PriceComponent.RolePattern(v => v.PricedBy, v => v.PricedBy.Party.AsOrganisation.WorkEffortsWhereTakenBy.WorkEffort.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment),
            m.PriceComponent.RolePattern(v => v.FromDate, v => v.PricedBy.Party.AsOrganisation.WorkEffortsWhereTakenBy.WorkEffort.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment),
            m.PriceComponent.RolePattern(v => v.ThroughDate, v => v.PricedBy.Party.AsOrganisation.WorkEffortsWhereTakenBy.WorkEffort.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment),
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
