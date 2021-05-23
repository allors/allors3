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

    public class CustomerReturnRule : Rule
    {
        public CustomerReturnRule(MetaPopulation m) : base(m, new Guid("F43BD748-619E-4A3C-A002-21AD436EA764")) =>
            this.Patterns = new Pattern[]
            {
                m.CustomerReturn.RolePattern(v => v.ShipFromParty),
                m.CustomerReturn.RolePattern(v => v.ShipFromAddress),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<CustomerReturn>())
            {
                if (!@this.ExistShipFromAddress && @this.ExistShipFromParty)
                {
                    @this.ShipFromAddress = @this.ShipFromParty.ShippingAddress;
                }
            }
        }
    }
}
