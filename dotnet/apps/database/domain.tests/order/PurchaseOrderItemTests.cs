// <copyright file="PurchaseOrderItemTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using TestPopulation;
    using Database.Derivations;
    using Resources;
    using Xunit;

    public class PurchaseOrderItemTests : DomainTest, IClassFixture<Fixture>
    {
        private Part finishedGood;
        private SupplierOffering currentPurchasePrice;
        private PurchaseOrder order;
        private Organisation supplier;

        public PurchaseOrderItemTests(Fixture fixture) : base(fixture)
        {
            var euro = new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR");

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            ContactMechanism takenViaContactMechanism = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var supplierContactMechanism = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(takenViaContactMechanism)
                .WithUseAsDefault(true)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .Build();

            this.supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            this.supplier.AddPartyContactMechanism(supplierContactMechanism);

            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(this.supplier).Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            this.finishedGood = good1.Part;

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithSupplier(this.supplier)
                .WithFromDate(this.Transaction.Now().AddYears(-1))
                .WithThroughDate(this.Transaction.Now().AddDays(-1))
                .WithCurrency(euro)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPrice(8)
                .Build();

            this.currentPurchasePrice = new SupplierOfferingBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithSupplier(this.supplier)
                .WithFromDate(this.Transaction.Now())
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .WithCurrency(euro)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPrice(10)
                .Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithSupplier(this.supplier)
                .WithFromDate(this.Transaction.Now().AddYears(1))
                .WithCurrency(euro)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPrice(8)
                .Build();

            this.order = new PurchaseOrderBuilder(this.Transaction)
                .WithTakenViaSupplier(this.supplier)
                .WithAssignedBillToContactMechanism(takenViaContactMechanism)
                .WithDeliveryDate(this.Transaction.Now())
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).Exempt)
                .WithAssignedIrpfRegime(new IrpfRegimes(this.Transaction).Assessable19)
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();
        }

        [Fact]
        public void GivenOrderItemForSubcontractingWork_WhenDerived_ThenOrderQuantityMustBeEqualTo1()
        {
            this.InstantiateObjects(this.Transaction);

            var item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithDescription("Do Something")
                .WithQuantityOrdered(0)
                .WithAssignedUnitPrice(1)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            var expectedMessage = $"{item} { this.M.PurchaseOrderItem.QuantityOrdered} { ErrorMessages.InvalidQuantity}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));

            this.Transaction.Rollback();

            item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithDescription("Do Something")
                .WithQuantityOrdered(2)
                .WithAssignedUnitPrice(1)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));

            this.Transaction.Rollback();

            item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithDescription("Do Something")
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Service)
                .WithQuantityOrdered(1)
                .WithAssignedUnitPrice(1)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenOrderItemForSerialisedPart_WhenDerived_ThenOrderQuantityMustBeEqualTo1()
        {
            this.InstantiateObjects(this.Transaction);

            var serialisedPart = new UnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(this.InternalOrganisation).Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(serialisedPart)
                .WithSerialNumber("1")
                .WithQuantityOrdered(0)
                .WithAssignedUnitPrice(1)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            var expectedMessage = $"{item} { this.M.PurchaseOrderItem.QuantityOrdered} { ErrorMessages.InvalidQuantity}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));

            this.Transaction.Rollback();

            item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(serialisedPart)
                .WithSerialNumber("1")
                .WithQuantityOrdered(2)
                .WithAssignedUnitPrice(1)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));

            this.Transaction.Rollback();

            item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(serialisedPart)
                .WithAssignedUnitPrice(1)
                .WithQuantityOrdered(1)
                .WithSerialNumber("1")
                .Build();

            this.order.AddPurchaseOrderItem(item);

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenOrderItemForNonSerialisedPart_WhenDerived_ThenOrderQuantityMustBeGreaterEqual1()
        {
            this.InstantiateObjects(this.Transaction);

            var nonSerialisedPart = new UnifiedGoodBuilder(this.Transaction).WithNonSerialisedDefaults(this.InternalOrganisation).Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(nonSerialisedPart)
                .WithQuantityOrdered(0)
                .WithAssignedUnitPrice(1)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            var expectedMessage = $"{item} { this.M.PurchaseOrderItem.QuantityOrdered} { ErrorMessages.InvalidQuantity}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));

            this.Transaction.Rollback();

            item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(nonSerialisedPart)
                .WithQuantityOrdered(2)
                .WithAssignedUnitPrice(1)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            Assert.False(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(nonSerialisedPart)
                .WithAssignedUnitPrice(1)
                .WithQuantityOrdered(2)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenConfirmedOrderItemForGood_WhenOrderItemIsRemoved_ThenItemIsRemovedFromValidOrderItems()
        {
            this.InstantiateObjects(this.Transaction);

            var item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            this.Transaction.Derive();

            this.order.SetReadyForProcessing();
            this.Transaction.Derive();

            Assert.Single(this.order.ValidOrderItems);

            item.Cancel();

            this.Transaction.Derive();

            Assert.Empty(this.order.ValidOrderItems);
        }

        [Fact]
        public void GivenOrderItemForPart_WhenDerivingPrices_ThenUsePartCurrentPurchasePrice()
        {
            this.InstantiateObjects(this.Transaction);

            const decimal QuantityOrdered = 3;
            var item1 = new PurchaseOrderItemBuilder(this.Transaction).WithPart(this.finishedGood).WithQuantityOrdered(QuantityOrdered).Build();
            this.order.AddPurchaseOrderItem(item1);

            this.Transaction.Derive();

            Assert.Equal(this.currentPurchasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentPurchasePrice.Price, item1.UnitPrice);

            Assert.Equal(this.currentPurchasePrice.Price * QuantityOrdered, item1.TotalBasePrice);
            Assert.Equal(0, item1.TotalDiscount);
            Assert.Equal(0, item1.TotalSurcharge);
            Assert.Equal(this.currentPurchasePrice.Price * QuantityOrdered, item1.TotalExVat);

            Assert.Equal(this.currentPurchasePrice.Price * QuantityOrdered, this.order.TotalBasePrice);
            Assert.Equal(0, this.order.TotalDiscount);
            Assert.Equal(0, this.order.TotalSurcharge);
            Assert.Equal(this.currentPurchasePrice.Price * QuantityOrdered, this.order.TotalExVat);
        }

        [Fact]
        public void GivenOrderItemForProduct_WhenDerivingPrices_ThenUseProductCurrentPurchasePrice()
        {
            var euro = new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR");

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithSupplier(this.supplier)
                .WithFromDate(this.Transaction.Now().AddYears(-1))
                .WithThroughDate(this.Transaction.Now().AddDays(-1))
                .WithCurrency(euro)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPrice(8)
                .Build();

            var currentOffer = new SupplierOfferingBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithSupplier(this.supplier)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .WithPrice(10)
                .WithCurrency(euro)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithSupplier(this.supplier)
                .WithFromDate(this.Transaction.Now().AddYears(1))
                .WithPrice(8)
                .WithCurrency(euro)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            const decimal QuantityOrdered = 3;
            var item1 = new PurchaseOrderItemBuilder(this.Transaction).WithPart(this.finishedGood).WithQuantityOrdered(QuantityOrdered).Build();
            this.order.AddPurchaseOrderItem(item1);

            this.Transaction.Derive();

            Assert.Equal(currentOffer.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(currentOffer.Price, item1.UnitPrice);

            Assert.Equal(currentOffer.Price * QuantityOrdered, item1.TotalBasePrice);
            Assert.Equal(0, item1.TotalDiscount);
            Assert.Equal(0, item1.TotalSurcharge);
            Assert.Equal(currentOffer.Price * QuantityOrdered, item1.TotalExVat);

            Assert.Equal(currentOffer.Price * QuantityOrdered, this.order.TotalBasePrice);
            Assert.Equal(0, this.order.TotalDiscount);
            Assert.Equal(0, this.order.TotalSurcharge);
            Assert.Equal(currentOffer.Price * QuantityOrdered, this.order.TotalExVat);
        }

        [Fact]
        public void GivenOrderItemForPartWithActualPrice_WhenDerivingPrices_ThenUseActualPrice()
        {
            this.InstantiateObjects(this.Transaction);

            var item1 = new PurchaseOrderItemBuilder(this.Transaction).WithPart(this.finishedGood).WithQuantityOrdered(3).WithAssignedUnitPrice(15).Build();
            this.order.AddPurchaseOrderItem(item1);

            this.Transaction.Derive();

            Assert.Equal(15, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(15, item1.UnitPrice);
            Assert.Equal(0, item1.UnitVat);
            Assert.Equal(45, item1.TotalBasePrice);
            Assert.Equal(0, item1.TotalDiscount);
            Assert.Equal(0, item1.TotalSurcharge);
            Assert.Equal(45, item1.TotalExVat);
            Assert.Equal(0, item1.TotalVat);
            Assert.Equal(45, item1.TotalIncVat);
            Assert.Equal(2.85M, item1.UnitIrpf);

            Assert.Equal(45, this.order.TotalBasePrice);
            Assert.Equal(0, this.order.TotalDiscount);
            Assert.Equal(0, this.order.TotalSurcharge);
            Assert.Equal(45, this.order.TotalExVat);
            Assert.Equal(0, this.order.TotalVat);
            Assert.Equal(45, this.order.TotalIncVat);
            Assert.Equal(8.55M, this.order.TotalIrpf);
            Assert.Equal(36.45M, this.order.GrandTotal);
        }

        [Fact]
        public void GivenOrderItemWithAssignedDeliveryDate_WhenDeriving_ThenDeliveryDateIsOrderItemAssignedDeliveryDate()
        {
            this.InstantiateObjects(this.Transaction);

            var item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithQuantityOrdered(1)
                .WithAssignedDeliveryDate(this.Transaction.Now().AddMonths(1))
                .Build();

            this.order.AddPurchaseOrderItem(item);

            this.Transaction.Derive();

            Assert.Equal(item.DerivedDeliveryDate, item.AssignedDeliveryDate);
        }

        [Fact]
        public void GivenOrderItemWithoutDeliveryDate_WhenDeriving_ThenDerivedDeliveryDateIsOrderDeliveryDate()
        {
            this.InstantiateObjects(this.Transaction);

            var item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithQuantityOrdered(1)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            this.Transaction.Derive();

            Assert.Equal(item.DerivedDeliveryDate, this.order.DeliveryDate);
        }

        private void InstantiateObjects(ITransaction transaction)
        {
            this.finishedGood = (Part)transaction.Instantiate(this.finishedGood);
            this.currentPurchasePrice = (SupplierOffering)transaction.Instantiate(this.currentPurchasePrice);
            this.order = (PurchaseOrder)transaction.Instantiate(this.order);
            this.supplier = (Organisation)transaction.Instantiate(this.supplier);
        }
    }

    public class PurchaseOrderItemOnBuildTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderItemOnBuildTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DerivePurchaseOrderItemState()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.True(orderItem.ExistPurchaseOrderItemState);
        }

        [Fact]
        public void DerivePurchaseOrderItemPaymentState()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.True(orderItem.ExistPurchaseOrderItemPaymentState);
        }

        [Fact]
        public void DeriveInvoiceItemType()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(new UnifiedGoodBuilder(this.Transaction).Build())
                .Build();

            this.Transaction.Derive(false);

            Assert.True(orderItem.ExistInvoiceItemType);
        }
    }

    public class PurchaseOrderItemCreatedRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderItemCreatedRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseOrderItemStateDeriveDerivedDeliveryDate()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).WithDeliveryDate(this.Transaction.Now().Date).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithPurchaseOrderItemState(new PurchaseOrderItemStates(this.Transaction).Finished).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            orderItem.PurchaseOrderItemState = new PurchaseOrderItemStates(this.Transaction).Created;
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.DerivedDeliveryDate, order.DeliveryDate);
        }

        [Fact]
        public void ChangedPurchaseOrderPurchaseOrderItemsDeriveDerivedDeliveryDate()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).WithDeliveryDate(this.Transaction.Now().Date).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.DerivedDeliveryDate, order.DeliveryDate);
        }

        [Fact]
        public void ChangedAssignedDeliveryDateDeriveDerivedDeliveryDate()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithAssignedDeliveryDate(this.Transaction.Now().Date).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.DerivedDeliveryDate, orderItem.AssignedDeliveryDate);
        }

        [Fact]
        public void ChangedsalesOrderDerivedDeliveryDateDeriveDerivedDeliveryDate()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).WithDeliveryDate(this.Transaction.Now().Date).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            order.DeliveryDate = this.Transaction.Now().Date;
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.DerivedDeliveryDate, order.DeliveryDate);
        }

        [Fact]
        public void ChangedAssignedVatRegimeDeriveDerivedVatRegime()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithAssignedVatRegime(new VatRegimes(this.Transaction).BelgiumStandard).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.DerivedVatRegime, orderItem.AssignedVatRegime);
        }

        [Fact]
        public void ChangedsalesOrderDerivedVatRegimeDeriveDerivedVatRegime()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            order.AssignedVatRegime = new VatRegimes(this.Transaction).BelgiumStandard;
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.DerivedVatRegime, order.AssignedVatRegime);
        }

        [Fact]
        public void ChangedDerivedVatRegimeDeriveVatRate()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            order.AssignedVatRegime = new VatRegimes(this.Transaction).BelgiumStandard;
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.VatRate, order.AssignedVatRegime.VatRates[0]);
        }

        [Fact]
        public void ChangedAssignedIrpfRegimeDeriveDerivedIrpfRegime()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithAssignedIrpfRegime(new IrpfRegimes(this.Transaction).Assessable15).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.DerivedIrpfRegime, orderItem.AssignedIrpfRegime);
        }

        [Fact]
        public void ChangedsalesOrderDerivedIrpfRegimeDeriveDerivedIrpfRegime()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            order.AssignedIrpfRegime = new IrpfRegimes(this.Transaction).Assessable15;
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.DerivedIrpfRegime, order.AssignedIrpfRegime);
        }

        [Fact]
        public void ChangedDerivedIrpfRegimeDeriveIrpfRate()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            order.AssignedIrpfRegime = new IrpfRegimes(this.Transaction).Assessable15;
            this.Transaction.Derive(false);

            Assert.Equal(orderItem.IrpfRate, order.AssignedIrpfRegime.IrpfRates[0]);
        }

        [Fact]
        public void ChangedPurchaseOrderOrderDateDeriveVatRate()
        {
            var vatRegime = new VatRegimes(this.Transaction).SpainReduced;
            vatRegime.VatRates[0].ThroughDate = this.Transaction.Now().AddDays(-1).Date;
            this.Transaction.Derive(false);

            var newVatRate = new VatRateBuilder(this.Transaction).WithFromDate(this.Transaction.Now().Date).WithRate(11).Build();
            vatRegime.AddVatRate(newVatRate);
            this.Transaction.Derive(false);

            var order = new PurchaseOrderBuilder(this.Transaction)
                .WithOrderDate(this.Transaction.Now().AddDays(-1).Date)
                .WithAssignedVatRegime(vatRegime).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.NotEqual(newVatRate, orderItem.VatRate);

            order.OrderDate = this.Transaction.Now().AddDays(1).Date;
            this.Transaction.Derive(false);

            Assert.Equal(newVatRate, orderItem.VatRate);
        }
    }

    public class PurchaseOrderItemRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderItemRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPartThrowValidationError()
        {
            var serialisedPart = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem).Build();
            this.Transaction.Derive(false);

            orderItem.Part = serialisedPart;

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("AssertAtLeastOne: PurchaseOrderItem.SerialisedItem\nPurchaseOrderItem.SerialNumber"));
        }

        [Fact]
        public void ChangedSerialisedItemThrowValidationError()
        {
            var serialisedPart = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(serialisedPart)
                .WithSerialNumber("number")
                .Build();
            this.Transaction.Derive(false);

            orderItem.SerialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("AssertExistsAtMostOne: PurchaseOrderItem.SerialisedItem\nPurchaseOrderItem.SerialNumber"));
        }

        [Fact]
        public void ChangedSerialNumberThrowValidationError()
        {
            var serialisedPart = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(serialisedPart)
                .WithSerialisedItem(new SerialisedItemBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            orderItem.SerialNumber = "number";

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("AssertExistsAtMostOne: PurchaseOrderItem.SerialisedItem\nPurchaseOrderItem.SerialNumber"));
        }

        [Fact]
        public void ChangedQuantityOrderedThrowValidationError_1()
        {
            var serialisedPart = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(serialisedPart)
                .WithSerialisedItem(new SerialisedItemBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            orderItem.QuantityOrdered = 2;

            var expectedMessage = $"{orderItem} { this.M.PurchaseOrderItem.QuantityOrdered} { ErrorMessages.InvalidQuantity}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedQuantityOrderedThrowValidationError_2()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithSerialisedItem(new SerialisedItemBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            orderItem.QuantityOrdered = 2;

            var expectedMessage = $"{orderItem} { this.M.PurchaseOrderItem.QuantityOrdered} { ErrorMessages.InvalidQuantity}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedQuantityOrderedThrowValidationError_3()
        {
            var nonSerialisedPart = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(nonSerialisedPart)
                .Build();
            this.Transaction.Derive(false);

            orderItem.QuantityOrdered = 0;

            var expectedMessage = $"{orderItem} { this.M.PurchaseOrderItem.QuantityOrdered} { ErrorMessages.InvalidQuantity}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedOrderItemBillingOrderItemDeriveCanInvoice_1()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new OrderItemBillingBuilder(this.Transaction).WithOrderItem(orderItem).Build();
            this.Transaction.Derive(false);

            Assert.False(orderItem.CanInvoice);
        }

        [Fact]
        public void ChangedOrderItemBillingOrderItemDeriveCanInvoice_2()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var billing = new OrderItemBillingBuilder(this.Transaction).WithOrderItem(orderItem).Build();
            this.Transaction.Derive(false);

            Assert.False(orderItem.CanInvoice);

            billing.Delete();
            this.Transaction.Derive(false);

            Assert.True(orderItem.CanInvoice);
        }
    }

    public class PurchaseOrderItemIsReceivableRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderItemIsReceivableRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedInvoiceItemTypeDeriveIsReceivable()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            orderItem.InvoiceItemType = new InvoiceItemTypes(this.Transaction).PartItem;
            this.Transaction.Derive(false);

            Assert.True(orderItem.IsReceivable);
        }

        [Fact]
        public void ChangedPartDeriveIsReceivable()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem)
                .Build();
            this.Transaction.Derive(false);

            orderItem.Part = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.True(orderItem.IsReceivable);
        }
    }

    public class PurchaseOrderItemStateRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderItemStateRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedIsReceivableDerivePurchaseOrderItemShipmentStateNotReceived()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithIsReceivable(true)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(new PurchaseOrderItemShipmentStates(this.Transaction).NotReceived, orderItem.PurchaseOrderItemShipmentState);
        }

        [Fact]
        public void ChangedIsReceivableDerivePurchaseOrderItemShipmentStateNa()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithIsReceivable(false)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(new PurchaseOrderItemShipmentStates(this.Transaction).Na, orderItem.PurchaseOrderItemShipmentState);
        }

        [Fact]
        public void ChangedPurchaseOrderPurchaseOrderStateDerivePurchaseOrderItemState()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction).Build())
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem)
                .Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            order.PurchaseOrderState = new PurchaseOrderStates(this.Transaction).InProcess;
            this.Transaction.Derive(false);

            Assert.True(order.PurchaseOrderState.IsInProcess);
            Assert.True(orderItem.PurchaseOrderItemState.IsInProcess);
        }

        [Fact]
        public void ChangedShipmentReceiptQuantityAcceptedDeriveQuantityReceived()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction).Build())
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem)
                .Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            new ShipmentReceiptBuilder(this.Transaction).WithOrderItem(orderItem).WithQuantityAccepted(1).Build();
            this.Transaction.Derive(false);

            Assert.Equal(1, orderItem.QuantityReceived);
        }

        [Fact]
        public void ChangedShipmentReceiptQuantityAcceptedDerivePurchaseOrderItemShipmentStatePartiallyReceived()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction).Build())
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem)
                .WithQuantityOrdered(2)
                .Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            new ShipmentReceiptBuilder(this.Transaction).WithOrderItem(orderItem).WithQuantityAccepted(1).Build();
            this.Transaction.Derive(false);

            Assert.True(orderItem.PurchaseOrderItemShipmentState.IsPartiallyReceived);
        }

        [Fact]
        public void ChangedShipmentReceiptQuantityAcceptedDerivePurchaseOrderItemShipmentStateReceived()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction).Build())
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem)
                .WithQuantityOrdered(1)
                .Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            new ShipmentReceiptBuilder(this.Transaction).WithOrderItem(orderItem).WithQuantityAccepted(1).Build();
            this.Transaction.Derive(false);

            Assert.True(orderItem.PurchaseOrderItemShipmentState.IsReceived);
        }

        [Fact]
        public void ChangedOrderItemBillingOrderItemDerivePurchaseOrderItemPaymentStateNotPaid()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            new OrderItemBillingBuilder(this.Transaction).WithOrderItem(orderItem).WithInvoiceItem(invoiceItem).Build();
            this.Transaction.Derive(false);

            Assert.True(orderItem.PurchaseOrderItemPaymentState.IsNotPaid);
        }

        [Fact]
        public void ChangedOrderItemBillingOrderItemDerivePurchaseOrderItemPaymentStatePartiallyPaid()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithAssignedUnitPrice(1).WithQuantity(10).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoice.PurchaseInvoiceState = new PurchaseInvoiceStates(this.Transaction).NotPaid;

            new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoiceItem).WithAmountApplied(1).Build();
            this.Transaction.Derive(false);

            new OrderItemBillingBuilder(this.Transaction).WithOrderItem(orderItem).WithInvoiceItem(invoiceItem).Build();
            this.Transaction.Derive(false);

            Assert.True(orderItem.PurchaseOrderItemPaymentState.IsPartiallyPaid);
        }

        [Fact]
        public void ChangedOrderItemBillingOrderItemDerivePurchaseOrderItemPaymentStatePaid()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithAssignedUnitPrice(1).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoice.PurchaseInvoiceState = new PurchaseInvoiceStates(this.Transaction).NotPaid;

            new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoiceItem).WithAmountApplied(1).Build();
            this.Transaction.Derive(false);

            new OrderItemBillingBuilder(this.Transaction).WithOrderItem(orderItem).WithInvoiceItem(invoiceItem).Build();
            this.Transaction.Derive(false);

            Assert.True(orderItem.PurchaseOrderItemPaymentState.IsPaid);
        }

        [Fact]
        public void ChangedPurchaseOrderItemStateDerivePurchaseOrderItemStateCompleted()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPurchaseOrderItemState(new PurchaseOrderItemStates(this.Transaction).InProcess)
                .WithIsReceivable(false)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(new PurchaseOrderItemStates(this.Transaction).Completed, orderItem.PurchaseOrderItemState);
        }

        [Fact]
        public void ChangedPurchaseOrderItemStateDerivePurchaseOrderItemStateFinished()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPurchaseOrderItemState(new PurchaseOrderItemStates(this.Transaction).InProcess)
                .WithPurchaseOrderItemPaymentState(new PurchaseOrderItemPaymentStates(this.Transaction).Paid)
                .WithIsReceivable(false)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(new PurchaseOrderItemStates(this.Transaction).Finished, orderItem.PurchaseOrderItemState);
        }
    }

    [Trait("Category", "Security")]
    public class PurchaseOrderItemSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        private Part finishedGood;
        private SupplierOffering currentPurchasePrice;
        private PurchaseOrder order;
        private Organisation supplier;

        public PurchaseOrderItemSecurityTests(Fixture fixture) : base(fixture)
        {
            var euro = new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR");

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            ContactMechanism takenViaContactMechanism = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();

            var supplierContactMechanism = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(takenViaContactMechanism)
                .WithUseAsDefault(true)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .Build();

            this.supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            this.supplier.AddPartyContactMechanism(supplierContactMechanism);

            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(this.supplier).Build();

            var good1 = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            this.finishedGood = good1.Part;

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithSupplier(this.supplier)
                .WithFromDate(this.Transaction.Now().AddYears(-1))
                .WithThroughDate(this.Transaction.Now().AddDays(-1))
                .WithCurrency(euro)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPrice(8)
                .Build();

            this.currentPurchasePrice = new SupplierOfferingBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithSupplier(this.supplier)
                .WithFromDate(this.Transaction.Now())
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .WithCurrency(euro)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPrice(10)
                .Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithSupplier(this.supplier)
                .WithFromDate(this.Transaction.Now().AddYears(1))
                .WithCurrency(euro)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPrice(8)
                .Build();

            this.order = new PurchaseOrderBuilder(this.Transaction)
                .WithTakenViaSupplier(this.supplier)
                .WithAssignedBillToContactMechanism(takenViaContactMechanism)
                .WithDeliveryDate(this.Transaction.Now())
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).Exempt)
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();
        }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsCreated_ThenItemMayBeDeletedButNotCancelledOrRejected()
        {
            var administrator = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Transaction).Administrators;
            administrators.AddMember(administrator);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            this.Transaction.Derive();
            this.Transaction.Commit();

            Assert.Equal(new PurchaseOrderItemStates(this.Transaction).Created, item.PurchaseOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[item];

            Assert.True(acl.CanExecute(this.M.PurchaseOrderItem.Delete));
            Assert.True(acl.CanExecute(this.M.PurchaseOrderItem.Cancel));
            Assert.True(acl.CanExecute(this.M.PurchaseOrderItem.Reject));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsSetReadyForProcessing_ThenItemMayBeCancelledOrRejected()
        {
            var administrator = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Transaction).Administrators;
            administrators.AddMember(administrator);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            this.order.SetReadyForProcessing();

            this.Transaction.Derive();

            Assert.Equal(new PurchaseOrderItemStates(this.Transaction).InProcess, item.PurchaseOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[item];
            Assert.False(acl.CanExecute(this.M.PurchaseOrderItem.Cancel));
            Assert.False(acl.CanExecute(this.M.PurchaseOrderItem.Reject));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsPartiallyReceived_ThenItemMayNotBeCancelledOrRejectedOrDeleted()
        {
            var administrator = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Transaction).Administrators;
            administrators.AddMember(administrator);
            this.Transaction.Derive();

            this.InstantiateObjects(this.Transaction);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithQuantityOrdered(20)
                .WithAssignedUnitPrice(5)
                .Build();
            this.order.AddPurchaseOrderItem(item);

            this.order.SetReadyForProcessing();
            this.Transaction.Derive();

            var shipment = new PurchaseShipmentBuilder(this.Transaction).WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground).WithShipFromParty(this.order.TakenViaSupplier).Build();
            this.Transaction.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithPart(this.finishedGood).WithQuantity(10).WithUnitPurchasePrice(1).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive();

            new ShipmentReceiptBuilder(this.Transaction)
                .WithQuantityAccepted(3)
                .WithShipmentItem(shipmentItem)
                .WithOrderItem(item)
                .WithInventoryItem(this.finishedGood.InventoryItemsWherePart.First)
                .WithFacility(shipmentItem.StoredInFacility)
                .Build();
            this.Transaction.Derive();

            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[item];

            Assert.Equal(new PurchaseOrderItemShipmentStates(this.Transaction).PartiallyReceived, item.PurchaseOrderItemShipmentState);
            Assert.False(acl.CanExecute(this.M.PurchaseOrderItem.Cancel));
            Assert.False(acl.CanExecute(this.M.PurchaseOrderItem.Reject));
            Assert.False(acl.CanExecute(this.M.PurchaseOrderItem.Delete));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsCancelled_ThenItemMayNotBeCancelledOrRejected()
        {
            var administrator = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Transaction).Administrators;
            administrators.AddMember(administrator);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            this.Transaction.Derive();
            this.Transaction.Commit();

            item.Cancel();

            this.Transaction.Derive();

            Assert.Equal(new PurchaseOrderItemStates(this.Transaction).Cancelled, item.PurchaseOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[item];
            Assert.False(acl.CanExecute(this.M.PurchaseOrderItem.Cancel));
            Assert.False(acl.CanExecute(this.M.PurchaseOrderItem.Reject));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsCancelled_ThenItemCanBeDeleted()
        {
            var administrator = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Transaction).Administrators;
            administrators.AddMember(administrator);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            this.Transaction.Derive();
            this.Transaction.Commit();

            item.Cancel();

            this.Transaction.Derive();

            Assert.Equal(new PurchaseOrderItemStates(this.Transaction).Cancelled, item.PurchaseOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[item];
            Assert.True(acl.CanExecute(this.M.PurchaseOrderItem.Delete));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsRejected_ThenItemMayNotBeCancelledOrRejected()
        {
            var administrator = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Transaction).Administrators;
            administrators.AddMember(administrator);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            this.Transaction.Derive();

            item.Reject();

            this.Transaction.Derive();

            Assert.Equal(new PurchaseOrderItemStates(this.Transaction).Rejected, item.PurchaseOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[item];
            Assert.False(acl.CanExecute(this.M.PurchaseOrderItem.Cancel));
            Assert.False(acl.CanExecute(this.M.PurchaseOrderItem.Reject));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsRejected_ThenItemCanBeDeleted()
        {
            var administrator = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Transaction).Administrators;
            administrators.AddMember(administrator);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            this.Transaction.Derive();

            item.Reject();

            this.Transaction.Derive();

            Assert.Equal(new PurchaseOrderItemStates(this.Transaction).Rejected, item.PurchaseOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[item];
            Assert.True(acl.CanExecute(this.M.PurchaseOrderItem.Delete));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsCompleted_ThenItemMayNotBeCancelledOrRejectedOrDeleted()
        {
            var administrator = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Transaction).Administrators;
            administrators.AddMember(administrator);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            this.order.SetReadyForProcessing();
            this.Transaction.Derive();

            this.order.QuickReceive();
            this.Transaction.Derive();

            var shipment = (PurchaseShipment)item.OrderShipmentsWhereOrderItem.First.ShipmentItem.ShipmentWhereShipmentItem;
            shipment.Receive();
            this.Transaction.Derive();

            Assert.Equal(new PurchaseOrderItemStates(this.Transaction).Completed, item.PurchaseOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[item];
            Assert.False(acl.CanExecute(this.M.PurchaseOrderItem.Cancel));
            Assert.False(acl.CanExecute(this.M.PurchaseOrderItem.Reject));
            Assert.False(acl.CanExecute(this.M.PurchaseOrderItem.Delete));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectShipmentStateIsReceived_ThenReceiveIsNotAllowed()
        {
            var administrator = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Transaction).Administrators;
            administrators.AddMember(administrator);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            this.order.SetReadyForProcessing();
            this.Transaction.Derive();

            this.order.QuickReceive();
            this.Transaction.Derive();

            var shipment = (PurchaseShipment)item.OrderShipmentsWhereOrderItem.First.ShipmentItem.ShipmentWhereShipmentItem;
            shipment.Receive();
            this.Transaction.Derive();

            Assert.True(item.PurchaseOrderItemShipmentState.IsReceived);
            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[item];
            Assert.False(acl.CanExecute(this.M.PurchaseOrderItem.QuickReceive));
            Assert.False(acl.CanExecute(this.M.PurchaseOrderItem.Cancel));
            Assert.False(acl.CanExecute(this.M.PurchaseOrderItem.Reject));
            Assert.False(acl.CanExecute(this.M.PurchaseOrderItem.Delete));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsFinished_ThenItemMayNotBeCancelledOrRejectedOrDeleted()
        {
            var administrator = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Transaction).Administrators;
            administrators.AddMember(administrator);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            this.Transaction.Derive();

            this.order.PurchaseOrderState = new PurchaseOrderStates(this.Transaction).Finished;

            this.Transaction.Derive();

            Assert.Equal(new PurchaseOrderItemStates(this.Transaction).Finished, item.PurchaseOrderItemState);
            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[item];
            Assert.False(acl.CanExecute(this.M.PurchaseOrderItem.Cancel));
            Assert.False(acl.CanExecute(this.M.PurchaseOrderItem.Reject));
            Assert.False(acl.CanExecute(this.M.PurchaseOrderItem.Delete));
        }

        [Fact]
        public void GivenOrderItem_WhenObjectStateIsPartiallyReceived_ThenProductChangeIsNotAllowed()
        {
            var administrator = new PersonBuilder(this.Transaction).WithFirstName("Koen").WithUserName("admin").Build();
            var administrators = new UserGroups(this.Transaction).Administrators;
            administrators.AddMember(administrator);
            this.Transaction.Derive();

            this.InstantiateObjects(this.Transaction);

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithQuantityOrdered(10)
                .WithAssignedUnitPrice(5)
                .Build();
            this.order.AddPurchaseOrderItem(item);

            this.order.SetReadyForProcessing();
            this.Transaction.Derive();

            var shipment = new PurchaseShipmentBuilder(this.Transaction).WithShipmentMethod(new ShipmentMethods(this.Transaction).Ground).WithShipFromParty(this.order.TakenViaSupplier).Build();
            this.Transaction.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithPart(this.finishedGood).WithQuantity(10).WithUnitPurchasePrice(1).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Transaction.Derive();

            new ShipmentReceiptBuilder(this.Transaction)
                .WithQuantityAccepted(3)
                .WithShipmentItem(shipmentItem)
                .WithOrderItem(item)
                .WithFacility(shipmentItem.StoredInFacility)
                .WithInventoryItem(this.finishedGood.InventoryItemsWherePart.First)
                .Build();
            this.Transaction.Derive();

            Assert.Equal(new PurchaseOrderItemShipmentStates(this.Transaction).PartiallyReceived, item.PurchaseOrderItemShipmentState);
            var acl = new DatabaseAccessControlLists(this.Transaction.GetUser())[item];
            Assert.False(acl.CanWrite(this.M.PurchaseOrderItem.Part));
        }

        private void InstantiateObjects(ITransaction transaction)
        {
            this.finishedGood = (Part)transaction.Instantiate(this.finishedGood);
            this.currentPurchasePrice = (SupplierOffering)transaction.Instantiate(this.currentPurchasePrice);
            this.order = (PurchaseOrder)transaction.Instantiate(this.order);
            this.supplier = (Organisation)transaction.Instantiate(this.supplier);
        }
    }

    [Trait("Category", "Security")]
    public class PurchaseOrderItemDeniedPermissonRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderItemDeniedPermissonRuleTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Transaction).Get(this.M.PurchaseOrderItem, this.M.PurchaseOrderItem.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedOrderItemBillingOrderItemDeriveDeletePermission()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItemBilling = new OrderItemBillingBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, orderItem.DeniedPermissions);

            orderItemBilling.OrderItem = orderItem;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, orderItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderItemPurchaseOrderItemStateDeriveDeletePermission()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, orderItem.DeniedPermissions);

            orderItem.PurchaseOrderItemState = new PurchaseOrderItemStates(this.Transaction).InProcess;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, orderItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangedOrderShipmentOrderItemDeriveDeletePermission()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderShipment = new OrderShipmentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, orderItem.DeniedPermissions);

            orderShipment.OrderItem = orderItem;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, orderItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangedOrderRequirementCommitmentOrderItemDeriveDeletePermission()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var requirementCommitment = new OrderRequirementCommitmentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, orderItem.DeniedPermissions);

            requirementCommitment.OrderItem = orderItem;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, orderItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortOrderItemFulfillmentDeriveDeletePermission()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var worktask = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, orderItem.DeniedPermissions);

            worktask.OrderItemFulfillment = orderItem;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, orderItem.DeniedPermissions);
        }
    }
}
