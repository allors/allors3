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

    public class CustomerReturnDerivation : DomainDerivation
    {
        public CustomerReturnDerivation(M m) : base(m, new Guid("F43BD748-619E-4A3C-A002-21AD436EA764")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.CustomerReturn.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<CustomerReturn>())
            {
                if (!@this.ExistShipToAddress && @this.ExistShipToParty)
                {
                    @this.ShipToAddress = @this.ShipToParty.ShippingAddress;
                }

                if (!@this.ExistShipFromAddress && @this.ExistShipFromParty)
                {
                    @this.ShipFromAddress = @this.ShipFromParty.ShippingAddress;
                }

                // session.Prefetch(this.SyncPrefetch, this);
                foreach (ShipmentItem shipmentItem in @this.ShipmentItems)
                {
                    shipmentItem.Sync(@this);
                }
            }
        }
    }
}
