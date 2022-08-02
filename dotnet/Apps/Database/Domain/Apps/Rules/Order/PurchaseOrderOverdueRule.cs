// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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

    public class PurchaseOrderOverdueRule : Rule
    {
        public PurchaseOrderOverdueRule(MetaPopulation m) : base(m, new Guid("fb640a78-f5c2-497f-b36f-b361dc2f67c9")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrderItem.RolePattern(v => v.DerivedDeliveryDate, v => v.PurchaseOrderWherePurchaseOrderItem.PurchaseOrder),
                m.PurchaseOrder.RolePattern(v => v.ValidOrderItems),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseOrder>())
            {
                @this.DerivePurchaseOrderOverdue(validation);
            }
        }
    }

    public static class PurchaseOrderOverdueRuleExtensions
    {
        public static void DerivePurchaseOrderOverdue(this PurchaseOrder @this, IValidation validation)
        {
            @this.Overdue = false;
            if ((@this.PurchaseOrderShipmentState.IsNotReceived || @this.PurchaseOrderShipmentState.IsPartiallyReceived)
                && @this.ValidOrderItems.Any(v => v.ExistDerivedDeliveryDate && @this.Strategy.Transaction.Now().Date > v.DerivedDeliveryDate))
            {
                @this.Overdue = true;
            }
        }
    }
}
