// <copyright file="NonUnifiedPartDerivation.cs" company="Allors bvba">
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

    public class ProductCategoryDisplayNameRule : Rule
    {
        public ProductCategoryDisplayNameRule(MetaPopulation m) : base(m, new Guid("f3227a0c-180c-4cb5-b66c-95adec69516b")) =>
            this.Patterns = new Pattern[]
            {
                m.ProductCategory.RolePattern(v => v.PrimaryAncestors),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<ProductCategory>())
            {
                var primaryAncestors = @this.PrimaryAncestors.Reverse().ToList();
                primaryAncestors.Add(@this);

                @this.DisplayName = string.Join("/", primaryAncestors.Select(v => v.Name));
            }
        }
    }
}
