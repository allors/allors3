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

    public class PurchaseReturnRule : Rule
    {
        public PurchaseReturnRule(MetaPopulation m) : base(m, new Guid("B5AB3B14-310A-42EE-9EF5-963290D812CC")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseReturn.RolePattern(v => v.ShipFromParty),
                m.PurchaseReturn.RolePattern(v => v.ShipFromAddress),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseReturn>())
            {
                if (!@this.ExistShipFromAddress && @this.ExistShipFromParty)
                {
                    @this.ShipFromAddress = @this.ShipFromParty.ShippingAddress;
                }
            }
        }
    }
}
