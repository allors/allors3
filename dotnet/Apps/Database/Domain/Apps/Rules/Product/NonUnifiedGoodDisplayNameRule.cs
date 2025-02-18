// <copyright file="NonUnifiedGoodDisplayNameRule.cs" company="Allors bv">
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

    public class NonUnifiedGoodDisplayNameRule : Rule
    {
        public NonUnifiedGoodDisplayNameRule(MetaPopulation m) : base(m, new Guid("a07ae907-b6b1-43d7-a500-8b5d68f540d6")) =>
            this.Patterns = new Pattern[]
            {
                m.NonUnifiedGood.RolePattern(v => v.Name),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<NonUnifiedGood>())
            {
                @this.DeriveNonUnifiedGoodDisplayName(validation);
            }
        }
    }
    public static class NonUnifiedGoodDisplayNameRuleExtensions
    {
        public static void DeriveNonUnifiedGoodDisplayName(this NonUnifiedGood @this, IValidation validation)
        {
            @this.DisplayName = @this.Name ?? "N/A";
        }
    }
}
