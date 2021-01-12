// <copyright file="QuoteTests.cs" company="Allors bvba">
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

    public class QuoteTests : DomainTest, IClassFixture<Fixture>
    {
        public QuoteTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenProductQuote_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var party = new PersonBuilder(this.Session).WithLastName("party").Build();

            this.Session.Commit();

            var builder = new ProductQuoteBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithReceiver(party);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithFullfillContactMechanism(new WebAddressBuilder(this.Session).WithElectronicAddressString("test").Build());
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenProposal_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var party = new PersonBuilder(this.Session).WithLastName("party").Build();

            this.Session.Commit();

            var builder = new ProposalBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithReceiver(party);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithFullfillContactMechanism(new WebAddressBuilder(this.Session).WithElectronicAddressString("test").Build());
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenStatementOfWork_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var party = new PersonBuilder(this.Session).WithLastName("party").Build();

            this.Session.Commit();

            var builder = new StatementOfWorkBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithReceiver(party);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithFullfillContactMechanism(new WebAddressBuilder(this.Session).WithElectronicAddressString("test").Build());
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenProductQuote_WhenDeriving_ThenTotalPriceIsDerivedFromItems()
        {
            var party = new PersonBuilder(this.Session).WithLastName("party").Build();

            var good = new Goods(this.Session).FindBy(this.M.Good.Name, "good1");

            var quote = new ProductQuoteBuilder(this.Session)
                .WithReceiver(party)
                .WithFullfillContactMechanism(new WebAddressBuilder(this.Session).WithElectronicAddressString("test").Build())
                .Build();

            var item1 = new QuoteItemBuilder(this.Session).WithProduct(good).WithQuantity(1).WithAssignedUnitPrice(1000).Build();
            var item2 = new QuoteItemBuilder(this.Session).WithProduct(good).WithQuantity(3).WithAssignedUnitPrice(100).Build();

            quote.AddQuoteItem(item1);
            quote.AddQuoteItem(item2);

            this.Session.Derive();

            Assert.Equal(1300, quote.TotalExVat);
        }

        [Fact]
        public void GivenIssuerWithoutQuoteNumberPrefix_WhenDeriving_ThenSortableQuoteNumberIsSet()
        {
            this.InternalOrganisation.RemoveQuoteNumberPrefix();
            this.Session.Derive();

            var party = new PersonBuilder(this.Session).WithLastName("party").Build();

            var quote = new ProductQuoteBuilder(this.Session)
                .WithReceiver(party)
                .WithFullfillContactMechanism(new WebAddressBuilder(this.Session).WithElectronicAddressString("test").Build())
                .Build();

            this.Session.Derive();

            Assert.Equal(int.Parse(quote.QuoteNumber), quote.SortableQuoteNumber);
        }

        [Fact]
        public void GivenIssuerWithQuoteNumberPrefix_WhenDeriving_ThenSortableQuoteNumberIsSet()
        {
            this.InternalOrganisation.QuoteSequence = new QuoteSequences(this.Session).EnforcedSequence;
            this.InternalOrganisation.QuoteNumberPrefix = "prefix-";
            this.Session.Derive();

            var party = new PersonBuilder(this.Session).WithLastName("party").Build();

            var quote = new ProductQuoteBuilder(this.Session)
                .WithReceiver(party)
                .WithFullfillContactMechanism(new WebAddressBuilder(this.Session).WithElectronicAddressString("test").Build())
                .Build();

            this.Session.Derive();

            Assert.Equal(int.Parse(quote.QuoteNumber.Split('-')[1]), quote.SortableQuoteNumber);
        }

        [Fact]
        public void GivenIssuerWithParametrizedQuoteNumberPrefix_WhenDeriving_ThenSortableQuoteNumberIsSet()
        {
            this.InternalOrganisation.QuoteSequence = new QuoteSequences(this.Session).EnforcedSequence;
            this.InternalOrganisation.QuoteNumberPrefix = "prefix-{year}-";
            this.Session.Derive();

            var party = new PersonBuilder(this.Session).WithLastName("party").Build();

            var quote = new ProductQuoteBuilder(this.Session)
                .WithReceiver(party)
                .WithFullfillContactMechanism(new WebAddressBuilder(this.Session).WithElectronicAddressString("test").Build())
                .WithIssueDate(this.Session.Now().Date)
                .Build();

            this.Session.Derive();

            Assert.Equal(int.Parse(string.Concat(this.Session.Now().Date.Year.ToString(), quote.QuoteNumber.Split('-').Last())), quote.SortableQuoteNumber);
        }
    }

    public class ProductQuoteDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductQuoteDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedIssuerThrowValidationError()
        {
            var quote = new ProductQuoteBuilder(this.Session).Build();
            this.Session.Derive(false);

            quote.Issuer = new OrganisationBuilder(this.Session).WithIsInternalOrganisation(true).Build();

            var expectedError = $"{quote} {this.M.ProductQuote.Issuer} {ErrorMessages.InternalOrganisationChanged}";
            Assert.Equal(expectedError, this.Session.Derive(false).Errors[0].Message);
        }

        [Fact]
        public void ChangedQuoteItemsDeriveValidQuoteItems()
        {
            var quote = new ProductQuoteBuilder(this.Session).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).Build();
            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            Assert.Contains(quoteItem, quote.ValidQuoteItems);
        }

        [Fact]
        public void ChangedQuoteItemQuoteItemStateDeriveValidQuoteItems()
        {
            var quote = new ProductQuoteBuilder(this.Session).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).Build();
            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            quoteItem.Cancel();
            this.Session.Derive(false);

            Assert.DoesNotContain(quoteItem, quote.ValidQuoteItems);
        }

        [Fact]
        public void ChangedIssuerDeriveWorkItemDescription()
        {
            var quote = new ProductQuoteBuilder(this.Session).Build();
            this.Session.Derive(false);

            var expected = $"ProductQuote: {quote.QuoteNumber} [{quote.Issuer?.PartyName}]";
            Assert.Equal(expected, quote.WorkItemDescription);
        }

        [Fact]
        public void ChangedQuoteItemsDeriveQuoteItemSyncedQuote()
        {
            var quote = new ProductQuoteBuilder(this.Session).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).Build();
            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            Assert.Equal(quoteItem.SyncedQuote, quote);
        }
    }

    public class ProductQuoteAwaitingApprovalDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductQuoteAwaitingApprovalDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedQuoteStateCreateApprovalTask()
        {
            var quote = this.InternalOrganisation.CreateB2BProductQuoteWithSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            quote.QuoteState = new QuoteStates(this.Session).AwaitingApproval;
            this.Session.Derive(false);

            Assert.True(quote.ExistProductQuoteApprovalsWhereProductQuote);
        }
    }

    public class ProductQuotePriceDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductQuotePriceDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnChangedValidQuoteItemsCalculatePrice()
        {
            var quote = this.InternalOrganisation.CreateB2BProductQuoteWithSerialisedItem(this.Session.Faker());
            this.Session.Derive();

            Assert.True(quote.TotalIncVat > 0);
            var totalIncVatBefore = quote.TotalIncVat;

            quote.QuoteItems.First.Cancel();
            this.Session.Derive();

            Assert.Equal(quote.TotalIncVat, totalIncVatBefore - quote.QuoteItems.First.TotalIncVat);
        }

        [Fact]
        public void ChangedQuoteItemsCalculatePriceForProductFeature()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).WithNonSerialisedDefaults(this.InternalOrganisation).Build();

            new BasePriceBuilder(this.Session)
                .WithPricedBy(this.InternalOrganisation)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Session).WithIssueDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            var productFeature = new ColourBuilder(this.Session)
                .WithName("a colour")
                .Build();

            new BasePriceBuilder(this.Session)
                .WithPricedBy(this.InternalOrganisation)
                .WithProductFeature(productFeature)
                .WithPrice(0.2M)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            this.Session.Derive(false);

            var featureItem = new QuoteItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductFeatureItem).WithProductFeature(productFeature).WithQuantity(1).Build();
            quoteItem.AddQuotedWithFeature(featureItem);
            this.Session.Derive(false);

            Assert.Equal(1.2M, quote.TotalExVat);
        }

        [Fact]
        public void OnChangedDerivationTriggerTriggeredByPriceComponentFromDateCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            var basePrice = new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddDays(1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Session).WithIssueDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);

            var expectedMessage = $"{quoteItem}, {this.M.QuoteItem.UnitBasePrice} No BasePrice with a Price";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Contains(expectedMessage));

            Assert.Equal(0, quote.TotalIncVat);

            basePrice.FromDate = this.Session.Now().AddMinutes(-1);
            this.Session.Derive(false);

            Assert.Equal(basePrice.Price, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedDerivationTriggerTriggeredByDiscountComponentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Session).WithIssueDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            new DiscountComponentBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(0.1M)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            this.Session.Derive(false);

            Assert.Equal(0.9M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedDerivationTriggerTriggeredBySurchargeComponentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Session).WithIssueDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            new SurchargeComponentBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(0.1M)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            this.Session.Derive(false);

            Assert.Equal(1.1M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedQuoteItemQuantityCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            var quote = new ProductQuoteBuilder(this.Session).WithIssueDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).WithProduct(product).WithQuantity(1).WithAssignedUnitPrice(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            quoteItem.Quantity = 2;
            this.Session.Derive(false);

            Assert.Equal(2, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedQuoteItemAssignedUnitPriceCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            var quote = new ProductQuoteBuilder(this.Session).WithIssueDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).WithProduct(product).WithQuantity(1).WithAssignedUnitPrice(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            quoteItem.AssignedUnitPrice = 3;
            this.Session.Derive(false);

            Assert.Equal(3, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedQuoteItemProductCalculatePrice()
        {
            var product1 = new NonUnifiedGoodBuilder(this.Session).Build();
            var product2 = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product1)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product2)
                .WithPrice(2)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Session).WithIssueDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product1).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            quoteItem.Product = product2;
            this.Session.Derive(false);

            Assert.Equal(2, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedQuoteItemProductFeatureCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithPricedBy(this.InternalOrganisation)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Session).WithIssueDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            var productFeature = new ColourBuilder(this.Session)
                .WithName("a colour")
                .Build();

            new BasePriceBuilder(this.Session)
                .WithPricedBy(this.InternalOrganisation)
                .WithProduct(product)
                .WithProductFeature(productFeature)
                .WithPrice(0.1M)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            this.Session.Derive(false);

            var orderFeatureItem = new QuoteItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductFeatureItem).WithProductFeature(productFeature).WithQuantity(1).Build();
            quoteItem.AddQuotedWithFeature(orderFeatureItem);
            this.Session.Derive(false);

            Assert.Equal(1.1M, quote.TotalExVat);
        }

        [Fact]
        public void OnChangedReceiverCalculatePrice()
        {
            var theGood = new CustomOrganisationClassificationBuilder(this.Session).WithName("good customer").Build();
            var theBad = new CustomOrganisationClassificationBuilder(this.Session).WithName("bad customer").Build();
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            var customer1 = this.InternalOrganisation.ActiveCustomers.First;
            customer1.AddPartyClassification(theGood);

            var customer2 = this.InternalOrganisation.ActiveCustomers.Last();
            customer2.AddPartyClassification(theBad);

            this.Session.Derive(false);

            Assert.NotEqual(customer1, customer2);

            new BasePriceBuilder(this.Session)
                .WithPartyClassification(theGood)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            new BasePriceBuilder(this.Session)
                .WithPartyClassification(theBad)
                .WithProduct(product)
                .WithPrice(2)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Session).WithReceiver(customer1).WithIssueDate(this.Session.Now()).WithAssignedVatRegime(new VatRegimes(this.Session).Exempt).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            quote.Receiver = customer2;
            this.Session.Derive(false);

            Assert.Equal(2, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedQuoteItemDiscountAdjustmentsCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Session).WithIssueDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Session).WithPercentage(10).Build();
            quoteItem.AddDiscountAdjustment(discount);
            this.Session.Derive(false);

            Assert.Equal(0.9M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedQuoteItemDiscountAdjustmentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Session).WithIssueDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Session).WithPercentage(10).Build();
            quoteItem.AddDiscountAdjustment(discount);
            this.Session.Derive(false);

            Assert.Equal(0.9M, quote.TotalIncVat);

            discount.Percentage = 20M;
            this.Session.Derive(false);

            Assert.Equal(0.8M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedQuoteItemDiscountAdjustmentAmountCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Session).WithIssueDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Session).WithAmount(0.5M).Build();
            quoteItem.AddDiscountAdjustment(discount);
            this.Session.Derive(false);

            Assert.Equal(0.5M, quote.TotalIncVat);

            discount.Amount = 0.4M;
            this.Session.Derive(false);

            Assert.Equal(0.6M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedQuoteItemSurchargeAdjustmentsCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Session).WithIssueDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Session).WithPercentage(10).Build();
            quoteItem.AddSurchargeAdjustment(surcharge);
            this.Session.Derive(false);

            Assert.Equal(1.1M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedQuoteItemSurchargeAdjustmentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Session).WithIssueDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Session).WithPercentage(10).Build();
            quoteItem.AddSurchargeAdjustment(surcharge);
            this.Session.Derive(false);

            Assert.Equal(1.1M, quote.TotalIncVat);

            surcharge.Percentage = 20M;
            this.Session.Derive(false);

            Assert.Equal(1.2M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedQuoteItemSurchargeAdjustmentAmountCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Session).WithIssueDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Session).WithAmount(0.5M).Build();
            quoteItem.AddSurchargeAdjustment(surcharge);
            this.Session.Derive(false);

            Assert.Equal(1.5M, quote.TotalIncVat);

            surcharge.Amount = 0.4M;
            this.Session.Derive(false);

            Assert.Equal(1.4M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedDiscountAdjustmentsCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Session).WithIssueDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Session).WithPercentage(10).Build();
            quote.AddOrderAdjustment(discount);
            this.Session.Derive(false);

            Assert.Equal(0.9M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedDiscountAdjustmentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Session).WithIssueDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Session).WithPercentage(10).Build();
            quote.AddOrderAdjustment(discount);
            this.Session.Derive(false);

            Assert.Equal(0.9M, quote.TotalIncVat);

            discount.Percentage = 20M;
            this.Session.Derive(false);

            Assert.Equal(0.8M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedDiscountAdjustmentAmountCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Session).WithIssueDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Session).WithAmount(0.5M).Build();
            quote.AddOrderAdjustment(discount);
            this.Session.Derive(false);

            Assert.Equal(0.5M, quote.TotalIncVat);

            discount.Amount = 0.4M;
            this.Session.Derive(false);

            Assert.Equal(0.6M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedSurchargeAdjustmentsCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Session).WithIssueDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Session).WithPercentage(10).Build();
            quote.AddOrderAdjustment(surcharge);
            this.Session.Derive(false);

            Assert.Equal(1.1M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedSurchargeAdjustmentPercentageCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Session).WithIssueDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Session).WithPercentage(10).Build();
            quote.AddOrderAdjustment(surcharge);
            this.Session.Derive(false);

            Assert.Equal(1.1M, quote.TotalIncVat);

            surcharge.Percentage = 20M;
            this.Session.Derive(false);

            Assert.Equal(1.2M, quote.TotalIncVat);
        }

        [Fact]
        public void OnChangedSurchargeAdjustmentAmountCalculatePrice()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            new BasePriceBuilder(this.Session)
                .WithProduct(product)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();

            var quote = new ProductQuoteBuilder(this.Session).WithIssueDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).ProductItem).WithProduct(product).WithQuantity(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Session.Derive(false);

            Assert.Equal(1, quote.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Session).WithAmount(0.5M).Build();
            quote.AddOrderAdjustment(surcharge);
            this.Session.Derive(false);

            Assert.Equal(1.5M, quote.TotalIncVat);

            surcharge.Amount = 0.4M;
            this.Session.Derive(false);

            Assert.Equal(1.4M, quote.TotalIncVat);
        }
    }

    [Trait("Category", "Security")]
    public class ProductQuoteDeniedPermissionDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductQuoteDeniedPermissionDerivationTests(Fixture fixture) : base(fixture)
        {
            this.deletePermission = new Permissions(this.Session).Get(this.M.ProductQuote.ObjectType, this.M.ProductQuote.Delete);
            this.setReadyPermission = new Permissions(this.Session).Get(this.M.ProductQuote.ObjectType, this.M.ProductQuote.SetReadyForProcessing);
        }

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;
        private readonly Permission setReadyPermission;

        [Fact]
        public void OnChangedProductQuoteQuoteItemStateCreatedDeriveSetReadyPermission()
        {
            var productQuote = new ProductQuoteBuilder(this.Session).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session)
                    .WithInvoiceItemType(new InvoiceItemTypeBuilder(this.Session).Build())
                    .WithAssignedUnitPrice(1)
                    .Build();
            productQuote.AddQuoteItem(quoteItem);

            this.Session.Derive(false);

            Assert.DoesNotContain(this.setReadyPermission, productQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProductQuoteWithoutQuoteItemStateCreatedDeriveSetReadyPermission()
        {
            var productQuote = new ProductQuoteBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Contains(this.setReadyPermission, productQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProductQuoteStateNotCreatedDeriveSetReadyPermission()
        {
            var productQuote = this.InternalOrganisation.CreateB2BProductQuoteWithSerialisedItem(this.Session.Faker());
            this.Session.Derive(false);

            productQuote.Send();
            this.Session.Derive(false);

            Assert.True(productQuote.QuoteState.IsAwaitingAcceptance);
            Assert.Contains(this.setReadyPermission, productQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProductQuoteStateCreatedDeriveDeletePermission()
        {
            var productQuote = new ProductQuoteBuilder(this.Session).Build();

            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, productQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProductQuoteStateCreatedWithRequestDeriveDeletePermission()
        {
            var request = new RequestForQuoteBuilder(this.Session).Build();
            this.Session.Derive(false);

            var productQuote = new ProductQuoteBuilder(this.Session).WithRequest(request).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, productQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProductQuoteStateApprovalDeriveDeletePermission()
        {
            var productQuote = new ProductQuoteBuilder(this.Session).Build();
            this.Session.Derive(false);

            productQuote.Send();
            this.Session.Derive(false);

            productQuote.Accept();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, productQuote.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProductQuoteStateOrderedWithSalesOrderDeriveDeletePermission()
        {
            var party = new PersonBuilder(this.Session).WithLastName("party").Build();

            var good = new Goods(this.Session).FindBy(this.M.Good.Name, "good1");
             
            var quote = new ProductQuoteBuilder(this.Session)
                .WithReceiver(party)
                .WithFullfillContactMechanism(new WebAddressBuilder(this.Session).WithElectronicAddressString("test").Build())
                .Build();

            var item1 = new QuoteItemBuilder(this.Session).WithProduct(good).WithQuantity(1).WithAssignedUnitPrice(1000).Build();
            var item2 = new QuoteItemBuilder(this.Session).WithProduct(good).WithQuantity(3).WithAssignedUnitPrice(100).Build();

            quote.AddQuoteItem(item1);
            quote.AddQuoteItem(item2);

            this.Session.Derive(false);

            quote.Send();
            this.Session.Derive(false);

            quote.Accept();
            this.Session.Derive(false);

            quote.Send();
            this.Session.Derive(false);

            var productQuoteOrder = quote.Order();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, quote.DeniedPermissions);
        }
    }

    [Trait("Category", "Security")]
    public class ProposalDeniedPermissionDerivationTestsTests : DomainTest, IClassFixture<Fixture>
    {
        public ProposalDeniedPermissionDerivationTestsTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Session).Get(this.M.Proposal.ObjectType, this.M.Proposal.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;


        [Fact]
        public void OnChangedProposalStateCreatedDeriveDeletePermission()
        {
            var proposal = new ProposalBuilder(this.Session).Build();

            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, proposal.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProposalStateZDeriveDeletePermission()
        {
            var proposal = new ProposalBuilder(this.Session).Build();
            this.Session.Derive(false);

            proposal.Accept();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, proposal.DeniedPermissions);
        }
    }
}
