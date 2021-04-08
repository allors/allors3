// <copyright file="PurchaseShipmentStateDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Database.Derivations;

    public class PurchaseShipmentStateRule : Rule
    {
        public PurchaseShipmentStateRule(MetaPopulation m) : base(m, new Guid("f5a01c30-1f20-4dba-a828-a32ab38d28de")) =>
            this.Patterns = new Pattern[]
            {
                m.ShipmentReceipt.RolePattern(v => v.QuantityAccepted, v => v.ShipmentItem.ShipmentItem.ShipmentWhereShipmentItem.Shipment.AsPurchaseShipment),
                m.ShipmentItem.RolePattern(v => v.ShipmentItemState, v => v.ShipmentWhereShipmentItem.Shipment.AsPurchaseShipment),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseShipment>())
            {
                if (@this.ShipmentItems.Any()
                    && @this.ShipmentItems.All(v => v.ExistShipmentReceiptWhereShipmentItem
                    && v.ShipmentReceiptWhereShipmentItem.QuantityAccepted.Equals(v.ShipmentReceiptWhereShipmentItem.OrderItem?.QuantityOrdered))
                    && @this.ShipmentItems.All(v => v.ShipmentItemState.Equals(new ShipmentItemStates(@this.Strategy.Transaction).Received)))
                {
                    @this.ShipmentState = new ShipmentStates(@this.Strategy.Transaction).Received;
                }
            }
        }
    }
}
