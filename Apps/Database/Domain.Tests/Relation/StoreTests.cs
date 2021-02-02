// <copyright file="StoreDerivationTests.cs" company="Allors bvba">
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

    public class StoreDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public StoreDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedDefaultCollectionMethodDeriveCollectionMethod()
        {
            var store = new StoreBuilder(this.Session).Build();
            this.Session.Derive(false);

            var collecionMethod = new CashBuilder(this.Session).Build();
            store.DefaultCollectionMethod = collecionMethod;
            this.Session.Derive(false);

            Assert.Contains(collecionMethod, store.CollectionMethods);
        }

        [Fact]
        public void ChangedCollectionMethodsDeriveDefaultCollectionMethod()
        {
            var collecionMethod = new CashBuilder(this.Session).Build();
            var store = new StoreBuilder(this.Session).WithCollectionMethod(collecionMethod).Build();
            this.Session.Derive(false);

            Assert.Equal(collecionMethod, store.DefaultCollectionMethod);
        }

        [Fact]
        public void ChangedInternalOrganisationDeriveDefaultCollectionMethod()
        {
            var store = new StoreBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(this.InternalOrganisation.DefaultCollectionMethod, store.DefaultCollectionMethod);
        }

        [Fact]
        public void ChangedInternalOrganisationDeriveSalesInvoiceNumberCounter()
        {
            this.InternalOrganisation.InvoiceSequence = new InvoiceSequences(this.Session).EnforcedSequence;
            this.Session.Derive(false);

            var store = new StoreBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.True(store.ExistSalesInvoiceNumberCounter);
        }

        [Fact]
        public void ChangedInternalOrganisationDeriveOutgoingShipmentNumberCounter()
        {
            this.InternalOrganisation.CustomerShipmentSequence = new CustomerShipmentSequences(this.Session).EnforcedSequence;
            this.Session.Derive(false);

            var store = new StoreBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.True(store.ExistOutgoingShipmentNumberCounter);
        }

        [Fact]
        public void ChangedFiscalYearsStoreSequenceNumbersThrowValidationError()
        {
            var store = new StoreBuilder(this.Session).WithSalesInvoiceNumberCounter(new CounterBuilder(this.Session).Build()).Build();
            this.Session.Derive(false);

            store.AddFiscalYearsStoreSequenceNumber(new FiscalYearStoreSequenceNumbersBuilder(this.Session).Build());

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("AssertExistsAtMostOne: Store.FiscalYearsStoreSequenceNumbers\nStore.SalesInvoiceNumberCounter"));
        }

        [Fact]
        public void ChangedSalesInvoiceNumberCounterThrowValidationError()
        {
            var store = new StoreBuilder(this.Session).WithFiscalYearsStoreSequenceNumber(new FiscalYearStoreSequenceNumbersBuilder(this.Session).Build()).Build();
            this.Session.Derive(false);

            store.SalesInvoiceNumberCounter = new CounterBuilder(this.Session).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("AssertExistsAtMostOne: Store.FiscalYearsStoreSequenceNumbers\nStore.SalesInvoiceNumberCounter"));
        }
    }
}
