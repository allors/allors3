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

    public class PurchaseOrderItemQuantityReturnedRule : Rule
    {
        public PurchaseOrderItemQuantityReturnedRule(MetaPopulation m) : base(m, new Guid("47887633-40b0-4008-bb0d-1075af74284d")) =>
            this.Patterns = new Pattern[]
            {
                m.OrderShipment.RolePattern(v => v.Quantity, v => v.OrderItem, m.PurchaseOrderItem),
                m.PurchaseReturn.RolePattern(v => v.ShipmentState, v => v.ShipmentItems.ObjectType.OrderShipmentsWhereShipmentItem.ObjectType.OrderItem, m.PurchaseOrderItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<PurchaseOrderItem>())
            {
                @this.DerivePurchaseOrderItemQuantityReturned(validation);
            }
        }
    }

    public static class PurchaseOrderItemQuantityReturnedRuleExtensions
    {
        public static void DerivePurchaseOrderItemQuantityReturned(this PurchaseOrderItem @this, IValidation validation)
        {
            var quantityReturned = 0M;

            foreach (var orderShipment in @this.OrderShipmentsWhereOrderItem)
            {
                if (orderShipment.OrderItem is PurchaseOrderItem
                    && orderShipment.ShipmentItem.ShipmentWhereShipmentItem is PurchaseReturn
                    && orderShipment.ShipmentItem.ShipmentWhereShipmentItem.ShipmentState.IsShipped)
                {
                    quantityReturned += orderShipment.Quantity;
                }
            }

            if (@this.QuantityReturned != quantityReturned)
            {
                @this.QuantityReturned = quantityReturned;
            }
        }
    }
}
