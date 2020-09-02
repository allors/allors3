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

    public class TransferDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("E915AF63-F1CE-4DD7-8A92-BA519C140753");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.Transfer.Class),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var transfer in matches.Cast<Transfer>())
            {
                if (!transfer.ExistShipToAddress && transfer.ExistShipToParty)
                {
                    transfer.ShipToAddress = transfer.ShipToParty.ShippingAddress;
                }

                if (!transfer.ExistShipFromAddress && transfer.ExistShipFromParty)
                {
                    transfer.ShipFromAddress = transfer.ShipFromParty.ShippingAddress;
                }

                // session.Prefetch(this.SyncPrefetch, this);
                foreach (ShipmentItem shipmentItem in transfer.ShipmentItems)
                {
                    shipmentItem.Sync(transfer);
                }
            }
        }
    }
}
