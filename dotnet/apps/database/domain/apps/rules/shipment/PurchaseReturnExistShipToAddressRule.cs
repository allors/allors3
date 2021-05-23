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

    public class PurchaseReturnExistShipToAddressRule : Rule
    {
        public PurchaseReturnExistShipToAddressRule(MetaPopulation m) : base(m, new Guid("82fa069b-fb5a-4d6d-89eb-6098bf214cc5")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseReturn.RolePattern(v => v.ShipToParty),
                m.PurchaseReturn.RolePattern(v => v.ShipToAddress),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseReturn>())
            {
                if (!@this.ExistShipToAddress && @this.ExistShipToParty)
                {
                    @this.ShipToAddress = @this.ShipToParty.ShippingAddress;
                }
            }
        }
    }
}
