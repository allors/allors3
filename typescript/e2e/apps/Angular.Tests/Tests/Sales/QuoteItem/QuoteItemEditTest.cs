
// <copyright file="PartyContactMechanismEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;

namespace Tests.ProductQuoteItemTests
{
    using Allors.Database.Domain;
    using Components;
    using libs.workspace.angular.apps.src.lib.objects.productquote.list;
    using libs.workspace.angular.apps.src.lib.objects.productquote.overview;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class QuoteItemEditTest : Test, IClassFixture<Fixture>
    {
        private readonly ProductQuoteListComponent productQuotes;

        public QuoteItemEditTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.productQuotes = this.Sidenav.NavigateToProductQuotes();
        }

        [Fact]
        public void EditWithProductItem()
        {
            var before = new QuoteItems(this.Transaction).Extent().ToArray();

            var productQuote = new ProductQuotes(this.Transaction).Extent().First();
            var product = new Products(this.Transaction).Extent().First();

            var expected = new QuoteItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithProduct(product)
                .WithQuantity(1M)
                .WithTotalBasePrice(10M)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .Build();

            this.Transaction.Derive();

            var expectedInvoiceItemType = expected.InvoiceItemType;
            var expectedQuantity = expected.Quantity;
            var expectedProduct = expected.Product;
            var expectedUnitOfMeasure = expected.UnitOfMeasure;
            var expectedTotalBasePrice = expected.TotalBasePrice;

            this.productQuotes.Table.DefaultAction(productQuote);
            var productQuotesDetails = new ProductQuoteOverviewComponent(this.productQuotes.Driver, this.M);
            var quoteItemDetail = productQuotesDetails.QuoteitemOverviewPanel.Click().CreateQuoteItem();

            quoteItemDetail
                .QuoteItemInvoiceItemType_1.Select(expectedInvoiceItemType)
                .Quantity.Set(expectedQuantity.ToString())
                .UnitOfMeasure.Select(expectedUnitOfMeasure)
                .Product.Select(product.Name)
                .PriceableAssignedUnitPrice_1.Set(expectedTotalBasePrice.ToString());

            this.Transaction.Rollback();
            quoteItemDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new QuoteItems(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var quoteItem = after.Except(before).First();

            Assert.Equal(expectedInvoiceItemType, quoteItem.InvoiceItemType);
            Assert.Equal(expectedQuantity, quoteItem.Quantity); 
            Assert.Equal(expectedProduct, quoteItem.Product);
            Assert.Equal(expectedUnitOfMeasure, quoteItem.UnitOfMeasure);
            Assert.Equal(expectedTotalBasePrice, quoteItem.TotalBasePrice);
        }

        [Fact]
        public void EditWithService()
        {
            var before = new QuoteItems(this.Transaction).Extent().ToArray();

            var productQuote = new ProductQuotes(this.Transaction).Extent().First();

            var expected = new QuoteItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Service)
                .WithTotalBasePrice(10M)
                .Build();

            this.Transaction.Derive();

            var expectedInvoiceItemType = expected.InvoiceItemType;
            var expectedTotalBasePrice = expected.TotalBasePrice;

            this.productQuotes.Table.DefaultAction(productQuote);
            var productQuotesDetails = new ProductQuoteOverviewComponent(this.productQuotes.Driver, this.M);
            var quoteItemDetail = productQuotesDetails.QuoteitemOverviewPanel.Click().CreateQuoteItem();

            quoteItemDetail
                .QuoteItemInvoiceItemType_1.Select(expectedInvoiceItemType)
                .PriceableAssignedUnitPrice_1.Set(expectedTotalBasePrice.ToString());

            this.Transaction.Rollback();
            quoteItemDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new QuoteItems(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var quoteItem = after.Except(before).First();

            Assert.Equal(expectedInvoiceItemType, quoteItem.InvoiceItemType);
            Assert.Equal(expectedTotalBasePrice, quoteItem.TotalBasePrice);
        }

        [Fact]
        public void EditWithTime()
        {
            var before = new QuoteItems(this.Transaction).Extent().ToArray();

            var productQuote = new ProductQuotes(this.Transaction).Extent().First();

            var expected = new QuoteItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Time)
                .WithTotalBasePrice(10M)
                .Build();

            this.Transaction.Derive();

            var expectedInvoiceItemType = expected.InvoiceItemType;
            var expectedTotalBasePrice = expected.TotalBasePrice;


            this.productQuotes.Table.DefaultAction(productQuote);
            var productQuotesDetails = new ProductQuoteOverviewComponent(this.productQuotes.Driver, this.M);
            var quoteItemDetail = productQuotesDetails.QuoteitemOverviewPanel.Click().CreateQuoteItem();

            quoteItemDetail
                .QuoteItemInvoiceItemType_1.Select(expectedInvoiceItemType)
                .PriceableAssignedUnitPrice_1.Set(expectedTotalBasePrice.ToString());

            this.Transaction.Rollback();
            quoteItemDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new QuoteItems(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var quoteItem = after.Except(before).First();

            Assert.Equal(expectedInvoiceItemType, quoteItem.InvoiceItemType);
            Assert.Equal(expectedTotalBasePrice, quoteItem.TotalBasePrice);
        }
    }
}
