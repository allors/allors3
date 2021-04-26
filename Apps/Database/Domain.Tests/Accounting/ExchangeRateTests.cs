// <copyright file="ExchangeRateTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using Allors.Database.Derivations;
    using Resources;
    using Xunit;

    public class ExchangeRateRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public ExchangeRateRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedFromCurrencyThrowValidationError()
        {
            var currency = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "EUR");

            var exchangeRate = new ExchangeRateBuilder(this.Transaction).WithToCurrency(currency).Build();
            this.Transaction.Derive(false);

            exchangeRate.FromCurrency = currency;

            var expectedMessage = $"{exchangeRate}, {exchangeRate.FromCurrency}, Currencies can not be same";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedToCurrencyThrowValidationError()
        {
            var currency = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "EUR");

            var exchangeRate = new ExchangeRateBuilder(this.Transaction).WithFromCurrency(currency).Build();
            this.Transaction.Derive(false);

            exchangeRate.ToCurrency = currency;

            var expectedMessage = $"{exchangeRate}, {exchangeRate.FromCurrency}, Currencies can not be same";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }
    }
}
