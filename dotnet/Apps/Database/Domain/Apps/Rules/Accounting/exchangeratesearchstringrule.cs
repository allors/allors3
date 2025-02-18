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

    public class ExchangeRateSearchStringRule : Rule
    {
        public ExchangeRateSearchStringRule(MetaPopulation m) : base(m, new Guid("4bb4a817-b6e2-474a-b192-42781bcf5a45")) =>
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
                @this.DeriveExchangeRateSearchString(validation);
            }
        }
    }

    public static class ExchangeRateSearchStringRuleExtensions
    {
        public static void DeriveExchangeRateSearchString(this ExchangeRate @this, IValidation validation)
        {
            var array = new string[] {
                    @this.FromCurrency?.Abbreviation,
                    @this.FromCurrency?.Name,
                    @this.ToCurrency?.Abbreviation,
                    @this.ToCurrency?.Name,
                };

            if (array.Any(s => !string.IsNullOrEmpty(s)))
            {
                @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
            else
            {
                @this.RemoveSearchString();
            }
        }
    }
}
