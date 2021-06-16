// <copyright file="StoreTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using Xunit;

    public class StoreTests : DomainTest, IClassFixture<Fixture>
    {
        public StoreTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenStore_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new StoreBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithName("Organisation store");
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithDefaultCarrier(new Carriers(this.Transaction).Fedex);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithDefaultShipmentMethod(new ShipmentMethods(this.Transaction).Ground);
            builder.Build();

            Assert.False(this.Derive().HasErrors);

            builder.WithSalesInvoiceNumberCounter(new CounterBuilder(this.Transaction).Build()).Build();
            builder.Build();

            Assert.False(this.Derive().HasErrors);

            builder.WithFiscalYearsStoreSequenceNumber(new FiscalYearStoreSequenceNumbersBuilder(this.Transaction).WithFiscalYear(DateTime.Today.Year).Build());
            builder.Build();

            Assert.True(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenStore_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            var internalOrganisation = this.InternalOrganisation;

            var store = new StoreBuilder(this.Transaction)
                .WithName("Organisation store")
                .WithDefaultCarrier(new Carriers(this.Transaction).Fedex)
                .WithDefaultShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(0, store.CreditLimit);
            Assert.Equal(0, store.PaymentGracePeriod);
            Assert.Equal(0, store.ShipmentThreshold);
            Assert.Equal(internalOrganisation.DefaultCollectionMethod, store.DefaultCollectionMethod);
            Assert.Single(store.CollectionMethods);
            Assert.Equal(new Facilities(this.Transaction).FindBy(this.M.Facility.FacilityType, new FacilityTypes(this.Transaction).Warehouse), store.DefaultFacility);
        }

        [Fact]
        public void GivenStore_WhenDefaultPaymentMethodIsSet_ThenPaymentMethodIsAddedToCollectionPaymentMethods()
        {
            this.Transaction.Commit();

            var netherlands = new Countries(this.Transaction).CountryByIsoCode["NL"];
            var euro = netherlands.Currency;

            var bank = new BankBuilder(this.Transaction).WithCountry(netherlands).WithName("RABOBANK GROEP").WithBic("RABONL2U").Build();

            var ownBankAccount = new OwnBankAccountBuilder(this.Transaction)
                .WithDescription("BE23 3300 6167 6391")
                .WithBankAccount(new BankAccountBuilder(this.Transaction).WithBank(bank).WithCurrency(euro).WithIban("NL50RABO0109546784").WithNameOnAccount("Martien").Build())
                .Build();

            var store = new StoreBuilder(this.Transaction)
                .WithName("Organisation store")
                .WithDefaultCarrier(new Carriers(this.Transaction).Fedex)
                .WithDefaultShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithDefaultCollectionMethod(ownBankAccount)
                .Build();

            this.Transaction.Derive();

            Assert.Single(store.CollectionMethods);
            Assert.Equal(ownBankAccount, store.CollectionMethods.First);
        }

        [Fact]
        public void GivenStoreWithoutDefaultPaymentMethod_WhenSinglePaymentMethodIsAdded_ThenDefaultPaymentMethodIsSet()
        {
            this.Transaction.Commit();

            var netherlands = new Countries(this.Transaction).CountryByIsoCode["NL"];
            var euro = netherlands.Currency;

            var bank = new BankBuilder(this.Transaction).WithCountry(netherlands).WithName("RABOBANK GROEP").WithBic("RABONL2U").Build();

            var ownBankAccount = new OwnBankAccountBuilder(this.Transaction)
                .WithDescription("BE23 3300 6167 6391")
                .WithBankAccount(new BankAccountBuilder(this.Transaction).WithBank(bank).WithCurrency(euro).WithIban("NL50RABO0109546784").WithNameOnAccount("Martien").Build())
                .Build();

            var store = new StoreBuilder(this.Transaction)
                .WithName("Organisation store")
                .WithDefaultCarrier(new Carriers(this.Transaction).Fedex)
                .WithDefaultShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithCollectionMethod(ownBankAccount)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(ownBankAccount, store.DefaultCollectionMethod);
        }
    }
}
