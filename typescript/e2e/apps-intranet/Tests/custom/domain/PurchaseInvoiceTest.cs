// <copyright file="PersonEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.E2E.Objects
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.E2E.Angular;
    using Allors.E2E.Angular.Html;
    using Allors.E2E.Angular.Material.Factory;
    using Allors.E2E.Test;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class PurchaseInvoiceTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task CreatePurchaseInvoiceMinimal()
        {
            var before = new PurchaseInvoices(this.Transaction).Extent().ToArray();

            var purchaseInvoiceType = new PurchaseInvoiceTypes(this.Transaction).PurchaseInvoice;
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(v => v.Name == "Allors BV");
            var billedFrom = internalOrganisation.ActiveSuppliers.First();
            var invoiceDate = DateTimeFactory.CreateDate(System.DateTime.Now);
            var actualAmount = 50m;

            var @class = this.M.PurchaseInvoice;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new PurchaseinvoiceCreateFormComponent(this.OverlayContainer);

            await form.PurchaseInvoiceTypeSelect.SelectAsync(purchaseInvoiceType);
            await form.BilledFromAutocomplete.SelectAsync(billedFrom.Name);
            await form.InvoiceDateDatepicker.SetAsync(invoiceDate);
            await form.ActualInvoiceAmountInput.SetAsync(actualAmount);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new PurchaseInvoices(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var purchaseInvoice = after.Except(before).First();

            Assert.AreEqual(purchaseInvoiceType, purchaseInvoice.PurchaseInvoiceType);
            Assert.AreEqual(billedFrom, purchaseInvoice.BilledFrom);
            Assert.AreEqual(invoiceDate, purchaseInvoice.InvoiceDate);
            Assert.AreEqual(actualAmount, purchaseInvoice.ActualInvoiceAmount);
        }

        [Test]
        public async Task CreatePurchaseInvoiceMaximal()
        {
            var before = new PurchaseInvoices(this.Transaction).Extent().ToArray();

            var purchaseInvoiceType = new PurchaseInvoiceTypes(this.Transaction).PurchaseInvoice;
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(v => v.Name == "Allors BV");
            var billedFrom = internalOrganisation.ActiveSuppliers.First();
            var billedFromContactMechanism = billedFrom.CurrentPartyContactMechanisms.First().ContactMechanism;
            var billedFromContactPerson = billedFrom.CurrentContacts.First();
            var billToContactPerson = internalOrganisation.CurrentContacts.First();
            var shipToCustomer = internalOrganisation.ActiveCustomers.First();
            var shipToContactPerson = shipToCustomer.CurrentContacts.First();
            var billToEndCustomer = shipToCustomer;
            var biledToContactPerson = shipToContactPerson;
            var shipingContactPerson = biledToContactPerson;
            var vatRegime = new VatRegimes(this.Transaction).BelgiumStandard;
            var invoiceDate = DateTimeFactory.CreateDate(System.DateTime.Now);
            var dueDate = DateTimeFactory.CreateDate(System.DateTime.Now.AddDays(7));
            var customerReference = "CustomerReference";
            var actualAmount = 50m;

            var @class = this.M.PurchaseInvoice;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new PurchaseinvoiceCreateFormComponent(this.OverlayContainer);

            await form.PurchaseInvoiceTypeSelect.SelectAsync(purchaseInvoiceType);
            await form.BilledFromAutocomplete.SelectAsync(billedFrom.Name);
            await form.DerivedBilledFromContactMechanismSelect.SelectAsync(billedFromContactMechanism);
            await form.BilledFromContactPersonSelect.SelectAsync(billedFromContactPerson);
            await form.BilledToContactPersonAutocomplete.SelectAsync(billToContactPerson.DisplayName);
            await form.ShipToCustomerAutocomplete.SelectAsync(shipToCustomer.DisplayName);
            await form.ShipToCustomerContactPersonSelect.SelectAsync(shipToContactPerson);
            await form.BillToEndCustomerAutocomplete.SelectAsync(billToEndCustomer.DisplayName);
            await form.BillToEndCustomerContactPersonSelect.SelectAsync(biledToContactPerson);
            await form.ShipToEndCustomerContactPersonSelect.SelectAsync(shipingContactPerson);
            await form.DerivedVatRegimeSelect.SelectAsync(vatRegime);
            await form.InvoiceDateDatepicker.SetAsync(invoiceDate);
            await form.DueDateDatepicker.SetAsync(dueDate);
            await form.CustomerReferenceInput.SetValueAsync(customerReference);
            await form.ActualInvoiceAmountInput.SetAsync(actualAmount);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new PurchaseInvoices(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var purchaseInvoice = after.Except(before).First();

            Assert.AreEqual(purchaseInvoiceType, purchaseInvoice.PurchaseInvoiceType);
            Assert.AreEqual(billedFrom, purchaseInvoice.BilledFrom);
            Assert.AreEqual(billedFromContactMechanism, purchaseInvoice.AssignedBilledFromContactMechanism);
            Assert.AreEqual(billedFromContactPerson, purchaseInvoice.BilledFromContactPerson);
            Assert.AreEqual(billToContactPerson, purchaseInvoice.BilledToContactPerson);
            Assert.AreEqual(shipToCustomer, purchaseInvoice.ShipToCustomer);
            Assert.AreEqual(shipToContactPerson, purchaseInvoice.ShipToCustomerContactPerson);
            Assert.AreEqual(billToEndCustomer, purchaseInvoice.BillToEndCustomer);
            Assert.AreEqual(biledToContactPerson, purchaseInvoice.BillToEndCustomerContactPerson);
            Assert.AreEqual(shipingContactPerson, purchaseInvoice.ShipToEndCustomerContactPerson);
            Assert.AreEqual(vatRegime, purchaseInvoice.AssignedVatRegime);
            Assert.AreEqual(invoiceDate, purchaseInvoice.InvoiceDate);
            Assert.AreEqual(dueDate, purchaseInvoice.DueDate);
            Assert.AreEqual(customerReference, purchaseInvoice.CustomerReference);
            Assert.AreEqual(actualAmount, purchaseInvoice.ActualInvoiceAmount);
        }
    }
}
