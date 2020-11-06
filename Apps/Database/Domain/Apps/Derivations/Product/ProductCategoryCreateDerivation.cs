// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class ProductCategoryCreateDerivation : DomainDerivation
    {
        public ProductCategoryCreateDerivation(M m) : base(m, new Guid("cc693fbf-ed27-4422-bc0d-d780d0943d8a")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.ProductCategory.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<ProductCategory>())
            {
                if (!@this.ExistCatScope)
                {
                    @this.CatScope = new CatScopes(@this.Strategy.Session).Public;
                }
            }
        }
    }
}
