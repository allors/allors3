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

    public class PartCategoryImageRule : Rule
    {
        public PartCategoryImageRule(MetaPopulation m) : base(m, new Guid("90cbc963-0307-46ca-86ce-20af7c6bdb41")) =>
            this.Patterns = new Pattern[]
            {
                m.PartCategory.RolePattern(v => v.CategoryImage),
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
