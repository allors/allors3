// <copyright file="PurchaseOrderItemTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Resources;
    using TestPopulation;
    using Xunit;
    using ContactMechanism = Domain.ContactMechanism;
    using Organisation = Domain.Organisation;
    using Part = Domain.Part;
    using PurchaseOrder = Domain.PurchaseOrder;
    using PurchaseShipment = Domain.PurchaseShipment;
    using Revocation = Domain.Revocation;
    using SupplierOffering = Domain.SupplierOffering;
    using User = Domain.User;

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

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InvalidQuantity));

            this.Transaction.Rollback();

            item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithDescription("Do Something")
                .WithQuantityOrdered(2)
                .WithAssignedUnitPrice(1)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InvalidQuantity));

            this.Transaction.Rollback();

            item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithDescription("Do Something")
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Service)
                .WithQuantityOrdered(1)
                .WithAssignedUnitPrice(1)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            Assert.False(this.Derive().HasErrors);
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

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InvalidQuantity));

            this.Transaction.Rollback();

            item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(serialisedPart)
                .WithSerialNumber("1")
                .WithQuantityOrdered(2)
                .WithAssignedUnitPrice(1)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InvalidQuantity));

            this.Transaction.Rollback();

            item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(serialisedPart)
                .WithAssignedUnitPrice(1)
                .WithQuantityOrdered(1)
                .WithSerialNumber("1")
                .Build();

            this.order.AddPurchaseOrderItem(item);

            Assert.False(this.Derive().HasErrors);
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

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InvalidQuantity));

            this.Transaction.Rollback();

            item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(nonSerialisedPart)
                .WithQuantityOrdered(2)
                .WithAssignedUnitPrice(1)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            Assert.False(this.Derive().HasErrors);

            this.Transaction.Rollback();

            item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(nonSerialisedPart)
                .WithAssignedUnitPrice(1)
                .WithQuantityOrdered(2)
                .Build();

            this.order.AddPurchaseOrderItem(item);

            Assert.False(this.Derive().HasErrors);
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
            this.Derive();

            Assert.True(orderItem.ExistPurchaseOrderItemState);
        }

        [Fact]
        public void DerivePurchaseOrderItemPaymentState()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            this.Derive();

            Assert.True(orderItem.ExistPurchaseOrderItemPaymentState);
        }

        [Fact]
        public void DeriveInvoiceItemType()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(new UnifiedGoodBuilder(this.Transaction).Build())
                .Build();

            this.Derive();

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
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithPurchaseOrderItemState(new PurchaseOrderItemStates(this.Transaction).Finished).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            orderItem.PurchaseOrderItemState = new PurchaseOrderItemStates(this.Transaction).Created;
            this.Derive();

            Assert.Equal(orderItem.DerivedDeliveryDate, order.DeliveryDate);
        }

        [Fact]
        public void ChangedPurchaseOrderPurchaseOrderItemsDeriveDerivedDeliveryDate()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).WithDeliveryDate(this.Transaction.Now().Date).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            Assert.Equal(orderItem.DerivedDeliveryDate, order.DeliveryDate);
        }

        [Fact]
        public void ChangedAssignedDeliveryDateDeriveDerivedDeliveryDate()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithAssignedDeliveryDate(this.Transaction.Now().Date).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            Assert.Equal(orderItem.DerivedDeliveryDate, orderItem.AssignedDeliveryDate);
        }

        [Fact]
        public void ChangedsalesOrderDerivedDeliveryDateDeriveDerivedDeliveryDate()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).WithDeliveryDate(this.Transaction.Now().Date).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            order.DeliveryDate = this.Transaction.Now().Date;
            this.Derive();

            Assert.Equal(orderItem.DerivedDeliveryDate, order.DeliveryDate);
        }

        [Fact]
        public void ChangedAssignedVatRegimeDeriveDerivedVatRegime()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithAssignedVatRegime(new VatRegimes(this.Transaction).BelgiumStandard).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            Assert.Equal(orderItem.DerivedVatRegime, orderItem.AssignedVatRegime);
        }

        [Fact]
        public void ChangedsalesOrderDerivedVatRegimeDeriveDerivedVatRegime()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            order.AssignedVatRegime = new VatRegimes(this.Transaction).BelgiumStandard;
            this.Derive();

            Assert.Equal(orderItem.DerivedVatRegime, order.AssignedVatRegime);
        }

        [Fact]
        public void ChangedDerivedVatRegimeDeriveVatRate()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            order.AssignedVatRegime = new VatRegimes(this.Transaction).BelgiumStandard;
            this.Derive();

            Assert.Equal(orderItem.VatRate, order.AssignedVatRegime.VatRates.ElementAt(0));
        }

        [Fact]
        public void ChangedAssignedIrpfRegimeDeriveDerivedIrpfRegime()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithAssignedIrpfRegime(new IrpfRegimes(this.Transaction).Assessable15).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            Assert.Equal(orderItem.DerivedIrpfRegime, orderItem.AssignedIrpfRegime);
        }

        [Fact]
        public void ChangedsalesOrderDerivedIrpfRegimeDeriveDerivedIrpfRegime()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            order.AssignedIrpfRegime = new IrpfRegimes(this.Transaction).Assessable15;
            this.Derive();

            Assert.Equal(orderItem.DerivedIrpfRegime, order.AssignedIrpfRegime);
        }

        [Fact]
        public void ChangedDerivedIrpfRegimeDeriveIrpfRate()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            order.AssignedIrpfRegime = new IrpfRegimes(this.Transaction).Assessable15;
            this.Derive();

            Assert.Equal(orderItem.IrpfRate, order.AssignedIrpfRegime.IrpfRates.ElementAt(0));
        }

        [Fact]
        public void ChangedPurchaseOrderOrderDateDeriveVatRate()
        {
            var vatRegime = new VatRegimes(this.Transaction).SpainReduced;
            vatRegime.VatRates.ElementAt(0).ThroughDate = this.Transaction.Now().AddDays(-1).Date;
            this.Derive();

            var newVatRate = new VatRateBuilder(this.Transaction).WithFromDate(this.Transaction.Now().Date).WithRate(11).Build();
            vatRegime.AddVatRate(newVatRate);
            this.Derive();

            var order = new PurchaseOrderBuilder(this.Transaction)
                .WithOrderDate(this.Transaction.Now().AddDays(-1).Date)
                .WithAssignedVatRegime(vatRegime).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            Assert.NotEqual(newVatRate, orderItem.VatRate);

            order.OrderDate = this.Transaction.Now().AddDays(1).Date;
            this.Derive();

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
            this.Derive();

            orderItem.Part = serialisedPart;

            var errors = this.Derive().Errors.OfType<IDerivationErrorAtLeastOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.PurchaseOrderItem.SerialisedItem,
                this.M.PurchaseOrderItem.SerialNumber,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedSerialisedItemThrowValidationError()
        {
            var serialisedPart = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(serialisedPart)
                .WithSerialNumber("number")
                .Build();
            this.Derive();

            orderItem.SerialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.OfType<IDerivationErrorAtMostOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.PurchaseOrderItem.SerialisedItem,
                this.M.PurchaseOrderItem.SerialNumber,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedSerialNumberThrowValidationError()
        {
            var serialisedPart = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(serialisedPart)
                .WithSerialisedItem(new SerialisedItemBuilder(this.Transaction).Build())
                .Build();
            this.Derive();

            orderItem.SerialNumber = "number";

            var errors = this.Derive().Errors.OfType<IDerivationErrorAtMostOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.PurchaseOrderItem.SerialisedItem,
                this.M.PurchaseOrderItem.SerialNumber,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedQuantityOrderedThrowValidationError_1()
        {
            var serialisedPart = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(serialisedPart)
                .WithSerialisedItem(new SerialisedItemBuilder(this.Transaction).Build())
                .Build();
            this.Derive();

            orderItem.QuantityOrdered = 2;

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InvalidQuantity));
        }

        [Fact]
        public void ChangedQuantityOrderedThrowValidationError_2()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithSerialisedItem(new SerialisedItemBuilder(this.Transaction).Build())
                .Build();
            this.Derive();

            orderItem.QuantityOrdered = 2;

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InvalidQuantity));
        }

        [Fact]
        public void ChangedQuantityOrderedThrowValidationError_3()
        {
            var nonSerialisedPart = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(nonSerialisedPart)
                .WithQuantityOrdered(1)
                .Build();
            this.Derive();

            orderItem.QuantityOrdered = 0;

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InvalidQuantity));
        }

        [Fact]
        public void ChangedOrderItemBillingOrderItemDeriveCanInvoice_1()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            this.Derive();

            new OrderItemBillingBuilder(this.Transaction).WithOrderItem(orderItem).Build();
            this.Derive();

            Assert.False(orderItem.CanInvoice);
        }

        [Fact]
        public void ChangedOrderItemBillingOrderItemDeriveCanInvoice_2()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            this.Derive();

            var billing = new OrderItemBillingBuilder(this.Transaction).WithOrderItem(orderItem).Build();
            this.Derive();

            Assert.False(orderItem.CanInvoice);

            billing.Delete();
            this.Derive();

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
            this.Derive();

            orderItem.InvoiceItemType = new InvoiceItemTypes(this.Transaction).PartItem;
            this.Derive();

            Assert.True(orderItem.IsReceivable);
        }

        [Fact]
        public void ChangedPartDeriveIsReceivable()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem)
                .Build();
            this.Derive();

            orderItem.Part = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

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
            this.Derive();

            Assert.Equal(new PurchaseOrderItemShipmentStates(this.Transaction).NotReceived, orderItem.PurchaseOrderItemShipmentState);
        }

        [Fact]
        public void ChangedIsReceivableDerivePurchaseOrderItemShipmentStateNa()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithIsReceivable(false)
                .Build();
            this.Derive();

            Assert.Equal(new PurchaseOrderItemShipmentStates(this.Transaction).Na, orderItem.PurchaseOrderItemShipmentState);
        }

        [Fact]
        public void ChangedPurchaseOrderPurchaseOrderStateDerivePurchaseOrderItemStateInProcess()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction).Build())
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem)
                .Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            order.PurchaseOrderState = new PurchaseOrderStates(this.Transaction).InProcess;
            this.Derive();

            Assert.True(order.PurchaseOrderState.IsInProcess);
            Assert.True(orderItem.PurchaseOrderItemState.IsInProcess);
        }

        [Fact]
        public void ChangedShipmentReceiptQuantityAcceptedDeriveQuantityReceived()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction).Build())
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem)
                .Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            new ShipmentReceiptBuilder(this.Transaction).WithOrderItem(orderItem).WithQuantityAccepted(1).Build();
            this.Derive();

            Assert.Equal(1, orderItem.QuantityReceived);
        }

        [Fact]
        public void ChangedShipmentReceiptQuantityAcceptedDerivePurchaseOrderItemShipmentStatePartiallyReceived()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction).Build())
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem)
                .WithQuantityOrdered(2)
                .Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            new ShipmentReceiptBuilder(this.Transaction).WithOrderItem(orderItem).WithQuantityAccepted(1).Build();
            this.Derive();

            Assert.True(orderItem.PurchaseOrderItemShipmentState.IsPartiallyReceived);
        }

        [Fact]
        public void ChangedShipmentReceiptQuantityAcceptedDerivePurchaseOrderItemShipmentStateReceived()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction).Build())
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem)
                .WithQuantityOrdered(1)
                .Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            new ShipmentReceiptBuilder(this.Transaction).WithOrderItem(orderItem).WithQuantityAccepted(1).Build();
            this.Derive();

            Assert.True(orderItem.PurchaseOrderItemShipmentState.IsReceived);
        }

        [Fact]
        public void ChangedOrderItemBillingOrderItemDerivePurchaseOrderItemPaymentStateNotPaid()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            new OrderItemBillingBuilder(this.Transaction).WithOrderItem(orderItem).WithInvoiceItem(invoiceItem).Build();
            this.Derive();

            Assert.True(orderItem.PurchaseOrderItemPaymentState.IsNotPaid);
        }

        [Fact]
        public void ChangedOrderItemBillingOrderItemDerivePurchaseOrderItemPaymentStatePartiallyPaid()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithAssignedUnitPrice(1).WithQuantity(10).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            invoice.PurchaseInvoiceState = new PurchaseInvoiceStates(this.Transaction).NotPaid;

            new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoiceItem).WithAmountApplied(1).Build();
            this.Derive();

            new OrderItemBillingBuilder(this.Transaction).WithOrderItem(orderItem).WithInvoiceItem(invoiceItem).Build();
            this.Derive();

            Assert.True(orderItem.PurchaseOrderItemPaymentState.IsPartiallyPaid);
        }

        [Fact]
        public void ChangedOrderItemBillingOrderItemDerivePurchaseOrderItemPaymentStatePaid()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithAssignedUnitPrice(1).WithQuantity(1).Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Derive();

            invoice.PurchaseInvoiceState = new PurchaseInvoiceStates(this.Transaction).NotPaid;

            new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoiceItem).WithAmountApplied(1).Build();
            this.Derive();

            new OrderItemBillingBuilder(this.Transaction).WithOrderItem(orderItem).WithInvoiceItem(invoiceItem).Build();
            this.Derive();

            Assert.True(orderItem.PurchaseOrderItemPaymentState.IsPaid);
        }

        [Fact]
        public void ChangedPurchaseOrderItemStateDerivePurchaseOrderItemStateCompleted()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPurchaseOrderItemState(new PurchaseOrderItemStates(this.Transaction).InProcess)
                .WithIsReceivable(false)
                .Build();
            this.Derive();

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
            this.Derive();

            Assert.Equal(new PurchaseOrderItemStates(this.Transaction).Finished, orderItem.PurchaseOrderItemState);
        }
    }

    public class PurchaseOrderItemQuantityReturnedRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderItemQuantityReturnedRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedOrderShipmentQuantityDeriveQuantityReturned()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(this.InternalOrganisation.ActiveSuppliers.First()).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithIsReceivable(true)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction).Build())
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem)
                .WithQuantityOrdered(2)
                .Build();

            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            new ShipmentReceiptBuilder(this.Transaction).WithOrderItem(orderItem).WithQuantityAccepted(2).Build();
            this.Derive();

            orderItem.Return();
            this.Derive();

            Assert.Equal(2, orderItem.QuantityReturned);
        }

        [Fact]
        public void ChangedMultipleShipmentsOrderShipmentQuantityDeriveQuantityReturned()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(this.InternalOrganisation.ActiveSuppliers.First()).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithIsReceivable(true)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction).Build())
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem)
                .WithQuantityOrdered(2)
                .Build();

            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            new ShipmentReceiptBuilder(this.Transaction).WithOrderItem(orderItem).WithQuantityAccepted(2).Build();
            this.Derive();

            var shipment1 = new PurchaseReturnBuilder(this.Transaction).Build();
            var shipmentItem1 = new ShipmentItemBuilder(this.Transaction).WithQuantity(1).Build();
            shipment1.AddShipmentItem(shipmentItem1);

            var shipment2 = new PurchaseReturnBuilder(this.Transaction).Build();
            var shipmentItem2 = new ShipmentItemBuilder(this.Transaction).WithQuantity(1).Build();
            shipment2.AddShipmentItem(shipmentItem2);

            new OrderShipmentBuilder(this.Transaction)
                .WithOrderItem(orderItem)
                .WithShipmentItem(shipmentItem1)
                .WithQuantity(1)
                .Build();

            new OrderShipmentBuilder(this.Transaction)
                .WithOrderItem(orderItem)
                .WithShipmentItem(shipmentItem2)
                .WithQuantity(1)
                .Build();

            this.Derive();

            Assert.Equal(2, orderItem.QuantityReturned);
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
            var acl = new DatabaseAccessControl(this.Transaction.GetUser())[item];

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
            var acl = new DatabaseAccessControl(this.Transaction.GetUser())[item];
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
                .WithInventoryItem(this.finishedGood.InventoryItemsWherePart.FirstOrDefault())
                .WithFacility(shipmentItem.StoredInFacility)
                .Build();
            this.Transaction.Derive();

            var acl = new DatabaseAccessControl(this.Transaction.GetUser())[item];

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
            var acl = new DatabaseAccessControl(this.Transaction.GetUser())[item];
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
            var acl = new DatabaseAccessControl(this.Transaction.GetUser())[item];
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
            var acl = new DatabaseAccessControl(this.Transaction.GetUser())[item];
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
            var acl = new DatabaseAccessControl(this.Transaction.GetUser())[item];
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

            var shipment = (PurchaseShipment)item.OrderShipmentsWhereOrderItem.First().ShipmentItem.ShipmentWhereShipmentItem;
            shipment.Receive();
            this.Transaction.Derive();

            Assert.Equal(new PurchaseOrderItemStates(this.Transaction).Completed, item.PurchaseOrderItemState);
            var acl = new DatabaseAccessControl(this.Transaction.GetUser())[item];
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

            var shipment = (PurchaseShipment)item.OrderShipmentsWhereOrderItem.First().ShipmentItem.ShipmentWhereShipmentItem;
            shipment.Receive();
            this.Transaction.Derive();

            Assert.True(item.PurchaseOrderItemShipmentState.IsReceived);
            var acl = new DatabaseAccessControl(this.Transaction.GetUser())[item];
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
            var acl = new DatabaseAccessControl(this.Transaction.GetUser())[item];
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
                .WithInventoryItem(this.finishedGood.InventoryItemsWherePart.FirstOrDefault())
                .Build();
            this.Transaction.Derive();

            Assert.Equal(new PurchaseOrderItemShipmentStates(this.Transaction).PartiallyReceived, item.PurchaseOrderItemShipmentState);
            var acl = new DatabaseAccessControl(this.Transaction.GetUser())[item];
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
        public PurchaseOrderItemDeniedPermissonRuleTests(Fixture fixture) : base(fixture)
        {
            this.deleteRevocation = new Revocations(this.Transaction).PurchaseOrderItemDeleteRevocation;
            this.returnRevocation = new Revocations(this.Transaction).PurchaseOrderItemReturnRevocation;
        }

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Revocation deleteRevocation;

        private readonly Revocation returnRevocation;

        [Fact]
        public void OnChangedOrderItemBillingOrderItemDeriveDeletePermission()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            this.Derive();

            var orderItemBilling = new OrderItemBillingBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, orderItem.Revocations);

            orderItemBilling.OrderItem = orderItem;
            this.Derive();

            Assert.Contains(this.deleteRevocation, orderItem.Revocations);
        }

        [Fact]
        public void OnChangedPurchaseOrderItemPurchaseOrderItemStateDeriveDeletePermission()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, orderItem.Revocations);

            orderItem.PurchaseOrderItemState = new PurchaseOrderItemStates(this.Transaction).InProcess;
            this.Derive();

            Assert.Contains(this.deleteRevocation, orderItem.Revocations);
        }

        [Fact]
        public void OnChangedOrderShipmentOrderItemDeriveDeletePermission()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            this.Derive();

            var orderShipment = new OrderShipmentBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, orderItem.Revocations);

            orderShipment.OrderItem = orderItem;
            this.Derive();

            Assert.Contains(this.deleteRevocation, orderItem.Revocations);
        }

        [Fact]
        public void OnChangedOrderRequirementCommitmentOrderItemDeriveDeletePermission()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            this.Derive();

            var requirementCommitment = new OrderRequirementCommitmentBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, orderItem.Revocations);

            requirementCommitment.OrderItem = orderItem;
            this.Derive();

            Assert.Contains(this.deleteRevocation, orderItem.Revocations);
        }

        [Fact]
        public void OnChangedWorkEffortOrderItemFulfillmentDeriveDeletePermission()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            this.Derive();

            var worktask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, orderItem.Revocations);

            worktask.OrderItemFulfillment = orderItem;
            this.Derive();

            Assert.Contains(this.deleteRevocation, orderItem.Revocations);
        }

        [Fact]
        public void OnChangedQuantityReturnedDeriveReturnPermission()
        {
            this.InternalOrganisation.IsAutomaticallyReceived = true;

            var order = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(this.InternalOrganisation.ActiveSuppliers.First()).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction).Build())
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem)
                .WithQuantityOrdered(2)
                .Build();

            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            order.SetReadyForProcessing();
            this.Transaction.Derive();

            order.QuickReceive();
            this.Transaction.Derive();

            Assert.True(orderItem.PurchaseOrderItemState.IsCompleted);
            Assert.DoesNotContain(this.returnRevocation, orderItem.Revocations);

            orderItem.Return();
            this.Derive();

            Assert.Contains(this.returnRevocation, orderItem.Revocations);
        }
    }
}
