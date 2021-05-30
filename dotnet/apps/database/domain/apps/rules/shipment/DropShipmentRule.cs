// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations.Rules;
    using Meta;

    public class DropShipmentRule : Rule
    {
        public DropShipmentRule(MetaPopulation m) : base(m, new Guid("1B7E3857-425A-4946-AB63-15AEE196350D")) =>
            this.Patterns = new Pattern[]
            {
                m.DropShipment.RolePattern(v => v.ShipFromParty),
                m.DropShipment.RolePattern(v => v.ShipFromAddress),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<DropShipment>())
            {
                if (!@this.ExistShipFromAddress && @this.ExistShipFromParty)
                {
                    @this.ShipFromAddress = @this.ShipFromParty.ShippingAddress;
                }
            }
        }
    }
}
