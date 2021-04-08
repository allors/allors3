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

    public class TransferDeriveShipFromAddressRule : Rule
    {
        public TransferDeriveShipFromAddressRule(MetaPopulation m) : base(m, new Guid("032f3068-356c-4e40-9fb3-6a7b5b05ece5")) =>
            this.Patterns = new Pattern[]
            {
               m.Transfer.RolePattern(v => v.ShipFromParty),
               m.Transfer.RolePattern(v => v.ShipFromAddress),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Transfer>())
            {
                if (!@this.ExistShipFromAddress && @this.ExistShipFromParty)
                {
                    @this.ShipFromAddress = @this.ShipFromParty.ShippingAddress;
                }
            }
        }
    }
}
