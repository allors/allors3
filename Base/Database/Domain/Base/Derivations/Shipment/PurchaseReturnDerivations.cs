// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class PurchaseReturnCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdPurchaseReturn = changeSet.Created.Select(session.Instantiate).OfType<PurchaseReturn>();

                foreach(var purchaseReturn in createdPurchaseReturn)
                {
                    if (!purchaseReturn.ExistShipToAddress && purchaseReturn.ExistShipToParty)
                    {
                        purchaseReturn.ShipToAddress = purchaseReturn.ShipToParty.ShippingAddress;
                    }

                    if (!purchaseReturn.ExistShipFromAddress && purchaseReturn.ExistShipFromParty)
                    {
                        purchaseReturn.ShipFromAddress = purchaseReturn.ShipFromParty.ShippingAddress;
                    }

                    this.Sync(purchaseReturn);
                }
            }

            void Sync(PurchaseReturn purchaseReturn)
            {
                // session.Prefetch(this.SyncPrefetch, this);
                foreach (ShipmentItem shipmentItem in purchaseReturn.ShipmentItems)
                {
                    shipmentItem.Sync(purchaseReturn);
                }
            }
        }

        public static void PurchaseReturnRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("b54cc3b2-e3cb-46af-ab97-f3172041b1b2")] = new PurchaseReturnCreationDerivation();
        }
    }
}
