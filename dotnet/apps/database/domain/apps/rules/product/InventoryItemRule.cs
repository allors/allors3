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

    public class InventoryItemRule : Rule
    {
        public InventoryItemRule(MetaPopulation m) : base(m, new Guid("E1BE8D0A-DD31-404F-A0F5-B03B0D3DB3AB")) =>
            this.Patterns = new[]
            {
                m.InventoryItem.RolePattern(v => v.Part),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<InventoryItem>())
            {
                if (!@this.ExistFacility && @this.ExistPart && @this.Part.ExistDefaultFacility)
                {
                    @this.Facility = @this.Part.DefaultFacility;
                }
            }
        }
    }
}
