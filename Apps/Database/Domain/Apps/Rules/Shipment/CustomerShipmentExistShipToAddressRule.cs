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

    public class CustomerShipmentExistShipToAddressRule : Rule
    {
        public CustomerShipmentExistShipToAddressRule(MetaPopulation m) : base(m, new Guid("f0128367-5006-4b73-a328-547bfd99d76b")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.CustomerShipment, m.CustomerShipment.ShipToParty),
                new RolePattern(m.CustomerShipment, m.CustomerShipment.ShipToAddress),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<CustomerShipment>())
            {
                if (!@this.ExistShipToAddress && @this.ExistShipToParty)
                {
                    @this.ShipToAddress = @this.ShipToParty.ShippingAddress;
                }
            }
        }
    }
}
