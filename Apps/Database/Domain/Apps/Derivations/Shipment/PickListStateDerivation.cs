// <copyright file="PickListStateDerivation.cs" company="Allors bvba">
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

    public class PickListStateDerivation : DomainDerivation
    {
        public PickListStateDerivation(M m) : base(m, new Guid("71f5e0b8-a050-4e6f-8562-8b072a94ca58")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Shipment, m.Shipment.ShipmentState) { Steps = new IPropertyType[] { m.Shipment.ShipmentItems, m.ShipmentItem.ItemIssuancesWhereShipmentItem, m.ItemIssuance.PickListItem, m.PickListItem.PickListWherePickListItem } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PickList>())
            {
                if (!@this.ExistPicker)
                {
                    foreach(PickListItem pickListItem in @this.PickListItems)
                    {
                        foreach(ItemIssuance itemIssuance in pickListItem.ItemIssuancesWherePickListItem)
                        {
                            if (itemIssuance.ShipmentItem.ShipmentWhereShipmentItem.ShipmentState.IsOnHold)
                            {
                                @this.Hold();
                            }

                            if (!itemIssuance.ShipmentItem.ShipmentWhereShipmentItem.ShipmentState.IsOnHold
                                && @this.PickListState.IsOnHold)
                            {
                                @this.Continue();
                            }
                        }
                    }
                }
            }
        }
    }
}
