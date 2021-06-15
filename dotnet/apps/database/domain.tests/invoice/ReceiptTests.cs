// <copyright file="ReceiptTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Resources;
    using Xunit;

    public class ReceiptTests : DomainTest, IClassFixture<Fixture>
    {
        private Singleton singleton;
        private Part finishedGood;
        private Good good;
        private Organisation billToCustomer;

        public ReceiptTests(Fixture fixture) : base(fixture)
        {
            var euro = new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR");

            this.singleton = this.Transaction.GetSingleton();
            this.billToCustomer = new OrganisationBuilder(this.Transaction).WithName("billToCustomer").WithPreferredCurrency(euro).Build();
            this.good = new Goods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").WithLocale(new Locales(this.Transaction).EnglishGreatBritain).Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(this.billToCustomer).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithSupplier(supplier)
                .WithFromDate(this.Transaction.Now())
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithCurrency(euro)
                .WithPrice(7)
                .Build();

            new BasePriceBuilder(this.Transaction)
                .WithDescription("current good")
                .WithProduct(this.good)
                .WithPrice(10)
                .WithFromDate(this.Transaction.Now())
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();
        }

        [Fact]
        public void GivenReceipt_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            this.InstantiateObjects(this.Transaction);

            var receipt = new ReceiptBuilder(this.Transaction).WithEffectiveDate(this.Transaction.Now()).Build();

            Assert.True(receipt.ExistUniqueId);
        }

        [Fact]
        public void GivenReceipt_WhenApplied_ThenInvoiceItemAmountPaidIsUpdated()
        {
            this.InstantiateObjects(this.Transaction);

            var productItem = new InvoiceItemTypes(this.Transaction).ProductItem;
            var contactMechanism = new ContactMechanisms(this.Transaction).Extent().First;

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithBillToCustomer(this.billToCustomer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .Build();

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithQuantity(1).WithAssignedUnitPrice(100M).WithInvoiceItemType(productItem).Build();
            var item2 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithQuantity(1).WithAssignedUnitPrice(200M).WithInvoiceItemType(productItem).Build();
            var item3 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithQuantity(1).WithAssignedUnitPrice(300M).WithInvoiceItemType(productItem).Build();

            invoice.AddSalesInvoiceItem(item1);
            invoice.AddSalesInvoiceItem(item2);
            invoice.AddSalesInvoiceItem(item3);

            this.Transaction.Derive();

            new ReceiptBuilder(this.Transaction)
                .WithAmount(50)
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(item2).WithAmountApplied(50).Build())
                .WithEffectiveDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(0, item1.AmountPaid);
            Assert.Equal(50, item2.AmountPaid);
            Assert.Equal(0, item3.AmountPaid);

            new ReceiptBuilder(this.Transaction)
                .WithAmount(350)
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(item1).WithAmountApplied(100).Build())
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(item2).WithAmountApplied(150).Build())
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(item3).WithAmountApplied(100).Build())
                .WithEffectiveDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(100, item1.AmountPaid);
            Assert.Equal(200, item2.AmountPaid);
            Assert.Equal(100, item3.AmountPaid);
        }

        [Fact]
        public void GivenReceipt_WhenDeriving_ThenAmountCanNotBeSmallerThenAmountApplied()
        {
            this.InstantiateObjects(this.Transaction);

            var billToContactMechanism = new EmailAddressBuilder(this.Transaction).WithElectronicAddressString("info@allors.com").Build();

            var customer = new PersonBuilder(this.Transaction)
                .WithLastName("customer")

                .Build();

            new CustomerRelationshipBuilder(this.Transaction)
                .WithCustomer(customer)
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(billToContactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .WithSalesInvoiceItem(new SalesInvoiceItemBuilder(this.Transaction)
                                        .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                                        .WithProduct(this.good)
                                        .WithQuantity(1)
                                        .WithAssignedUnitPrice(100M)
                                        .Build())
                .Build();

            this.Transaction.Derive();

            var receipt = new ReceiptBuilder(this.Transaction)
                .WithAmount(100)
                .WithEffectiveDate(this.Transaction.Now())
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoice.SalesInvoiceItems[0]).WithAmountApplied(50).Build())
                .Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            receipt.AddPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoice.SalesInvoiceItems[0]).WithAmountApplied(50).Build());

            Assert.False(this.Transaction.Derive(false).HasErrors);

            receipt.AddPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoice.SalesInvoiceItems[0]).WithAmountApplied(1).Build());

            var errors = this.Transaction.Derive(false).Errors.ToList();

            Assert.Single(errors.FindAll(e => e.Message.Contains(ErrorMessages.PaymentAmountIsToSmall)));
        }

        private void InstantiateObjects(ITransaction transaction)
        {
            this.finishedGood = (Part)transaction.Instantiate(this.finishedGood);
            this.good = (Good)transaction.Instantiate(this.good);
            this.singleton = (Singleton)transaction.Instantiate(this.singleton);
            this.billToCustomer = (Organisation)transaction.Instantiate(this.billToCustomer);
        }
    }
}
