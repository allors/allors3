// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public class ShipmentDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("C08727A3-808A-4CB1-B926-DA7432BAAC44");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.Shipment.Interface),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var shipment in matches.Cast<Shipment>())
            {
                shipment.AddSecurityToken(new SecurityTokens(cycle.Session).DefaultSecurityToken);
            }
        }
    }
}
