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

    public class ShipmentDerivation : DomainDerivation
    {
        public ShipmentDerivation(M m) : base(m, new Guid("C08727A3-808A-4CB1-B926-DA7432BAAC44")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.Shipment.ShipmentNumber),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Shipment>())
            {
                @this.AddSecurityToken(new SecurityTokens(cycle.Session).DefaultSecurityToken);
            }
        }
    }
}
