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
    using Derivations.Rules;

    public class WorkEffortPurchaseOrderItemAssignmentPurchaseOrderItemRule : Rule
    {
        public WorkEffortPurchaseOrderItemAssignmentPurchaseOrderItemRule(MetaPopulation m) : base(m, new Guid("723302f1-a8a4-40f8-9b75-51207ec65d60")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkEffortPurchaseOrderItemAssignment.RolePattern(v => v.PurchaseOrderItem),
            m.PurchaseOrderItem.RolePattern(v => v.UnitPrice, v => v.WorkEffortPurchaseOrderItemAssignmentsWherePurchaseOrderItem),
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
