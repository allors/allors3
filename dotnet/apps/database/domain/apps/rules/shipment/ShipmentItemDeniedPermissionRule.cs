// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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

    public class ShipmentItemDeniedPermissionRule : Rule
    {
        public ShipmentItemDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("a690e467-5509-4e2a-905b-b5a3fb0bee12")) =>
            this.Patterns = new Pattern[]
        {
            m.ShipmentItem.RolePattern(v => v.TransitionalRevocations),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<ShipmentItem>())
            {
                @this.Revocations = @this.TransitionalRevocations;
            }
        }
    }
}
