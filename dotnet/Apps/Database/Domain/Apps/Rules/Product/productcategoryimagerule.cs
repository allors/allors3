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

    public class ProductCategoryImageRule : Rule
    {
        public ProductCategoryImageRule(MetaPopulation m) : base(m, new Guid("72fbcc17-25e8-4313-a0bf-243f524c63c2")) =>
            this.Patterns = new Pattern[]
            {
                m.ProductCategory.RolePattern(v => v.CategoryImage),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<ProductCategory>())
            {
                if (!@this.ExistCategoryImage)
                {
                    @this.CategoryImage =
                        @this.Strategy.Transaction.GetSingleton().Settings.NoImageAvailableImage;
                }
            }
        }
    }
}
