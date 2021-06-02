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

    public class BasePriceOrderValueRule : Rule
    {
        public BasePriceOrderValueRule(MetaPopulation m) : base(m, new Guid("ee30d0fe-a0ea-48e9-b27b-ee43bd4cae69")) =>
            this.Patterns = new Pattern[]
            {
                m.BasePrice.RolePattern(v => v.OrderValue),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<BasePrice>())
            {
                if (@this.ExistOrderValue)
                {
                    validation.AddError(@this, this.M.BasePrice.OrderValue, ErrorMessages.BasePriceOrderValueNotAllowed);
                }
            }
        }
    }
}
