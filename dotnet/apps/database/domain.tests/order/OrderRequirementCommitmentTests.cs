// <copyright file="OrderRequirementCommitmentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class OrderRequirementCommitmentTests : DomainTest, IClassFixture<Fixture>
    {
        public OrderRequirementCommitmentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenOrderRequirementCommitment_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var shipToCustomer = new OrganisationBuilder(this.Transaction).WithName("shipToCustomer").Build();
            var billToCustomer = new OrganisationBuilder(this.Transaction).WithName("billToCustomer").Build();

            new CustomerRelationshipBuilder(this.Transaction)
                .WithCustomer(billToCustomer)

                .Build();

            new CustomerRelationshipBuilder(this.Transaction)
                .WithCustomer(shipToCustomer)

                .Build();

            var good = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good1");

            this.Transaction.Derive();

            var salesOrder = new SalesOrderBuilder(this.Transaction)
                .WithShipToCustomer(shipToCustomer)
                .WithBillToCustomer(billToCustomer)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .Build();

            var goodOrderItem = new SalesOrderItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithProduct(good)
                .WithAssignedUnitPrice(1)
                .WithQuantityOrdered(1)
                .Build();

            salesOrder.AddSalesOrderItem(goodOrderItem);

            var customerRequirement = new RequirementBuilder(this.Transaction).WithDescription("100 gizmo's").Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var builder = new OrderRequirementCommitmentBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithOrderItem(goodOrderItem);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithRequirement(customerRequirement);
            var tsts = builder.Build();

            Assert.False(this.Derive().HasErrors);
        }
    }
}
