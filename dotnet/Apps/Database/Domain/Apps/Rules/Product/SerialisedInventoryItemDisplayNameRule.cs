// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Derivations.Rules;
    using Meta;

    public class SerialisedInventoryItemDisplayNameRule : Rule
    {
        public SerialisedInventoryItemDisplayNameRule(MetaPopulation m) : base(m, new Guid("29B3C9B5-7BB2-4851-A424-F984E7AE348B")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedInventoryItem.RolePattern(v => v.Part),
                m.SerialisedInventoryItem.RolePattern(v => v.Facility),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedInventoryItem>())
            {
                @this.DeriveSerialisedInventoryItemDisplayNameRule(validation);
            }
        }
    }

    public static class SerialisedInventoryItemDisplayNameRuleExtensions
    {
        public static void DeriveSerialisedInventoryItemDisplayNameRule(this SerialisedInventoryItem @this, IValidation validation) => @this.DisplayName = $"{@this.Part?.Name} at {@this.Facility?.Name} with state {@this.SerialisedInventoryItemState?.Name}";
    }
}
