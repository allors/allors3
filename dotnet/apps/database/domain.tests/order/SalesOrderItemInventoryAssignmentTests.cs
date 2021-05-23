// <copyright file="SalesOrderItemInventoryAssignmentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class SalesOrderItemInventoryAssignmentTests : DomainTest, IClassFixture<Fixture>
    {
        private readonly InventoryTransactionReasons reasons;
        private readonly SalesOrderItem salesOrderItem;
        private readonly Part part;

        public SalesOrderItemInventoryAssignmentTests(Fixture fixture) : base(fixture)
        {
            this.reasons = new InventoryTransactionReasons(this.Transaction);

            var customer = new PersonBuilder(this.Transaction).WithFirstName("Koen").Build();
            var internalOrganisation = this.InternalOrganisation;

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();

            this.part = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();

            var good = new NonUnifiedGoodBuilder(this.Transaction)
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("10101")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithName("good")
                .WithPart(this.part)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .Build();

            this.Transaction.Derive();

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(this.part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(11)
                .Build();

            this.Transaction.Derive();

            var salesOrder = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .WithAssignedBillToContactMechanism(new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build())
                .Build();

            this.salesOrderItem = new SalesOrderItemBuilder(this.Transaction)
                .WithProduct(good)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            salesOrder.AddSalesOrderItem(this.salesOrderItem);

            this.Transaction.Derive();

            salesOrder.SetReadyForPosting();
            this.Transaction.Derive();

            salesOrder.Post();
            this.Transaction.Derive();

            salesOrder.Accept();
            this.Transaction.Derive();

            this.Transaction.Commit();
        }

        [Fact]
        public void GivenSalesOrderItem_WhenAddedToOrder_ThenInventoryReservationCreated()
        {
            Assert.True(this.salesOrderItem.SalesOrderItemState.IsInProcess);
            Assert.Single(this.salesOrderItem.SalesOrderItemInventoryAssignments);
            var transactions = this.salesOrderItem.SalesOrderItemInventoryAssignments.First.InventoryItemTransactions;

            Assert.Single(transactions);
            var transaction = transactions[0];
            Assert.Equal(this.part, transaction.Part);
            Assert.Equal(3, transaction.Quantity);
            Assert.Equal(this.reasons.Reservation, transaction.Reason);

            Assert.Equal(3, this.salesOrderItem.QuantityReserved);
            Assert.Equal(3, this.salesOrderItem.QuantityCommittedOut);

            Assert.Equal(3, ((NonSerialisedInventoryItem)this.part.InventoryItemsWherePart.First()).QuantityCommittedOut);
            Assert.Equal(11, ((NonSerialisedInventoryItem)this.part.InventoryItemsWherePart.First()).QuantityOnHand);

            Assert.Equal(3, this.part.QuantityCommittedOut);
            Assert.Equal(11, this.part.QuantityOnHand);
        }

        //[Fact]
        //public void GivenSalesOrderItem_WhenChangingInventoryItem_ThenInventoryReservationsChange()
        //{
        //    var facility2 = new FacilityBuilder(this.Transaction)
        //        .WithFacilityType(new FacilityTypes(this.Transaction).Warehouse)
        //        .WithName("facility 2")
        //        .WithOwner(this.InternalOrganisation)
        //        .Build();

        //    new InventoryItemTransactionBuilder(this.Transaction)
        //        .WithPart(this.part)
        //        .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
        //        .WithFacility(facility2)
        //        .WithQuantity(10)
        //        .Build();

        //    this.Transaction.Derive();

        //    this.salesOrderItem.ReservedFromNonSerialisedInventoryItem = (NonSerialisedInventoryItem)this.part.InventoryItemsWherePart.Single(v => v.Facility.Equals(facility2));

        //    this.Transaction.Derive();

        //    Assert.True(this.salesOrderItem.SalesOrderItemState.InProcess);
        //    Assert.Equal(2, this.salesOrderItem.SalesOrderItemInventoryAssignments.Count);

        //    var previousInventoryItem = (NonSerialisedInventoryItem)this.part.InventoryItemsWherePart.FirstOrDefault(v => v.Facility.Name.Equals("facility"));
        //    var currentInventoryItem = this.salesOrderItem.ReservedFromNonSerialisedInventoryItem;

        //    Assert.Equal(11, previousInventoryItem.QuantityOnHand);
        //    Assert.Equal(0, previousInventoryItem.QuantityCommittedOut);
        //    Assert.Equal(11, previousInventoryItem.AvailableToPromise);

        //    Assert.Equal(10, currentInventoryItem.QuantityOnHand);
        //    Assert.Equal(3, currentInventoryItem.QuantityCommittedOut);
        //    Assert.Equal(7, currentInventoryItem.AvailableToPromise);

        //    Assert.Equal(3, this.salesOrderItem.QuantityReserved);

        //    Assert.Equal(3, this.part.QuantityCommittedOut);
        //    Assert.Equal(21, this.part.QuantityOnHand);
        //}

        [Fact]
        public void GivenSalesOrderItem_WhenChangingQuantity_ThenInventoryReservationsChange()
        {
            this.salesOrderItem.QuantityOrdered = 6;

            this.Transaction.Derive();

            var transaction = this.salesOrderItem.SalesOrderItemInventoryAssignments.First.InventoryItemTransactions.Last();
            Assert.Equal(this.part, transaction.Part);
            Assert.Equal(3, transaction.Quantity);
            Assert.Equal(this.reasons.Reservation, transaction.Reason);

            var inventoryItem = (NonSerialisedInventoryItem)this.part.InventoryItemsWherePart.First();

            Assert.Equal(11, inventoryItem.QuantityOnHand);
            Assert.Equal(6, inventoryItem.QuantityCommittedOut);
            Assert.Equal(5, inventoryItem.AvailableToPromise);

            Assert.Equal(6, this.salesOrderItem.QuantityReserved);

            Assert.Equal(6, this.part.QuantityCommittedOut);
            Assert.Equal(11, this.part.QuantityOnHand);
        }
    }
}
