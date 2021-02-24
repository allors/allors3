// <copyright file="SerialisedItemOwnerDerivation.cs" company="Allors bvba">
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

    public class SerialisedItemOwnerDerivation : DomainDerivation
    {
        public SerialisedItemOwnerDerivation(M m) : base(m, new Guid("4505de7b-8e45-4683-a90b-ffd31d981c1c")) =>
            this.Patterns = new Pattern[]
            {
                new AssociationPattern(m.Shipment.ShipmentState) { Steps = new IPropertyType[] {m.Shipment.ShipmentItems, m.ShipmentItem.SerialisedItem } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                foreach (ShipmentItem shipmentItem in @this.ShipmentItemsWhereSerialisedItem)
                {
                    var shipment = shipmentItem.ShipmentWhereShipmentItem;

                    if (shipment.ShipmentState.IsShipped
                           && (!shipment.ExistLastShipmentState || !shipment.LastShipmentState.IsShipped))
                    {
                        if (shipmentItem.ExistNextSerialisedItemAvailability)
                        {
                            @this.SerialisedItemAvailability = shipmentItem.NextSerialisedItemAvailability;

                            if ((shipment.ShipFromParty as InternalOrganisation)?.SerialisedItemSoldOns.Contains(new SerialisedItemSoldOns(@this.Transaction()).CustomerShipmentShip) == true
                                && shipmentItem.NextSerialisedItemAvailability.Equals(new SerialisedItemAvailabilities(@this.Transaction()).Sold))
                            {
                                @this.OwnedBy = shipment.ShipToParty;
                                @this.Ownership = new Ownerships(@this.Transaction()).ThirdParty;
                            }
                        }

                        @this.AvailableForSale = false;
                    }
                }
            }
        }
    }
}
