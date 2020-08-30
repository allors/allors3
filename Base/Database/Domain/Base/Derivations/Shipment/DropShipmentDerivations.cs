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
        public class DropShipmentCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdDropShipments = changeSet.Created.Select(v=>v.GetObject()).OfType<DropShipment>();

                foreach(var dropShipment in createdDropShipments)
                {
                    if (!dropShipment.ExistShipToAddress && dropShipment.ExistShipToParty)
                    {
                        dropShipment.ShipToAddress = dropShipment.ShipToParty.ShippingAddress;
                    }

                    if (!dropShipment.ExistShipFromAddress && dropShipment.ExistShipFromParty)
                    {
                        dropShipment.ShipFromAddress = dropShipment.ShipFromParty.ShippingAddress;
                    }

                    Sync(dropShipment);
                }

                void Sync(DropShipment dropShipment)
                {
                    // session.Prefetch(this.SyncPrefetch, this);
                    foreach (ShipmentItem shipmentItem in dropShipment.ShipmentItems)
                    {
                        shipmentItem.Sync(dropShipment);
                    }
                }
            }
        }

        public static void DropShipmentsRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("2fa4cd39-8cdb-4c51-a0f3-faa336827da5")] = new DropShipmentCreationDerivation();
        }
    }
}
