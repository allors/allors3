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

    public class ShipmentValueDerivation : DomainDerivation
    {
        public ShipmentValueDerivation(M m) : base(m, new Guid("FF7A2ED6-9D20-4A68-874D-98BF42B5CB5B")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.ShipmentValue.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<ShipmentValue>())
            {
                cycle.Validation.AssertAtLeastOne(@this, this.M.ShipmentValue.FromAmount, this.M.ShipmentValue.ThroughAmount);
            }
        }
    }
}
