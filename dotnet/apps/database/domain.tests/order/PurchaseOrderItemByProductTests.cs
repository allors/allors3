// <copyright file="PurchaseOrderItemByProductTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class PurchaseOrderItemByProductTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderItemByProductTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProductQuoteItemProductDeriveQuantityOrdered()
        {
            var product = new UnifiedGoodBuilder(this.Transaction).Build();

            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem).WithPart(product).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            Assert.Equal(1, order.PurchaseOrderItemsByProduct.First().QuantityOrdered);
        }

        [Fact]
        public void ChangedProductQuoteItemProductDeriveValueOrdered()
        {
            var product = new UnifiedGoodBuilder(this.Transaction).Build();

            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem).WithPart(product).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            Assert.Equal(1, order.PurchaseOrderItemsByProduct.First().ValueOrdered);
        }

        [Fact]
        public void ChangedProductQuoteItemVersionProductDeriveValueOrdered()
        {
            var product1 = new UnifiedGoodBuilder(this.Transaction).Build();
            var product2 = new UnifiedGoodBuilder(this.Transaction).Build();

            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem).WithPart(product1).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            orderItem.Part = product2;
            this.Derive();

            Assert.Equal(0, product1.PurchaseOrderItemByProductsWhereUnifiedProduct.First().ValueOrdered);
        }
    }
}
