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
    using Resources;

    public class PurchaseReturnShipmentItemQuantityRule : Rule
    {
        public PurchaseReturnShipmentItemQuantityRule(MetaPopulation m) : base(m, new Guid("36467cd8-77ff-495d-bcbd-6fdb6ac63c70")) =>
            this.Patterns = new Pattern[]
            {
                m.ShipmentItem.RolePattern(v => v.Quantity),
                m.OrderShipment.RolePattern(v => v.Quantity, v => v.ShipmentItem.ObjectType),
                m.NonSerialisedInventoryItem.RolePattern(v => v.QuantityOnHand, v => v.ShipmentItemsWhereReservedFromInventoryItem.ObjectType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<ShipmentItem>())
            {
                @this.DerivePurchaseReturnShipmentItemQuantity(validation);
            }
        }
    }

    public static class PurchaseReturnShipmentItemQuantityRuleExtensions
    {
        public static void DerivePurchaseReturnShipmentItemQuantity(this ShipmentItem @this, IValidation validation)
        {
            if (@this.ExistShipmentWhereShipmentItem
                && @this.ShipmentWhereShipmentItem is PurchaseReturn
                && @this.ExistOrderShipmentsWhereShipmentItem
                && @this.Quantity > ((PurchaseOrderItem)@this.OrderShipmentsWhereShipmentItem.First().OrderItem).QuantityReceived)
            {
                validation.AddError(@this, @this.Meta.Quantity, ErrorMessages.InvalidQuantity);
            }
        }
    }
}
