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
    using Derivations.Rules;
    using Resources;

    public class BasePriceOrderQuantityBreakRule : Rule
    {
        public BasePriceOrderQuantityBreakRule(MetaPopulation m) : base(m, new Guid("6cd22bfa-6a21-44e4-8fc8-f04906aca33e")) =>
            this.Patterns = new Pattern[]
            {
                m.BasePrice.RolePattern(v => v.OrderQuantityBreak),
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
            }
        }
    }
}
