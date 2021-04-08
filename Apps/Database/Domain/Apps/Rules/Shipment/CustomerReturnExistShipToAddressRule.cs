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

    public class CustomerReturnExistShipToAddressRule : Rule
    {
        public CustomerReturnExistShipToAddressRule(MetaPopulation m) : base(m, new Guid("76256de5-22d7-4525-85ff-439522a61a5f")) =>
            this.Patterns = new Pattern[]
            {
                m.CustomerReturn.RolePattern(v => v.ShipToParty),
                m.CustomerReturn.RolePattern(v => v.ShipToAddress),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<CustomerReturn>())
            {
                if (!@this.ExistShipToAddress && @this.ExistShipToParty)
                {
                    @this.ShipToAddress = @this.ShipToParty.ShippingAddress;
                }
            }
        }
    }
}
