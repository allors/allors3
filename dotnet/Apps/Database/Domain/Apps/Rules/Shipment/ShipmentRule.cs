// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class ShipmentRule : Rule
    {
        public ShipmentRule(MetaPopulation m) : base(m, new Guid("C08727A3-808A-4CB1-B926-DA7432BAAC44")) =>
            this.Patterns = new Pattern[]
            {
                m.Shipment.RolePattern(v => v.DerivationTrigger),
                m.Shipment.RolePattern(v => v.ShipmentItems),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Shipment>())
            {
                foreach (var shipmentItem in @this.ShipmentItems)
                {
                    shipmentItem.DelegatedAccess = @this;
                }
            }
        }
    }
}
