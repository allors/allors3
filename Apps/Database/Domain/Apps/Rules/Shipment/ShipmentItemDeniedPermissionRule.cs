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

    public class ShipmentItemDeniedPermissionRule : Rule
    {
        public ShipmentItemDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("a690e467-5509-4e2a-905b-b5a3fb0bee12")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.ShipmentItem, m.ShipmentItem.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<ShipmentItem>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
            }
        }
    }
}
