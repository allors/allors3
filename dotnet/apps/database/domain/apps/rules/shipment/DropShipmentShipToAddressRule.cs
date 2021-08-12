// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Derivations.Rules;
    using Meta;

    public class DropShipmentShipToAddressRule : Rule
    {
        public DropShipmentShipToAddressRule(MetaPopulation m) : base(m, new Guid("1e8610c1-4292-468b-a677-df11842fd606")) =>
            this.Patterns = new Pattern[]
            {
                m.DropShipment.RolePattern(v => v.ShipToParty),
                m.DropShipment.RolePattern(v => v.ShipToAddress),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<DropShipment>())
            {
                if (!@this.ExistShipToAddress && @this.ExistShipToParty)
                {
                    @this.ShipToAddress = @this.ShipToParty.ShippingAddress;
                }
            }
        }
    }
}
