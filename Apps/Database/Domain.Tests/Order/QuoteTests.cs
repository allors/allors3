// <copyright file="QuoteCreatedDerivationTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Resources;
    using TestPopulation;
    using Xunit;

    public class QuoteOnBuildTests : DomainTest, IClassFixture<Fixture>
    {
        public QuoteOnBuildTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DeriveSalesOrderState()
        {
            var quote = new ProductQuoteBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            Assert.True(quote.ExistQuoteState);
        }
    }

    public class QuoteCreatedDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public QuoteCreatedDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedQuoteStateDeriveDerivedLocaleFromLocale()
        {
            var swedishLocale = new LocaleBuilder(this.Transaction)
               .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "SE"))
               .WithLanguage(new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "sv"))
               .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithQuoteState(new QuoteStates(this.Transaction).Cancelled).Build();
            this.Transaction.Derive(false);

            quote.Locale = swedishLocale;
            this.Transaction.Derive(false);

            quote.QuoteState = new QuoteStates(this.Transaction).Created;
            this.Transaction.Derive(false);

            Assert.Equal(quote.DerivedLocale, swedishLocale);
        }

        [Fact]
        public void ChangedReceiverDeriveDerivedLocaleFromReceiverLocale()
        {
            var swedishLocale = new LocaleBuilder(this.Transaction)
               .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "SE"))
               .WithLanguage(new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "sv"))
               .Build();

            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.Locale = swedishLocale;

            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            quote.Receiver = customer;
            this.Transaction.Derive(false);

            Assert.Equal(quote.DerivedLocale, customer.Locale);
        }

        [Fact]
        public void ChangedReceiverDeriveDerivedLocaleFromIssuerLocale()
        {
            var swedishLocale = new LocaleBuilder(this.Transaction)
               .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "SE"))
               .WithLanguage(new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "sv"))
               .Build();

            this.InternalOrganisation.Locale = swedishLocale;

            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.RemoveLocale();

            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            quote.Receiver = customer;
            this.Transaction.Derive(false);

            Assert.False(customer.ExistLocale);
            Assert.Equal(quote.DerivedLocale, swedishLocale);
        }

        [Fact]
        public void ChangedLocaleDeriveDerivedLocaleFromLocale()
        {
            var swedishLocale = new LocaleBuilder(this.Transaction)
               .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "SE"))
               .WithLanguage(new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "sv"))
               .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            quote.Locale = swedishLocale;
            this.Transaction.Derive(false);

            Assert.Equal(quote.DerivedLocale, swedishLocale);
        }

        [Fact]
        public void ChangedAssignedVatRegimeDeriveDerivedVatRegime()
        {
            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            quote.AssignedVatRegime = new VatRegimes(this.Transaction).ServiceB2B;
            this.Transaction.Derive(false);

            Assert.Equal(quote.DerivedVatRegime, quote.AssignedVatRegime);
        }

        [Fact]
        public void ChangedReceiverDeriveDerivedVatRegime()
        {
            var customer1 = this.InternalOrganisation.ActiveCustomers.First;
            customer1.VatRegime = new VatRegimes(this.Transaction).Assessable10;

            var customer2 = this.InternalOrganisation.CreateB2BCustomer(this.Transaction.Faker());
            customer2.VatRegime = new VatRegimes(this.Transaction).Assessable21;

            var quote = new ProductQuoteBuilder(this.Transaction).WithReceiver(customer1).Build();
            this.Transaction.Derive(false);

            quote.Receiver = customer2;
            this.Transaction.Derive(false);

            Assert.Equal(quote.DerivedVatRegime, customer2.VatRegime);
        }

        [Fact]
        public void ChangedReceiverVatRegimeDeriveDerivedVatRegime()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;

            var quote = new ProductQuoteBuilder(this.Transaction).WithReceiver(customer).Build();
            this.Transaction.Derive(false);

            customer.VatRegime = new VatRegimes(this.Transaction).Assessable10;
            this.Transaction.Derive(false);

            Assert.Equal(quote.DerivedVatRegime, customer.VatRegime);
        }

        [Fact]
        public void ChangedAssignedIrpfRegimeDeriveDerivedIrpfRegime()
        {
            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            quote.AssignedIrpfRegime = new IrpfRegimes(this.Transaction).Assessable15;
            this.Transaction.Derive(false);

            Assert.Equal(quote.DerivedIrpfRegime, quote.AssignedIrpfRegime);
        }

        [Fact]
        public void ChangedReceiverDeriveDerivedIrpfRegime()
        {
            var customer1 = this.InternalOrganisation.ActiveCustomers.First;
            customer1.IrpfRegime = new IrpfRegimes(this.Transaction).Assessable15;

            var customer2 = this.InternalOrganisation.CreateB2BCustomer(this.Transaction.Faker());
            customer2.IrpfRegime = new IrpfRegimes(this.Transaction).Assessable19;

            var quote = new ProductQuoteBuilder(this.Transaction).WithReceiver(customer1).Build();
            this.Transaction.Derive(false);

            quote.Receiver = customer2;
            this.Transaction.Derive(false);

            Assert.Equal(quote.DerivedVatRegime, customer2.VatRegime);
        }

        [Fact]
        public void ChangedReceiverIrpfRegimeDeriveDerivedIrpfRegime()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;

            var quote = new ProductQuoteBuilder(this.Transaction).WithReceiver(customer).Build();
            this.Transaction.Derive(false);

            customer.IrpfRegime = new IrpfRegimes(this.Transaction).Assessable15;
            this.Transaction.Derive(false);

            Assert.Equal(quote.DerivedIrpfRegime, customer.IrpfRegime);
        }

        [Fact]
        public void ChangedAssignedCurrencyDeriveDerivedCurrency()
        {
            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var swedishKrona = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "SEK");
            quote.AssignedCurrency = swedishKrona;
            this.Transaction.Derive(false);

            Assert.Equal(quote.DerivedCurrency, quote.AssignedCurrency);
        }

        [Fact]
        public void ChangedIssuerPreferredCurrencyDeriveDerivedCurrency()
        {
            Assert.True(this.InternalOrganisation.ExistPreferredCurrency);

            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(quote.DerivedCurrency, this.InternalOrganisation.PreferredCurrency);
        }

        [Fact]
        public void ChangedReceiverLocaleDeriveDerivedCurrency()
        {
            var se = new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "SE");
            var newLocale = new LocaleBuilder(this.Transaction)
                .WithCountry(se)
                .WithLanguage(new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "sv"))
                .Build();

            var customer = this.InternalOrganisation.ActiveCustomers.First;

            var quote = new ProductQuoteBuilder(this.Transaction).WithReceiver(customer).Build();
            this.Transaction.Derive(false);

            customer.RemovePreferredCurrency();
            customer.Locale = newLocale;
            this.Transaction.Derive(false);

            Assert.Equal(quote.DerivedCurrency, se.Currency);
        }

        [Fact]
        public void ChangedReceiverPreferredCurrencyDeriveDerivedCurrency()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.RemoveLocale();
            customer.RemovePreferredCurrency();

            var quote = new ProductQuoteBuilder(this.Transaction).WithReceiver(customer).Build();
            this.Transaction.Derive(false);

            var swedishKrona = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "SEK");
            customer.PreferredCurrency = swedishKrona;
            this.Transaction.Derive(false);

            Assert.Equal(quote.DerivedCurrency, swedishKrona);
        }

        [Fact]
        public void ChangedReceiverDeriveCurrencyFromReceiverLocale()
        {
            var newLocale = new LocaleBuilder(this.Transaction)
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "SE"))
                .WithLanguage(new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "sv"))
                .Build();

            var customer = this.InternalOrganisation.ActiveCustomers.First;
            customer.Locale = newLocale;
            customer.RemovePreferredCurrency();

            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            quote.Receiver = customer;
            this.Transaction.Derive(false);

            Assert.Equal(quote.DerivedCurrency, newLocale.Country.Currency);
        }
    }

    public class QuoteDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public QuoteDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedStoreDeriveOrderNumber()
        {
            this.InternalOrganisation.RemoveQuoteNumberPrefix();
            var number = this.InternalOrganisation.QuoteNumberCounter.Value;

            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(quote.QuoteNumber, (number + 1).ToString());
        }

        [Fact]
        public void ChangedStoreDeriveSortableOrderNumber()
        {
            var number = this.InternalOrganisation.QuoteNumberCounter.Value;

            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(quote.SortableQuoteNumber.Value, number + 1);
        }

        [Fact]
        public void ChangedQuoteItemsDeriveValidQuoteItems()
        {
            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            Assert.Contains(quoteItem, quote.ValidQuoteItems);
        }

        [Fact]
        public void ChangedQuoteItemQuoteItemStateDeriveValidQuoteItems()
        {
            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            quoteItem.Cancel();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(quoteItem, quote.ValidQuoteItems);
        }

        [Fact]
        public void ChangedQuoteItemsDeriveQuoteItemSyncedQuote()
        {
            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            Assert.Equal(quoteItem.SyncedQuote, quote);
        }
    }

    public class QuotePriceDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public QuotePriceDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnChangedQuoteStateCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithQuoteState(new QuoteStates(this.Transaction).Cancelled).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            Assert.Equal(0, quote.TotalIncVat);

            quote.QuoteState = new QuoteStates(this.Transaction).Created;
            this.Transaction.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedValidQuoteItemsCalculatePrice()
        {
            var quote = this.InternalOrganisation.CreateB2BProductQuoteWithSerialisedItem(this.Transaction.Faker());
            this.Transaction.Derive();

            Assert.True(quote.TotalIncVat > 0);
            var totalIncVatBefore = quote.TotalIncVat;

            quote.QuoteItems.First.Cancel();
            this.Transaction.Derive();

            Assert.Equal(quote.TotalIncVat, totalIncVatBefore - quote.QuoteItems.First.TotalIncVat);
        }

        [Fact]
        public void ChangedQuoteItemsCalculatePriceForProductFeature()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).WithNonSerialisedDefaults(this.InternalOrganisation).Build();

            new BasePriceBuilder(this.Transaction)
                .WithPricedBy(this.InternalOrganisation)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssueDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            var productFeature = new ColourBuilder(this.Transaction)
                .WithName("a colour")
                .Build();

            new BasePriceBuilder(this.Transaction)
                .WithPricedBy(this.InternalOrganisation)
                .WithProductFeature(productFeature)
                .WithPrice(0.2M)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            this.Transaction.Derive(false);

            var featureItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductFeatureItem).WithProductFeature(productFeature).WithQuantity(1).Build();
            quoteItem.AddQuotedWithFeature(featureItem);
            this.Transaction.Derive(false);

            Assert.Equal(1.2M, quote.TotalExVat);
        }

        [Fact]
        public void OnChangedDerivationTriggerTriggeredByPriceComponentFromDateCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var basePrice = new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssueDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);

            var expectedMessage = $"{quoteItem}, {this.M.QuoteItem.UnitBasePrice} No BasePrice with a Price";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(expectedMessage));

            Assert.Equal(0, quote.TotalIncVat);

            basePrice.FromDate = this.Transaction.Now().AddMinutes(-1);
            this.Transaction.Derive(false);

            Assert.Equal(basePrice.Price, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedDerivationTriggerTriggeredByDiscountComponentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssueDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            new DiscountComponentBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(0.1M)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            this.Transaction.Derive(false);

            Assert.Equal(0.9M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedDerivationTriggerTriggeredBySurchargeComponentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssueDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            new SurchargeComponentBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(0.1M)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            this.Transaction.Derive(false);

            Assert.Equal(1.1M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedQuoteItemQuantityCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssueDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithProduct(product).WithQuantity(1).WithAssignedUnitPrice(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            quoteItem.Quantity = 2;
            this.Transaction.Derive(false);

            Assert.Equal(2, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedQuoteItemAssignedUnitPriceCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssueDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithProduct(product).WithQuantity(1).WithAssignedUnitPrice(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            quoteItem.AssignedUnitPrice = 3;
            this.Transaction.Derive(false);

            Assert.Equal(3, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedQuoteItemProductCalculatePrice()
        {
            var product1 = new NonUnifiedGoodBuilder(this.Transaction).Build();
            var product2 = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product1)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product2)
                .WithPrice(2)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssueDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product1).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            quoteItem.Product = product2;
            this.Transaction.Derive(false);

            Assert.Equal(2, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedQuoteItemProductFeatureCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithPricedBy(this.InternalOrganisation)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssueDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            var productFeature = new ColourBuilder(this.Transaction)
                .WithName("a colour")
                .Build();

            new BasePriceBuilder(this.Transaction)
                .WithPricedBy(this.InternalOrganisation)
                .WithProduct(product)
                .WithProductFeature(productFeature)
                .WithPrice(0.1M)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            this.Transaction.Derive(false);

            var orderFeatureItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductFeatureItem).WithProductFeature(productFeature).WithQuantity(1).Build();
            quoteItem.AddQuotedWithFeature(orderFeatureItem);
            this.Transaction.Derive(false);

            Assert.Equal(1.1M, quote.TotalExVat);
        }

        [Fact]
        public void OnChangedReceiverCalculatePrice()
        {
            var theGood = new CustomOrganisationClassificationBuilder(this.Transaction).WithName("good customer").Build();
            var theBad = new CustomOrganisationClassificationBuilder(this.Transaction).WithName("bad customer").Build();
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var customer1 = this.InternalOrganisation.ActiveCustomers.First;
            customer1.AddPartyClassification(theGood);

            var customer2 = this.InternalOrganisation.ActiveCustomers.Last();
            customer2.AddPartyClassification(theBad);

            this.Transaction.Derive(false);

            Assert.NotEqual(customer1, customer2);

            new BasePriceBuilder(this.Transaction)
                .WithPartyClassification(theGood)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            new BasePriceBuilder(this.Transaction)
                .WithPartyClassification(theBad)
                .WithProduct(product)
                .WithPrice(2)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithReceiver(customer1).WithIssueDate(this.Transaction.Now()).WithAssignedVatRegime(new VatRegimes(this.Transaction).Exempt).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            quote.Receiver = customer2;
            this.Transaction.Derive(false);

            Assert.Equal(2, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedQuoteItemDiscountAdjustmentsCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssueDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            quoteItem.AddDiscountAdjustment(discount);
            this.Transaction.Derive(false);

            Assert.Equal(0.9M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedQuoteItemDiscountAdjustmentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssueDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            quoteItem.AddDiscountAdjustment(discount);
            this.Transaction.Derive(false);

            Assert.Equal(0.9M, quote.TotalIncVat);

            discount.Percentage = 20M;
            this.Transaction.Derive(false);

            Assert.Equal(0.8M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedQuoteItemDiscountAdjustmentAmountCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssueDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            quoteItem.AddDiscountAdjustment(discount);
            this.Transaction.Derive(false);

            Assert.Equal(0.5M, quote.TotalIncVat);

            discount.Amount = 0.4M;
            this.Transaction.Derive(false);

            Assert.Equal(0.6M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedQuoteItemSurchargeAdjustmentsCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssueDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            quoteItem.AddSurchargeAdjustment(surcharge);
            this.Transaction.Derive(false);

            Assert.Equal(1.1M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedQuoteItemSurchargeAdjustmentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssueDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            quoteItem.AddSurchargeAdjustment(surcharge);
            this.Transaction.Derive(false);

            Assert.Equal(1.1M, quote.TotalIncVat);

            surcharge.Percentage = 20M;
            this.Transaction.Derive(false);

            Assert.Equal(1.2M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedQuoteItemSurchargeAdjustmentAmountCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssueDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            quoteItem.AddSurchargeAdjustment(surcharge);
            this.Transaction.Derive(false);

            Assert.Equal(1.5M, quote.TotalIncVat);

            surcharge.Amount = 0.4M;
            this.Transaction.Derive(false);

            Assert.Equal(1.4M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedDiscountAdjustmentsCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssueDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            quote.AddOrderAdjustment(discount);
            this.Transaction.Derive(false);

            Assert.Equal(0.9M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedDiscountAdjustmentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssueDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            quote.AddOrderAdjustment(discount);
            this.Transaction.Derive(false);

            Assert.Equal(0.9M, quote.TotalIncVat);

            discount.Percentage = 20M;
            this.Transaction.Derive(false);

            Assert.Equal(0.8M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedDiscountAdjustmentAmountCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssueDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            quote.AddOrderAdjustment(discount);
            this.Transaction.Derive(false);

            Assert.Equal(0.5M, quote.TotalIncVat);

            discount.Amount = 0.4M;
            this.Transaction.Derive(false);

            Assert.Equal(0.6M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedSurchargeAdjustmentsCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssueDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            quote.AddOrderAdjustment(surcharge);
            this.Transaction.Derive(false);

            Assert.Equal(1.1M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedSurchargeAdjustmentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssueDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            quote.AddOrderAdjustment(surcharge);
            this.Transaction.Derive(false);

            Assert.Equal(1.1M, quote.TotalIncVat);

            surcharge.Percentage = 20M;
            this.Transaction.Derive(false);

            Assert.Equal(1.2M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedSurchargeAdjustmentAmountCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            new BasePriceBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssueDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            quote.AddOrderAdjustment(surcharge);
            this.Transaction.Derive(false);

            Assert.Equal(1.5M, quote.TotalIncVat);

            surcharge.Amount = 0.4M;
            this.Transaction.Derive(false);

            Assert.Equal(1.4M, quote.TotalIncVat);
        }
    }
}
