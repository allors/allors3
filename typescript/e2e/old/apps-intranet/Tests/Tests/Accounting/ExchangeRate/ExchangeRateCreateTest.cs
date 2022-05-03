// <copyright file="CustomerShipmentCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>


namespace Tests.ExchangeRateTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Allors.E2E.Angular;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;


    public class ExchangeRateCreateTest : Test
    {
        private readonly ExchangeRateListComponent exchangeRateListPage;


        [SetUp]
        public async Task Setup()
        {
            await this.LoginAsync("jane@example.com");
            //this.exchangeRateListPage = this.Sidenav.NavigateToExchangeRates();
            await this.GotoAsync("/exchangerates");
        }

        [Test]
        public async Task Create()
        {
            var before = new ExchangeRates(this.Transaction).Extent().ToArray();

            var expected = new ExchangeRateBuilder(this.Transaction).WithDefaults().Build();

            this.Transaction.Derive();

            var expectedValidFrom = expected.ValidFrom;
            var expectedFromCurrency = expected.FromCurrency;
            var expectedToCurrency = expected.ToCurrency;
            var expectedRate = expected.Rate;

            var exchangeRateListComponent = exchangeRateListPage.CreateExchangeRate();

            exchangeRateListComponent
                .ValidFrom.Set(expected.ValidFrom)
                .FromCurrency.Select(expected.FromCurrency)
                .ToCurrency.Select(expected.ToCurrency)
                .Rate.Set(expected.Rate.ToString());

            this.Transaction.Rollback();
            exchangeRateListComponent.SAVE.Click();

            await this.Page.WaitForAngular();
            this.Transaction.Rollback();

            var after = new ExchangeRates(this.Transaction).Extent().ToArray();

            Assert.AreEqual(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.AreEqual(expectedValidFrom.Date, actual.ValidFrom.Date);
            Assert.AreEqual(expectedFromCurrency, actual.FromCurrency);
            Assert.AreEqual(expectedToCurrency, actual.ToCurrency);
            Assert.AreEqual(expectedRate, actual.Rate);
        }
    }
}
