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

    public class SerialisedInventoryItemDeniedPermissionRule : Rule
    {
        public SerialisedInventoryItemDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("30ae162a-ac07-4a80-817c-1f5455976f93")) =>
            this.Patterns = new Pattern[]
        {
            m.SerialisedInventoryItem.RolePattern(v => v.TransitionalRevocations),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedInventoryItem>())
            {
                @this.Revocations = @this.TransitionalRevocations;
            }
        }
    }
}
