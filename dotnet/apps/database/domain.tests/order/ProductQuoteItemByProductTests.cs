// <copyright file="ProductQuoteItemByProductTestsTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class ProductQuoteItemByProductTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductQuoteItemByProductTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProductQuoteItemProductDeriveQuantityOrdered()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithProduct(product).WithQuantity(1).WithAssignedUnitPrice(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            Assert.Equal(1, quote.ProductQuoteItemsByProduct.First.QuantityOrdered);
        }

        [Fact]
        public void ChangedProductQuoteItemProductDeriveValueOrdered()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithProduct(product).WithQuantity(1).WithAssignedUnitPrice(1).Build();
            quote.AddQuoteItem(quoteItem);
            this.Derive();

            Assert.Equal(1, quote.ProductQuoteItemsByProduct.First.ValueOrdered);
        }

        [Fact]
        public void ChangedProductQuoteItemVersionProductDeriveValueOrdered()
        {
            var product1 = new NonUnifiedGoodBuilder(this.Transaction).Build();
            var product2 = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Derive();

            var QuoteItem = new QuoteItemBuilder(this.Transaction).WithProduct(product1).WithQuantity(1).WithAssignedUnitPrice(1).Build();
            quote.AddQuoteItem(QuoteItem);
            this.Derive();

            QuoteItem.Product = product2;
            this.Derive();

            Assert.Equal(0, product1.ProductQuoteItemByProductsWhereProduct.First.ValueOrdered);
        }
    }
}
