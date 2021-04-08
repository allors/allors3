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

    public class NonSerialisedInventoryItemDeniedPermissionRule : Rule
    {
        public NonSerialisedInventoryItemDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("29737b61-0cfe-4dc8-9b71-12f40861af3c")) =>
            this.Patterns = new Pattern[]
        {
            m.NonSerialisedInventoryItem.RolePattern(v => v.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<NonSerialisedInventoryItem>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
            }
        }
    }
}
