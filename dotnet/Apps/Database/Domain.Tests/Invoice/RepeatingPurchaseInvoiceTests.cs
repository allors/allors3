// <copyright file="RepeatingPurchaseInvoiceTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Linq;
    using Configuration.Derivations.Default;
    using Database.Derivations;
    using Meta;
    using Resources;
    using Xunit;
    using DateTime = System.DateTime;

    public class RepeatingPurchaseInvoiceTests : DomainTest, IClassFixture<Fixture>
    {
        public RepeatingPurchaseInvoiceTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedFrequencyThrowValidationError()
        {
            var repeatingInvoice = new RepeatingPurchaseInvoiceBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Hour)
                .Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.FrequencyNotSupported));
        }

        [Fact]
        public void ChangedDayOfWeekThrowValidationErrorAssertExistsDayOfWeek()
        {
            var repeatingInvoice = new RepeatingPurchaseInvoiceBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Week)
                .WithDayOfWeek(new DaysOfWeek(this.Transaction).Monday)
                .Build();
            this.Derive();

            repeatingInvoice.RemoveDayOfWeek();

            var errors = this.Derive().Errors.OfType<IDerivationErrorRequired>();
            Assert.Contains(this.M.RepeatingPurchaseInvoice.DayOfWeek, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedDayOfWeekThrowValidationErrorAssertnotExistsDayOfWeek()
        {
            var repeatingInvoice = new RepeatingPurchaseInvoiceBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Month)
                .Build();
            this.Derive();

            repeatingInvoice.DayOfWeek = new DaysOfWeek(this.Transaction).Monday;

            var errors = this.Derive().Errors.OfType<DerivationErrorNotAllowed>();
            Assert.Equal(new IRoleType[]
            {
                this.M.RepeatingPurchaseInvoice.DayOfWeek,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedNextExecutionDateThrowValidationError()
        {
            var repeatingInvoice = new RepeatingPurchaseInvoiceBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Week)
                .WithDayOfWeek(new DaysOfWeek(this.Transaction).Monday)
                .Build();
            this.Derive();

            repeatingInvoice.NextExecutionDate = new DateTime(2021, 01, 06, 12, 0, 0, DateTimeKind.Utc);

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.DateDayOfWeek));
        }

        [Fact]
        public void GivenAlreadyBilledPartiallyReceivedOrderItem_WhenRepeating_ThenItemIsNotRebilled()
        {
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).WithInternalOrganisation(this.InternalOrganisation).Build();

            var part = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1").Part;

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPrice(5)
                .WithCurrency(new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR"))
                .Build();

            var order = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).Build();

            var item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem)
                .WithPart(part)
                .WithQuantityOrdered(2)
                .WithAssignedUnitPrice(5)
                .Build();
            order.AddPurchaseOrderItem(item);
            this.Derive();

            order.SetReadyForProcessing();
            this.Derive();

            order.Send();
            this.Derive();

            // Partially receive the item (1 of 2 ordered).
            new ShipmentReceiptBuilder(this.Transaction).WithOrderItem(item).WithQuantityAccepted(1).Build();
            this.Derive();

            // The item is already billed.
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction).Build();
            purchaseInvoice.AddPurchaseInvoiceItem(invoiceItem);
            new OrderItemBillingBuilder(this.Transaction).WithOrderItem(item).WithInvoiceItem(invoiceItem).WithQuantity(1).WithAmount(5).Build();
            this.Derive();

            // Scenario preconditions for RepeatingPurchaseInvoice.Repeat to consider this order/item.
            Assert.True(order.PurchaseOrderState.IsSent || order.PurchaseOrderState.IsCompleted, $"orderState={order.PurchaseOrderState?.Name}");
            Assert.True(item.PurchaseOrderItemShipmentState.IsPartiallyReceived, $"itemShipmentState={item.PurchaseOrderItemShipmentState?.Name}");

            var repeatingInvoice = new RepeatingPurchaseInvoiceBuilder(this.Transaction)
                .WithSupplier(supplier)
                .WithInternalOrganisation(this.InternalOrganisation)
                .WithFrequency(new TimeFrequencies(this.Transaction).Month)
                .Build();
            this.Derive();

            repeatingInvoice.Repeat();
            this.Derive();

            // The already-billed item must not be billed a second time.
            Assert.Single(item.OrderItemBillingsWhereOrderItem);
        }
    }
}
