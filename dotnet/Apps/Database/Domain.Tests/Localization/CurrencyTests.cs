// <copyright file="CurrencyTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the CurrencyTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class CurrencyTests : DomainTest, IClassFixture<Fixture>
    {
        public CurrencyTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenRateForExactDate_ConvertFromCurrency()
        {
            var today = this.Transaction.Now().Date;
            var fromCurrency = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "TRY");
            var toCurrency = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "GBP");

            new ExchangeRateBuilder(this.Transaction)
                .WithValidFrom(today.AddDays(-1))
                .WithFromCurrency(fromCurrency)
                .WithToCurrency(toCurrency)
                .WithRate(0.085M)
                .Build();

            new ExchangeRateBuilder(this.Transaction)
                .WithValidFrom(today)
                .WithFromCurrency(fromCurrency)
                .WithToCurrency(toCurrency)
                .WithRate(0.085945871M)
                .Build();

            this.Transaction.Derive();

            var amount = Currencies.ConvertCurrency(270000M, today, fromCurrency, toCurrency);
            Assert.Equal(23205.39M, amount);
        }

        [Fact]
        public void GivenHistoricRate_ConvertFromCurrency_UsingMostRecent()
        {
            var today = this.Transaction.Now().Date;
            var fromCurrency = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "TRY");
            var toCurrency = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "GBP");

            new ExchangeRateBuilder(this.Transaction)
                .WithValidFrom(today.AddDays(-2))
                .WithFromCurrency(fromCurrency)
                .WithToCurrency(toCurrency)
                .WithRate(0.085M)
                .Build();

            new ExchangeRateBuilder(this.Transaction)
                .WithValidFrom(today.AddDays(-1))
                .WithFromCurrency(fromCurrency)
                .WithToCurrency(toCurrency)
                .WithRate(0.085945871M)
                .Build();

            this.Transaction.Derive();

            var amount = Currencies.ConvertCurrency(270000M, today, fromCurrency, toCurrency);
            Assert.Equal(23205.39M, amount);
        }

        [Fact]
        public void GivenFutureRate_ConvertFromCurrency()
        {
            var today = this.Transaction.Now().Date;
            var fromCurrency = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "TRY");
            var toCurrency = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "GBP");

            new ExchangeRateBuilder(this.Transaction)
                .WithValidFrom(today.AddDays(1))
                .WithFromCurrency(fromCurrency)
                .WithToCurrency(toCurrency)
                .WithRate(0.085945871M)
                .Build();

            this.Transaction.Derive();

            var amount = Currencies.ConvertCurrency(270000M, today, fromCurrency, toCurrency);
            Assert.Equal(0M, amount);
        }

        [Fact]
        public void GivenRateForExactDate_ConvertFromCurrencyUsingInvertedRate()
        {
            var today = this.Transaction.Now().Date;
            var fromCurrency = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "TRY");
            var toCurrency = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "GBP");

            new ExchangeRateBuilder(this.Transaction)
                .WithValidFrom(today)
                .WithFromCurrency(toCurrency)
                .WithToCurrency(fromCurrency)
                .WithRate(11.635230272M)
                .Build();

            this.Transaction.Derive();

            var amount = Currencies.ConvertCurrency(270000M, today, fromCurrency, toCurrency);
            Assert.Equal(23205.39M, amount);
        }
    }
}
