// <copyright file="Domain.cs" company="Allors bvba">
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

    public class WorkEffortPurchaseOrderItemAssignmentPurchaseOrderItemRule : Rule
    {
        public WorkEffortPurchaseOrderItemAssignmentPurchaseOrderItemRule(MetaPopulation m) : base(m, new Guid("723302f1-a8a4-40f8-9b75-51207ec65d60")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.WorkEffortPurchaseOrderItemAssignment, m.WorkEffortPurchaseOrderItemAssignment.PurchaseOrderItem),
            new RolePattern(m.PurchaseOrderItem, m.PurchaseOrderItem.UnitPrice) { Steps = new IPropertyType[] { m.PurchaseOrderItem.WorkEffortPurchaseOrderItemAssignmentsWherePurchaseOrderItem } },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortPurchaseOrderItemAssignment>())
            {
                if (@this.ExistPurchaseOrderItem)
                {
                    @this.PurchaseOrder = @this.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem;
                    @this.UnitPurchasePrice = @this.PurchaseOrderItem.UnitPrice;
                }
            }
        }
    }
}
