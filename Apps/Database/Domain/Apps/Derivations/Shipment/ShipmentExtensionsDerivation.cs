// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class ShipmentDerivation : DomainDerivation
    {
        public ShipmentDerivation(M m) : base(m, new Guid("C08727A3-808A-4CB1-B926-DA7432BAAC44")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.Shipment.Interface),
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
