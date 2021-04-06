// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class PartCategoryDeriveImageRule : Rule
    {
        public PartCategoryDeriveImageRule(MetaPopulation m) : base(m, new Guid("d2ce530e-4e2c-4bf6-b2de-ea08e3e17145")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PartCategory, m.PartCategory.CategoryImage),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PartCategory>())
            {
                if (!@this.ExistCategoryImage)
                {
                    @this.CategoryImage = @this.Strategy.Transaction.GetSingleton().Settings.NoImageAvailableImage;
                }
            }
        }
    }
}
