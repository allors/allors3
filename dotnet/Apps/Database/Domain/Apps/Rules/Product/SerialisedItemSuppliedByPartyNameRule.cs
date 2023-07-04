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

    public class SerialisedItemSuppliedByPartyNameRule : Rule
    {
        public SerialisedItemSuppliedByPartyNameRule(MetaPopulation m) : base(m, new Guid("c729d4a1-1018-4ce6-aa49-94f7dd9e7594")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.SuppliedBy),
                m.Party.RolePattern(v => v.DisplayName, v => v.SerialisedItemsWhereSuppliedBy.ObjectType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.SuppliedByPartyName = @this.SuppliedBy?.DisplayName;
            }
        }
    }
}
