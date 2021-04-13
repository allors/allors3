// <copyright file="StoreRuleTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Xunit;

    public class StoreRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public StoreRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedDefaultCollectionMethodDeriveCollectionMethod()
        {
            var store = new StoreBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var collecionMethod = new CashBuilder(this.Transaction).Build();
            store.DefaultCollectionMethod = collecionMethod;
            this.Transaction.Derive(false);

            Assert.Contains(collecionMethod, store.CollectionMethods);
        }

        [Fact]
        public void ChangedCollectionMethodsDeriveDefaultCollectionMethod()
        {
            var collecionMethod = new CashBuilder(this.Transaction).Build();
            var store = new StoreBuilder(this.Transaction).WithCollectionMethod(collecionMethod).Build();
            this.Transaction.Derive(false);

            Assert.Equal(collecionMethod, store.DefaultCollectionMethod);
        }

        [Fact]
        public void ChangedInternalOrganisationDeriveDefaultCollectionMethod()
        {
            var store = new StoreBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(this.InternalOrganisation.DefaultCollectionMethod, store.DefaultCollectionMethod);
        }

        [Fact]
        public void ChangedInternalOrganisationDeriveSalesInvoiceNumberCounter()
        {
            this.InternalOrganisation.InvoiceSequence = new InvoiceSequences(this.Transaction).EnforcedSequence;
            this.Transaction.Derive(false);

            var store = new StoreBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.True(store.ExistSalesInvoiceNumberCounter);
        }

        [Fact]
        public void ChangedInternalOrganisationDeriveCustomerShipmentNumberCounter()
        {
            this.InternalOrganisation.CustomerShipmentSequence = new CustomerShipmentSequences(this.Transaction).EnforcedSequence;
            this.Transaction.Derive(false);

            var store = new StoreBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.True(store.ExistCustomerShipmentNumberCounter);
        }

        [Fact]
        public void ChangedFiscalYearsStoreSequenceNumbersThrowValidationError()
        {
            var store = new StoreBuilder(this.Transaction).WithSalesInvoiceNumberCounter(new CounterBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            store.AddFiscalYearsStoreSequenceNumber(new FiscalYearStoreSequenceNumbersBuilder(this.Transaction).Build());

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("AssertExistsAtMostOne: Store.FiscalYearsStoreSequenceNumbers\nStore.SalesInvoiceNumberCounter"));
        }

        [Fact]
        public void ChangedSalesInvoiceNumberCounterThrowValidationError()
        {
            var store = new StoreBuilder(this.Transaction).WithFiscalYearsStoreSequenceNumber(new FiscalYearStoreSequenceNumbersBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            store.SalesInvoiceNumberCounter = new CounterBuilder(this.Transaction).Build();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("AssertExistsAtMostOne: Store.FiscalYearsStoreSequenceNumbers\nStore.SalesInvoiceNumberCounter"));
        }
    }
}
