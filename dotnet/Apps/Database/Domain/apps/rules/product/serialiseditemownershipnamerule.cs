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

    public class SerialisedItemOwnershipNameRule : Rule
    {
        public SerialisedItemOwnershipNameRule(MetaPopulation m) : base(m, new Guid("457d5eb8-43c3-4b41-bd55-323ee83ffeeb")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.Ownership),
                m.Ownership.RolePattern(v => v.Name, v => v.SerialisedItemsWhereOwnership.SerialisedItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.OwnershipName = @this.Ownership?.Name;
            }
        }
    }
}
