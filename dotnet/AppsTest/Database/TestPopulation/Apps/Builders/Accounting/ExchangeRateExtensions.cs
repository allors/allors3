// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrandBuilderExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class ExchangeRateExtensions
    {
        public static ExchangeRateBuilder WithDefaults(this ExchangeRateBuilder @this)
        {
            var faker = @this.Transaction.Faker();

            var currencies = new Currencies(@this.Transaction).Extent().ToArray();

            var fromCurrency = faker.Random.ListItem(currencies);
            var toCurrency = faker.Random.ListItem(currencies.Except(new List<Currency>() { fromCurrency }).ToList());

            @this
                .WithValidFrom(faker.Date.Past())
                .WithFromCurrency(fromCurrency)
                .WithToCurrency(toCurrency)
                .WithRate(Math.Round(faker.Random.Decimal(0, 2.0M), 4));

            return @this;
        }
    }
}
