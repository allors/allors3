// <copyright file="ExchangeRateTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Xunit;

    public class ExchangeRateRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public ExchangeRateRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedFromCurrencyThrowValidationError()
        {
            var currency = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "EUR");

            var exchangeRate = new ExchangeRateBuilder(this.Transaction).WithToCurrency(currency).Build();
            this.Derive();

            exchangeRate.FromCurrency = currency;

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains("Currencies can not be same"));
        }

        [Fact]
        public void ChangedToCurrencyThrowValidationError()
        {
            var currency = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "EUR");

            var exchangeRate = new ExchangeRateBuilder(this.Transaction).WithFromCurrency(currency).Build();
            this.Derive();

            exchangeRate.ToCurrency = currency;

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains("Currencies can not be same"));
        }
    }
}
