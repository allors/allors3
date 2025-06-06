// <copyright file="Domain.cs" company="Allors bv">
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

    public class PurchaseReturnCanShipRule : Rule
    {
        public PurchaseReturnCanShipRule(MetaPopulation m) : base(m, new Guid("060150fe-5c85-47a9-832f-dc4058ab7f3b")) =>
            this.Patterns = new Pattern[]
            {
                m.ShipmentItem.RolePattern(v => v.Quantity, v => v.ShipmentWhereShipmentItem.ObjectType, m.PurchaseReturn),
                m.NonSerialisedInventoryItem.RolePattern(v => v.QuantityOnHand, v => v.ShipmentItemsWhereReservedFromInventoryItem.ObjectType.ShipmentWhereShipmentItem.ObjectType, m.PurchaseReturn),
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
            var NotEnoughAvailable = @this.ShipmentItems.Any(v => v.ExistPart
                && v.Part.InventoryItemKind.IsNonSerialised
                && v.ExistStoredInFacility
                && v.Quantity > ((NonSerialisedInventoryItem)v.StoredInFacility.InventoryItemsWhereFacility.First(i => i.Part.Equals(v.Part))).QuantityOnHand);

            @this.CanShip = !NotEnoughAvailable;
        }
    }
}
