// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Database.Derivations;

    public class PurchaseShipmentShipFromAddressRule : Rule
    {
        public PurchaseShipmentShipFromAddressRule(MetaPopulation m) : base(m, new Guid("2ae5168c-5543-41e7-86a1-2882a143b8b4")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PurchaseShipment, m.PurchaseShipment.ShipFromParty),
                new RolePattern(m.PurchaseShipment, m.PurchaseShipment.ShipFromAddress),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseShipment>())
            {
                if (!@this.ExistShipFromAddress && @this.ExistShipFromParty)
                {
                    @this.ShipFromAddress = @this.ShipFromParty.ShippingAddress;
                }
            }
        }
    }
}
