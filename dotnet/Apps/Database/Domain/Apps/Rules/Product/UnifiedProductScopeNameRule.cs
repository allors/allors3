// <copyright file="UnifiedProductProductIdentificationNamesRule.cs" company="Allors bv">
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

    public class UnifiedProductScopeNameRule : Rule
    {
        public UnifiedProductScopeNameRule(MetaPopulation m) : base(m, new Guid("0e75a3a8-783a-4a02-9004-28977faa7b37")) =>
            this.Patterns = new Pattern[]
            {
                m.UnifiedProduct.RolePattern(v => v.Scope),
                m.Scope.RolePattern(v => v.Name, v => v.UnifiedProductsWhereScope),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<UnifiedProduct>())
            {
                @this.DeriveUnifiedProductScopeName(validation);
            }
        }
    }

    public static class UnifiedProductScopeNameRuleExtensions
    {
        public static void DeriveUnifiedProductScopeName(this UnifiedProduct @this, IValidation validation) => @this.ScopeName = @this.Scope?.Name;
    }
}
