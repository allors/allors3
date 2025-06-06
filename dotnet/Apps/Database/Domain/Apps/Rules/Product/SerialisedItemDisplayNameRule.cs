// <copyright file="SerialisedItemServerDispalyNameRule.cs" company="Allors bv">
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

    public class SerialisedItemDisplayNameRule : Rule
    {
        public SerialisedItemDisplayNameRule(MetaPopulation m) : base(m, new Guid("881d963b-011e-45a2-82cf-e067dfe463e1")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.ItemNumber),
                m.SerialisedItem.RolePattern(v => v.SerialNumber),
                m.SerialisedItem.AssociationPattern(v => v.PartWhereSerialisedItem),
                m.Part.RolePattern(v => v.SerialisedItems, v => v.SerialisedItems),
                m.Part.RolePattern(v => v.Name, v => v.SerialisedItems),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.DeriveSerialisedItemDisplayName(validation);
            }
        }
    }

    public static class SerialisedItemDisplayNameRuleExtensions
    {
        public static void DeriveSerialisedItemDisplayName(this SerialisedItem @this, IValidation validation)
        {
            @this.DisplayName = $"{@this.ItemNumber} {@this.PartWhereSerialisedItem?.Name} SN: {@this.SerialNumber}";
        }
    }
}
