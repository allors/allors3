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

    public class PartCategoryDisplayNameRule : Rule
    {
        public PartCategoryDisplayNameRule(MetaPopulation m) : base(m, new Guid("ef3d0304-019d-4580-adae-3f5736ca0211")) =>
            this.Patterns = new Pattern[]
            {
                m.PartCategory.RolePattern(v => v.Name),
                m.PartCategory.RolePattern(v => v.PrimaryAncestors),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PartCategory>())
            {
                @this.DerivePartCategoryDisplayName(validation);
            }
        }
    }

    public static class PartCategoryDisplayNameRuleExtensions
    {
        public static void DerivePartCategoryDisplayName(this PartCategory @this, IValidation validation)
        {
            var primaryAncestors = @this.PrimaryAncestors.Reverse().ToList();
            primaryAncestors.Add(@this);

            var array = new string[] {
                    string.Join(", ", string.Join("/", primaryAncestors.Select(v => v.Name ?? string.Empty).ToArray()))
                };

            if (array.Any(s => !string.IsNullOrEmpty(s)))
            {
                @this.DisplayName = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
            else
            {
                @this.RemoveDisplayName();
            }
        }
    }
}
