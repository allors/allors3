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

    public class ShipmentValueDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("FF7A2ED6-9D20-4A68-874D-98BF42B5CB5B");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.ShipmentValue.Class),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var shipmentValue in matches.Cast<ShipmentValue>())
            {
                cycle.Validation.AssertAtLeastOne(shipmentValue, M.ShipmentValue.FromAmount, M.ShipmentValue.ThroughAmount);
            }
        }
    }
}
