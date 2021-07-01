// <copyright file="QuoteCreatedRuleTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using TestPopulation;
    using Xunit;

    public class QuoteOnBuildTests : DomainTest, IClassFixture<Fixture>
    {
        public QuoteOnBuildTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DeriveSalesOrderState()
        {
            var quote = new ProductQuoteBuilder(this.Transaction).Build();

            this.Derive();

            Assert.True(quote.ExistQuoteState);
        }
    }

    public class QuoteCreatedRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public QuoteCreatedRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedQuoteStateDeriveDerivedLocaleFromLocale()
        {
            var swedishLocale = new LocaleBuilder(this.Transaction)
               .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "SE"))
               .WithLanguage(new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "sv"))
               .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithQuoteState(new QuoteStates(this.Transaction).Cancelled).Build();
            this.Derive();

            quote.Locale = swedishLocale;
            this.Derive();

            quote.QuoteState = new QuoteStates(this.Transaction).Created;
            this.Derive();

            Assert.Equal(quote.DerivedLocale, swedishLocale);
        }

        [Fact]
        public void ChangedReceiverDeriveDerivedLocaleFromReceiverLocale()
        {
            var swedishLocale = new LocaleBuilder(this.Transaction)
               .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "SE"))
               .WithLanguage(new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "sv"))
               .Build();

            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            customer.Locale = swedishLocale;

            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            quote.Receiver = customer;
            this.Derive();

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

            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            customer.RemoveLocale();

            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            quote.Receiver = customer;
            this.Derive();

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
            this.Derive();

            quote.Locale = swedishLocale;
            this.Derive();

            Assert.Equal(quote.DerivedLocale, swedishLocale);
        }

        [Fact]
        public void ChangedAssignedVatRegimeDeriveDerivedVatRegime()
        {
            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            quote.AssignedVatRegime = new VatRegimes(this.Transaction).ServiceB2B;
            this.Derive();

            Assert.Equal(quote.DerivedVatRegime, quote.AssignedVatRegime);
        }

        [Fact]
        public void ChangedAssignedIrpfRegimeDeriveDerivedIrpfRegime()
        {
            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            quote.AssignedIrpfRegime = new IrpfRegimes(this.Transaction).Assessable15;
            this.Derive();

            Assert.Equal(quote.DerivedIrpfRegime, quote.AssignedIrpfRegime);
        }

        [Fact]
        public void ChangedAssignedCurrencyDeriveDerivedCurrency()
        {
            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            var swedishKrona = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "SEK");
            quote.AssignedCurrency = swedishKrona;
            this.Derive();

            Assert.Equal(quote.DerivedCurrency, quote.AssignedCurrency);
        }

        [Fact]
        public void ChangedIssuerPreferredCurrencyDeriveDerivedCurrency()
        {
            Assert.True(this.InternalOrganisation.ExistPreferredCurrency);

            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

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

            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();

            var quote = new ProductQuoteBuilder(this.Transaction).WithReceiver(customer).Build();
            this.Derive();

            customer.RemovePreferredCurrency();
            customer.Locale = newLocale;
            this.Derive();

            Assert.Equal(quote.DerivedCurrency, se.Currency);
        }

        [Fact]
        public void ChangedReceiverPreferredCurrencyDeriveDerivedCurrency()
        {
            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            customer.RemoveLocale();
            customer.RemovePreferredCurrency();

            var quote = new ProductQuoteBuilder(this.Transaction).WithReceiver(customer).Build();
            this.Derive();

            var swedishKrona = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "SEK");
            customer.PreferredCurrency = swedishKrona;
            this.Derive();

            Assert.Equal(quote.DerivedCurrency, swedishKrona);
        }

        [Fact]
        public void ChangedReceiverDeriveCurrencyFromReceiverLocale()
        {
            var newLocale = new LocaleBuilder(this.Transaction)
                .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "SE"))
                .WithLanguage(new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "sv"))
                .Build();

            var customer = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            customer.Locale = newLocale;
            customer.RemovePreferredCurrency();

            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            quote.Receiver = customer;
            this.Derive();

            Assert.Equal(quote.DerivedCurrency, newLocale.Country.Currency);
        }

        [Fact]
        public void ChangedIssueDateDeriveVatRate()
        {
            var vatRegime = new VatRegimes(this.Transaction).SpainReduced;
            vatRegime.VatRates.ElementAt(0).ThroughDate = this.Transaction.Now().AddDays(-1).Date;
            this.Derive();

            var newVatRate = new VatRateBuilder(this.Transaction).WithFromDate(this.Transaction.Now().Date).WithRate(11).Build();
            vatRegime.AddVatRate(newVatRate);
            this.Derive();

            var quote = new ProductQuoteBuilder(this.Transaction)
                .WithIssueDate(this.Transaction.Now().AddDays(-1).Date)
                .WithAssignedVatRegime(vatRegime).Build();
            this.Derive();

            Assert.NotEqual(newVatRate, quote.DerivedVatRate);

            quote.IssueDate = this.Transaction.Now().AddDays(1).Date;
            this.Derive();

            Assert.Equal(newVatRate, quote.DerivedVatRate);
        }
    }

    public class QuoteRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public QuoteRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedStoreDeriveOrderNumber()
        {
            this.InternalOrganisation.RemoveQuoteNumberPrefix();
            var number = this.InternalOrganisation.QuoteNumberCounter.Value;

            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(quote.QuoteNumber, (number + 1).ToString());
        }

        [Fact]
        public void ChangedStoreDeriveSortableOrderNumber()
        {
            var number = this.InternalOrganisation.QuoteNumberCounter.Value;

            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(quote.SortableQuoteNumber.Value, number + 1);
        }

        [Fact]
        public void ChangedQuoteItemsDeriveValidQuoteItems()
        {
            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            Assert.Contains(quoteItem, quote.ValidQuoteItems);
        }

        [Fact]
        public void ChangedQuoteItemQuoteItemStateDeriveValidQuoteItems()
        {
            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            quoteItem.Cancel();
            this.Derive();

            Assert.DoesNotContain(quoteItem, quote.ValidQuoteItems);
        }

        [Fact]
        public void ChangedQuoteItemsDeriveQuoteItemSyncedQuote()
        {
            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            Assert.Equal(quoteItem.SyncedQuote, quote);
        }
    }

    public class QuotePriceRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public QuotePriceRuleTests(Fixture fixture) : base(fixture) { }

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
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            Assert.Equal(0, quote.TotalIncVat);

            quote.QuoteState = new QuoteStates(this.Transaction).Created;
            this.Derive();

            Assert.Equal(1, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedValidQuoteItemsCalculatePrice()
        {
            var quote = this.InternalOrganisation.CreateB2BProductQuoteWithSerialisedItem(this.Transaction.Faker());
            this.Transaction.Derive();

            Assert.True(quote.TotalIncVat > 0);
            var totalIncVatBefore = quote.TotalIncVat;

            quote.QuoteItems.First().Cancel();
            this.Transaction.Derive();

            Assert.Equal(quote.TotalIncVat, totalIncVatBefore - quote.QuoteItems.First().TotalIncVat);
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
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            var productFeature = new ColourBuilder(this.Transaction)
                .WithName("a colour")
                .Build();

            new BasePriceBuilder(this.Transaction)
                .WithPricedBy(this.InternalOrganisation)
                .WithProductFeature(productFeature)
                .WithPrice(0.2M)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            this.Derive();

            var featureItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductFeatureItem).WithProductFeature(productFeature).WithQuantity(1).Build();
            quoteItem.AddQuotedWithFeature(featureItem);
            this.Derive();

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
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains("No BasePrice with a Price"));

            Assert.Equal(0, quote.TotalIncVat);

            basePrice.FromDate = this.Transaction.Now().AddMinutes(-1);
            this.Derive();

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
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            Assert.Equal(1, quote.TotalIncVat);

            new DiscountComponentBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(0.1M)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            this.Derive();

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
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            Assert.Equal(1, quote.TotalIncVat);

            new SurchargeComponentBuilder(this.Transaction)
                .WithProduct(product)
                .WithPrice(0.1M)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            this.Derive();

            Assert.Equal(1.1M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedQuoteItemQuantityCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssueDate(this.Transaction.Now()).Build();
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithProduct(product).WithQuantity(1).WithAssignedUnitPrice(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            Assert.Equal(1, quote.TotalIncVat);

            quoteItem.Quantity = 2;
            this.Derive();

            Assert.Equal(2, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedQuoteItemAssignedUnitPriceCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssueDate(this.Transaction.Now()).Build();
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithProduct(product).WithQuantity(1).WithAssignedUnitPrice(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            Assert.Equal(1, quote.TotalIncVat);

            quoteItem.AssignedUnitPrice = 3;
            this.Derive();

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
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product1).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            Assert.Equal(1, quote.TotalIncVat);

            quoteItem.Product = product2;
            this.Derive();

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
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

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

            this.Derive();

            var orderFeatureItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductFeatureItem).WithProductFeature(productFeature).WithQuantity(1).Build();
            quoteItem.AddQuotedWithFeature(orderFeatureItem);
            this.Derive();

            Assert.Equal(1.1M, quote.TotalExVat);
        }

        [Fact]
        public void OnChangedReceiverCalculatePrice()
        {
            var theGood = new CustomOrganisationClassificationBuilder(this.Transaction).WithName("good customer").Build();
            var theBad = new CustomOrganisationClassificationBuilder(this.Transaction).WithName("bad customer").Build();
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var customer1 = this.InternalOrganisation.ActiveCustomers.FirstOrDefault();
            customer1.AddPartyClassification(theGood);

            var customer2 = this.InternalOrganisation.ActiveCustomers.Last();
            customer2.AddPartyClassification(theBad);

            this.Derive();

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
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            Assert.Equal(1, quote.TotalIncVat);

            quote.Receiver = customer2;
            this.Derive();

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
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            Assert.Equal(1, quote.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            quoteItem.AddDiscountAdjustment(discount);
            this.Derive();

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
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            Assert.Equal(1, quote.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            quoteItem.AddDiscountAdjustment(discount);
            this.Derive();

            Assert.Equal(0.9M, quote.TotalIncVat);

            discount.Percentage = 20M;
            this.Derive();

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
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            Assert.Equal(1, quote.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            quoteItem.AddDiscountAdjustment(discount);
            this.Derive();

            Assert.Equal(0.5M, quote.TotalIncVat);

            discount.Amount = 0.4M;
            this.Derive();

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
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            Assert.Equal(1, quote.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            quoteItem.AddSurchargeAdjustment(surcharge);
            this.Derive();

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
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            Assert.Equal(1, quote.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            quoteItem.AddSurchargeAdjustment(surcharge);
            this.Derive();

            Assert.Equal(1.1M, quote.TotalIncVat);

            surcharge.Percentage = 20M;
            this.Derive();

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
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            Assert.Equal(1, quote.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            quoteItem.AddSurchargeAdjustment(surcharge);
            this.Derive();

            Assert.Equal(1.5M, quote.TotalIncVat);

            surcharge.Amount = 0.4M;
            this.Derive();

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
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            Assert.Equal(1, quote.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            quote.AddOrderAdjustment(discount);
            this.Derive();

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
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            Assert.Equal(1, quote.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            quote.AddOrderAdjustment(discount);
            this.Derive();

            Assert.Equal(0.9M, quote.TotalIncVat);

            discount.Percentage = 20M;
            this.Derive();

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
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            Assert.Equal(1, quote.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            quote.AddOrderAdjustment(discount);
            this.Derive();

            Assert.Equal(0.5M, quote.TotalIncVat);

            discount.Amount = 0.4M;
            this.Derive();

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
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            Assert.Equal(1, quote.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            quote.AddOrderAdjustment(surcharge);
            this.Derive();

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
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            Assert.Equal(1, quote.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            quote.AddOrderAdjustment(surcharge);
            this.Derive();

            Assert.Equal(1.1M, quote.TotalIncVat);

            surcharge.Percentage = 20M;
            this.Derive();

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
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            Assert.Equal(1, quote.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            quote.AddOrderAdjustment(surcharge);
            this.Derive();

            Assert.Equal(1.5M, quote.TotalIncVat);

            surcharge.Amount = 0.4M;
            this.Derive();

            Assert.Equal(1.4M, quote.TotalIncVat);
        }
    }
}
