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
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            var order = new SalesOrderBuilder(this.Session).WithOrderDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithProduct(product).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Equal(1, order.SalesOrderItemsByProduct.First.QuantityOrdered);
        }

        [Fact]
        public void ChangedSalesOrderItemProductDeriveValueOrdered()
        {
            var product = new NonUnifiedGoodBuilder(this.Session).Build();

            var order = new SalesOrderBuilder(this.Session).WithOrderDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithProduct(product).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Equal(1, order.SalesOrderItemsByProduct.First.ValueOrdered);
        }

        [Fact]
        public void ChangedSalesOrderItemVersionProductDeriveValueOrdered()
        {
            var product1 = new NonUnifiedGoodBuilder(this.Session).Build();
            var product2 = new NonUnifiedGoodBuilder(this.Session).Build();

            var order = new SalesOrderBuilder(this.Session).WithOrderDate(this.Session.Now()).Build();
            this.Session.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Session).WithProduct(product1).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            order.AddSalesOrderItem(orderItem);
            this.Session.Derive(false);

            orderItem.Product = product2;
            this.Session.Derive(false);

            Assert.Equal(0, product1.SalesOrderItemByProductsWhereProduct.First.ValueOrdered);
        }
    }
}
