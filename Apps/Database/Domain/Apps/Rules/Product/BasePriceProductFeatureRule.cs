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
    using Resources;

    public class BasePriceProductFeatureRule : Rule
    {
        public BasePriceProductFeatureRule(MetaPopulation m) : base(m, new Guid("a2fda191-ca37-45b3-8556-bb429aac9a6d")) =>
            this.Patterns = new Pattern[]
            {
                m.BasePrice.RolePattern(v => v.ProductFeature),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<BasePrice>())
            {
                if (@this.ExistProductFeature)
                {
                    @this.ProductFeature.AddToBasePrice(@this);
                }
            }
        }
    }
}
