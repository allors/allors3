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

    public class SerialisedItemRentedByPartyNameRule : Rule
    {
        public SerialisedItemRentedByPartyNameRule(MetaPopulation m) : base(m, new Guid("34d325e4-dc52-4e5f-a698-4a5f64d52dc2")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.RentedBy),
                m.Party.RolePattern(v => v.DisplayName, v => v.SerialisedItemsWhereRentedBy.ObjectType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.RentedByPartyName = @this.RentedBy?.DisplayName;
            }
        }
    }
}
