// <copyright file="PurchaseOrderItemByProductTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class PurchaseOrderItemByProductTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderItemByProductTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProductQuoteItemProductDeriveQuantityOrdered()
        {
            var product = new UnifiedGoodBuilder(this.Session).Build();

            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).PartItem).WithPart(product).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Equal(1, order.PurchaseOrderItemsByProduct.First.QuantityOrdered);
        }

        [Fact]
        public void ChangedProductQuoteItemProductDeriveValueOrdered()
        {
            var product = new UnifiedGoodBuilder(this.Session).Build();

            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).PartItem).WithPart(product).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Equal(1, order.PurchaseOrderItemsByProduct.First.ValueOrdered);
        }

        [Fact]
        public void ChangedProductQuoteItemVersionProductDeriveValueOrdered()
        {
            var product1 = new UnifiedGoodBuilder(this.Session).Build();
            var product2 = new UnifiedGoodBuilder(this.Session).Build();

            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Session).WithInvoiceItemType(new InvoiceItemTypes(this.Session).PartItem).WithPart(product1).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Session.Derive(false);

            orderItem.Part = product2;
            this.Session.Derive(false);

            Assert.Equal(0, product1.PurchaseOrderItemByProductsWhereUnifiedProduct.First.ValueOrdered);
        }
    }
}
