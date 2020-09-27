// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public class PurchaseShipmentDerivation : DomainDerivation
    {
        public PurchaseShipmentDerivation(M m) : base(m, new Guid("89A2FB27-6839-40D4-AFAB-79E25259B1C8")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(M.PurchaseShipment.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var purchaseShipment in matches.Cast<PurchaseShipment>())
            {
                cycle.Validation.AssertExists(purchaseShipment, purchaseShipment.Meta.ShipFromParty);

                var internalOrganisations = new Organisations(purchaseShipment.Strategy.Session).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();
                var shipToParty = purchaseShipment.ShipToParty as InternalOrganisation;
                if (!purchaseShipment.ExistShipToParty && internalOrganisations.Count() == 1)
                {
                    purchaseShipment.ShipToParty = internalOrganisations.First();
                    shipToParty = internalOrganisations.First();
                }

                purchaseShipment.ShipToAddress = purchaseShipment.ShipToAddress ?? purchaseShipment.ShipToParty?.ShippingAddress ?? purchaseShipment.ShipToParty?.GeneralCorrespondence as PostalAddress;

                if (!purchaseShipment.ExistShipToFacility && shipToParty != null && shipToParty.StoresWhereInternalOrganisation.Count == 1)
                {
                    purchaseShipment.ShipToFacility = shipToParty.StoresWhereInternalOrganisation.Single().DefaultFacility;
                }

                if (!purchaseShipment.ExistShipmentNumber && shipToParty != null)
                {
                    purchaseShipment.ShipmentNumber = shipToParty.NextShipmentNumber(purchaseShipment.Strategy.Session.Now().Year);
                    purchaseShipment.SortableShipmentNumber = purchaseShipment.Session().GetSingleton().SortableNumber(((InternalOrganisation)purchaseShipment.ShipToParty).IncomingShipmentNumberPrefix, purchaseShipment.ShipmentNumber, purchaseShipment.CreationDate.Value.Year.ToString());
                }

                if (!purchaseShipment.ExistShipFromAddress && purchaseShipment.ExistShipFromParty)
                {
                    purchaseShipment.ShipFromAddress = purchaseShipment.ShipFromParty.ShippingAddress;
                }

                if (purchaseShipment.ShipmentItems.Any()
                    && purchaseShipment.ShipmentItems.All(v => v.ExistShipmentReceiptWhereShipmentItem
                    && v.ShipmentReceiptWhereShipmentItem.QuantityAccepted.Equals(v.ShipmentReceiptWhereShipmentItem.OrderItem?.QuantityOrdered))
                    && purchaseShipment.ShipmentItems.All(v => v.ShipmentItemState.Equals(new ShipmentItemStates(purchaseShipment.Strategy.Session).Received)))
                {
                    purchaseShipment.ShipmentState = new ShipmentStates(purchaseShipment.Strategy.Session).Received;
                }

                // session.Prefetch(this.SyncPrefetch, this);
                foreach (ShipmentItem shipmentItem in purchaseShipment.ShipmentItems)
                {
                    shipmentItem.Sync(purchaseShipment);
                }
            }
        }
    }
}
