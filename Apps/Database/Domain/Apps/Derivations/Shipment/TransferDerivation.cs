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

    public class TransferDerivation : DomainDerivation
    {
        public TransferDerivation(M m) : base(m, new Guid("E915AF63-F1CE-4DD7-8A92-BA519C140753")) =>
            this.Patterns = new Pattern[]
            {
                new AssociationPattern(m.Transfer.ShipFromParty),
                new AssociationPattern(m.Transfer.ShipFromAddress),
                new AssociationPattern(m.Transfer.ShipToParty),
                new AssociationPattern(m.Transfer.ShipToAddress),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Transfer>())
            {
                if (!@this.ExistShipToAddress && @this.ExistShipToParty)
                {
                    @this.ShipToAddress = @this.ShipToParty.ShippingAddress;
                }

                if (!@this.ExistShipFromAddress && @this.ExistShipFromParty)
                {
                    @this.ShipFromAddress = @this.ShipFromParty.ShippingAddress;
                }
            }
        }
    }
}
