// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class DropShipmentDerivation : DomainDerivation
    {
        public DropShipmentDerivation(M m) : base(m, new Guid("1B7E3857-425A-4946-AB63-15AEE196350D")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.DropShipment.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var dropShipment in matches.Cast<DropShipment>())
            {
                if (!dropShipment.ExistShipToAddress && dropShipment.ExistShipToParty)
                {
                    dropShipment.ShipToAddress = dropShipment.ShipToParty.ShippingAddress;
                }

                if (!dropShipment.ExistShipFromAddress && dropShipment.ExistShipFromParty)
                {
                    dropShipment.ShipFromAddress = dropShipment.ShipFromParty.ShippingAddress;
                }

                // session.Prefetch(this.SyncPrefetch, this);
                foreach (ShipmentItem shipmentItem in dropShipment.ShipmentItems)
                {
                    shipmentItem.Sync(dropShipment);
                }
            }
        }
    }
}
