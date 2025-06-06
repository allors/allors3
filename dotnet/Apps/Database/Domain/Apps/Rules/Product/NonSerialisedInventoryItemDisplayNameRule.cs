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
    using Meta;
    using Derivations.Rules;

    public class NonSerialisedInventoryItemDisplayNameRule : Rule
    {
        public NonSerialisedInventoryItemDisplayNameRule(MetaPopulation m) : base(m, new Guid("DDB383AD-3B4C-43BE-8F30-7E3A8D16F6BE")) =>
            this.Patterns = new Pattern[]
            {
                m.NonSerialisedInventoryItem.RolePattern(v => v.Part),
                m.NonSerialisedInventoryItem.RolePattern(v => v.Facility),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<NonSerialisedInventoryItem>())
            {
                @this.DeriveNonSerialisedInventoryItemDisplayName(validation);
            }
        }
    }

    public static class NonSerialisedInventoryItemDisplayNameRuleExtensions
    {
        public static void DeriveNonSerialisedInventoryItemDisplayName(this NonSerialisedInventoryItem @this, IValidation validation) => @this.DisplayName = $"{@this.Part?.Name} at {@this.Facility?.Name} with state {@this.NonSerialisedInventoryItemState?.Name}";
    }
}
