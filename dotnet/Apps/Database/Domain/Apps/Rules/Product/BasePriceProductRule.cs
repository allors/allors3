// <copyright file="Domain.cs" company="Allors bv">
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

    public class BasePriceProductRule : Rule
    {
        public BasePriceProductRule(MetaPopulation m) : base(m, new Guid("11a5592c-6f47-412a-879f-9d1c6d97169a")) =>
            this.Patterns = new Pattern[]
            {
                m.BasePrice.RolePattern(v => v.Product),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<BasePrice>())
            {
                if (@this.ExistProduct && !@this.ExistProductFeature)
                {
                    @this.Product.AddBasePrice(@this);
                }
            }
        }
    }
}
