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

    public class DropShipmentDeniedPermissionRule : Rule
    {
        public DropShipmentDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("e1455944-f25a-4e89-a39e-92f17969d3e0")) =>
            this.Patterns = new Pattern[]
        {
            m.DropShipment.RolePattern(v => v.TransitionalRevocations),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<DropShipment>())
            {
                @this.Revocations = @this.TransitionalRevocations;
            }
        }
    }
}
