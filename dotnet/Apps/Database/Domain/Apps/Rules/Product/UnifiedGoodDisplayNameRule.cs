// <copyright file="NonUnifiedPartDerivation.cs" company="Allors bv">
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

    public class UnifiedGoodDisplayNameRule : Rule
    {
        public UnifiedGoodDisplayNameRule(MetaPopulation m) : base(m, new Guid("43e6b28a-8418-48c4-9f74-32d1ac203100")) =>
            this.Patterns = new Pattern[]
            {
                m.UnifiedGood.RolePattern(v => v.Name),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<UnifiedGood>())
            {
                @this.DeriveUnifiedGoodDisplayName(validation);
            }
        }
    }

    public static class UnifiedGoodDisplayNameRuleExtensions
    {
        public static void DeriveUnifiedGoodDisplayName(this UnifiedGood @this, IValidation validation)
        {
            @this.DisplayName = @this.Name ?? "N/A";
        }
    }
}
