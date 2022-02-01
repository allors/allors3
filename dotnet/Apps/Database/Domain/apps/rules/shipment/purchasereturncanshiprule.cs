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

    public class PurchaseReturnCanShipRule : Rule
    {
        public PurchaseReturnCanShipRule(MetaPopulation m) : base(m, new Guid("060150fe-5c85-47a9-832f-dc4058ab7f3b")) =>
            this.Patterns = new Pattern[]
            {
                m.ShipmentItem.RolePattern(v => v.Quantity, v => v.ShipmentWhereShipmentItem.Shipment, m.PurchaseReturn),
                m.NonSerialisedInventoryItem.RolePattern(v => v.QuantityOnHand, v => v.ShipmentItemsWhereReservedFromInventoryItem.ShipmentItem.ShipmentWhereShipmentItem.Shipment, m.PurchaseReturn),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseReturn>())
            {
                @this.DerivePurchaseReturnCanShip(validation);
            }
        }
    }

    public static class PurchaseReturnCanShipRuleExtensions
    {
        public static void DerivePurchaseReturnCanShip(this PurchaseReturn @this, IValidation validation)
        {
            var NotEnoughAvailable = @this.ShipmentItems.Any(v => v.ShipmentItemState.IsCreated
                && v.Part.InventoryItemKind.IsNonSerialised
                && v.Quantity > ((NonSerialisedInventoryItem)v.ReservedFromInventoryItems.First()).QuantityOnHand);

            @this.CanShip = !NotEnoughAvailable;
        }
    }
}
