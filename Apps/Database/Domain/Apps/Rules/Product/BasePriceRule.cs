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

    public class BasePriceRule : Rule
    {
        public BasePriceRule(M m) : base(m, new Guid("499B0F1E-F653-4DB6-82D0-190C9738DA5A")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.BasePrice, m.BasePrice.OrderQuantityBreak),
                new RolePattern(m.BasePrice, m.BasePrice.OrderValue),
                new RolePattern(m.BasePrice, m.BasePrice.Product),
                new RolePattern(m.BasePrice, m.BasePrice.ProductFeature),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<BasePrice>())
            {
                if (@this.ExistOrderQuantityBreak)
                {
                    validation.AddError($"{@this} { this.M.BasePrice.OrderQuantityBreak} {ErrorMessages.BasePriceOrderQuantityBreakNotAllowed}");
                }

                if (@this.ExistOrderValue)
                {
                    validation.AddError($"{@this} {this.M.BasePrice.OrderValue} {ErrorMessages.BasePriceOrderValueNotAllowed}");
                }

                if (@this.ExistProduct && !@this.ExistProductFeature)
                {
                    @this.Product.AddBasePrice(@this);
                }

                if (@this.ExistProductFeature)
                {
                    @this.ProductFeature.AddToBasePrice(@this);
                }
            }
        }
    }
}
