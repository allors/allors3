// <copyright file="SalesOrderItemTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class SalesOrderItemByProductTestsTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderItemByProductTestsTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSalesOrderItemProductDeriveQuantityOrdered()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithProduct(product).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            Assert.Equal(1, order.SalesOrderItemsByProduct.First.QuantityOrdered);
        }

        [Fact]
        public void ChangedSalesOrderItemProductDeriveValueOrdered()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithProduct(product).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            Assert.Equal(1, order.SalesOrderItemsByProduct.First.ValueOrdered);
        }

        [Fact]
        public void ChangedSalesOrderItemVersionProductDeriveValueOrdered()
        {
            var product1 = new NonUnifiedGoodBuilder(this.Transaction).Build();
            var product2 = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var order = new SalesOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithProduct(product1).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Derive();

            orderItem.Product = product2;
            this.Derive();

            Assert.Equal(0, product1.SalesOrderItemByProductsWhereProduct.First.ValueOrdered);
        }
    }
}
