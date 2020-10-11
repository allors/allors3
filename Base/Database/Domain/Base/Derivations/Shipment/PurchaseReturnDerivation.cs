// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class PurchaseReturnDerivation : DomainDerivation
    {
        public PurchaseReturnDerivation(M m) : base(m, new Guid("B5AB3B14-310A-42EE-9EF5-963290D812CC")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.PurchaseReturn.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var purchaseReturn in matches.Cast<PurchaseReturn>())
            {
                if (!purchaseReturn.ExistShipToAddress && purchaseReturn.ExistShipToParty)
                {
                    purchaseReturn.ShipToAddress = purchaseReturn.ShipToParty.ShippingAddress;
                }

                if (!purchaseReturn.ExistShipFromAddress && purchaseReturn.ExistShipFromParty)
                {
                    purchaseReturn.ShipFromAddress = purchaseReturn.ShipFromParty.ShippingAddress;
                }

                // session.Prefetch(this.SyncPrefetch, this);
                foreach (ShipmentItem shipmentItem in purchaseReturn.ShipmentItems)
                {
                    shipmentItem.Sync(purchaseReturn);
                }
            }
        }
    }
}
