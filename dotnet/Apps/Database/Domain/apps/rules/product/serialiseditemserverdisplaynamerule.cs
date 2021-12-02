// <copyright file="SerialisedItemServerDispalyNameRule.cs" company="Allors bvba">
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

    public class SerialisedItemServerDispalyNameRule : Rule
    {
        public SerialisedItemServerDispalyNameRule(MetaPopulation m) : base(m, new Guid("881d963b-011e-45a2-82cf-e067dfe463e1")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.ItemNumber),
                m.SerialisedItem.RolePattern(v => v.Name),
                m.SerialisedItem.RolePattern(v => v.SerialNumber),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.Name = $"{ @this.ItemNumber} { @this.Name} SN: { @this.SerialNumber}";
            }
        }
    }
}
