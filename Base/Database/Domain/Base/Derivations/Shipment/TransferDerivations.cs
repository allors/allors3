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
        public class TransferCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdTransfer = changeSet.Created.Select(session.Instantiate).OfType<Transfer>();

                foreach(var transfer in createdTransfer)
                {
                    if (!transfer.ExistShipToAddress && transfer.ExistShipToParty)
                    {
                        transfer.ShipToAddress = transfer.ShipToParty.ShippingAddress;
                    }

                    if (!transfer.ExistShipFromAddress && transfer.ExistShipFromParty)
                    {
                        transfer.ShipFromAddress = transfer.ShipFromParty.ShippingAddress;
                    }

                    this.Sync(transfer);
                }
            }

            void Sync(Transfer transfer)
            {
                // session.Prefetch(this.SyncPrefetch, this);
                foreach (ShipmentItem shipmentItem in transfer.ShipmentItems)
                {
                    shipmentItem.Sync(transfer);
                }
            }
        }

        public static void TransferRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("59b2b160-b0bd-4c40-a7d3-f8a0ecf80731")] = new TransferCreationDerivation();
        }
    }
}
