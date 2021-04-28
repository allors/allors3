// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class CustomerShipmentRule : Rule
    {
        public CustomerShipmentRule(MetaPopulation m) : base(m, new Guid("7FE90E97-A4B4-4991-9063-91BF5670B4A9")) =>
            this.Patterns = new Pattern[]
            {
                m.CustomerShipment.RolePattern(v => v.ShipFromParty),
                m.CustomerShipment.RolePattern(v => v.ShipFromAddress),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<CustomerShipment>())
            {
                if (!@this.ExistShipFromAddress)
                {
                    @this.ShipFromAddress = @this.ShipFromParty?.ShippingAddress;
                }
            }
        }
    }
}
