// <copyright file="SerialisedItemOwnerDerivation.cs" company="Allors bv">
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

    public class SerialisedItemOwnerRule : Rule
    {
        public SerialisedItemOwnerRule(MetaPopulation m) : base(m, new Guid("4505de7b-8e45-4683-a90b-ffd31d981c1c")) =>
            this.Patterns = new Pattern[]
            {
                m.Shipment.RolePattern(v => v.ShipmentState, v => v.ShipmentItems.ObjectType.SerialisedItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                foreach (var shipmentItem in @this.ShipmentItemsWhereSerialisedItem)
                {
                    var shipment = shipmentItem.ShipmentWhereShipmentItem;

                    if (shipment.ShipmentState.IsShipped
                           && (!shipment.ExistLastShipmentState || !shipment.LastShipmentState.IsShipped))
                    {
                        if (shipmentItem.ExistNextSerialisedItemAvailability)
                        {
                            @this.SerialisedItemAvailability = shipmentItem.NextSerialisedItemAvailability;

                            if ((shipment.ShipFromParty as InternalOrganisation)?.SerialisedItemSoldOns.Contains(new SerialisedItemSoldOns(@this.Transaction()).CustomerShipmentShip) == true)
                            {
                                if (shipmentItem.NextSerialisedItemAvailability.Equals(new SerialisedItemAvailabilities(@this.Transaction()).Sold))
                                {
                                    @this.OwnedBy = shipment.ShipToParty;
                                    @this.Ownership = new Ownerships(@this.Transaction()).ThirdParty;

                                }

                                if (shipmentItem.NextSerialisedItemAvailability.Equals(new SerialisedItemAvailabilities(@this.Transaction()).InRent))
                                {
                                    @this.RentedBy = shipment.ShipToParty;
                                }
                            }
                        }

                        @this.AvailableForSale = false;
                    }
                }
            }
        }
    }
}
