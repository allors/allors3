// <copyright file="CustomerShipmentCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.customershipment.create;
using libs.workspace.angular.apps.src.lib.objects.shipment.list;

namespace Tests.ExchangeRateTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using libs.workspace.angular.apps.src.lib.objects.exchangerate.list;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Accounting")]
    public class ExchangeRateCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly ExchangeRateListComponent exchangeRateListPage;

        public ExchangeRateCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.exchangeRateListPage = this.Sidenav.NavigateToExchangeRates();
        }

        [Fact]
        public void Create()
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

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new ExchangeRates(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedValidFrom.Date, actual.ValidFrom.Date);
            Assert.Equal(expectedFromCurrency, actual.FromCurrency);
            Assert.Equal(expectedToCurrency, actual.ToCurrency);
            Assert.Equal(expectedRate, actual.Rate);
        }
    }
}
