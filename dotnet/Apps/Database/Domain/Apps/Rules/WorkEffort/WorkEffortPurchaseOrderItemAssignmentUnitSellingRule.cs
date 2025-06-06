// <copyright file="WorkEffortPurchaseOrderItemAssignmentUnitSellingPriceDerivation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
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
            m.WorkEffort.RolePattern(v => v.ScheduledStart, v => v.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment),
            m.WorkEffort.RolePattern(v => v.ActualHours, v => v.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment),
            m.PriceComponent.RolePattern(v => v.PricedBy, v => v.PricedBy.ObjectType.AsOrganisation.WorkEffortsWhereTakenBy.ObjectType.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment),
            m.PriceComponent.RolePattern(v => v.FromDate, v => v.PricedBy.ObjectType.AsOrganisation.WorkEffortsWhereTakenBy.ObjectType.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment),
            m.PriceComponent.RolePattern(v => v.ThroughDate, v => v.PricedBy.ObjectType.AsOrganisation.WorkEffortsWhereTakenBy.ObjectType.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
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
                    var part = @this.PurchaseOrderItem?.Part;

                    var startDate = @this.Assignment?.ActualStart ?? @this.Assignment?.ScheduledStart ?? transaction.Now().Date;
                    var currentPriceComponents = @this.Assignment?.TakenBy?.PriceComponentsWherePricedBy
                        .Where(v => v.ExistPrice && v.FromDate <= startDate && (!v.ExistThroughDate || v.ThroughDate >= startDate))
                        .ToArray();

                    if (@this.ExistAssignment && part != null && currentPriceComponents.Length > 0)
                    {
                        var currentPartPriceComponents = part.GetPriceComponents(currentPriceComponents);

                        var maxPrice = currentPartPriceComponents.OfType<BasePrice>().Max(v => Rounder.RoundDecimal(Currencies.ConvertCurrency(v.Price.Value, startDate, v.Currency, @this.Assignment.Currency), 2));

                        @this.UnitSellingPrice = maxPrice;
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
