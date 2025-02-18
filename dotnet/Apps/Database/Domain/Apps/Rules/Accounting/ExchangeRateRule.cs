// <copyright file="ExchangeRateRule.cs" company="Allors bv">
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

    public class ExchangeRateRule : Rule
    {
        public ExchangeRateRule(MetaPopulation m) : base(m, new Guid("d67dfa14-10c4-40e0-93fd-0b29a8885160")) =>
            this.Patterns = new Pattern[]
            {
                m.ExchangeRate.RolePattern(v => v.FromCurrency),
                m.ExchangeRate.RolePattern(v => v.ToCurrency),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<ExchangeRate>())
            {
                if (@this.ExistFromCurrency && @this.ExistToCurrency && @this.FromCurrency.Equals(@this.ToCurrency))
                {
                    // TODO: Move text to Resources
                    validation.AddError(@this, @this.Meta.FromCurrency, "Currencies can not be same");
                }
            }
        }
    }
}
