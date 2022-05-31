// <copyright file="PersonEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.E2E.Objects
{
    using System;
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.E2E.Angular;
    using Allors.E2E.Angular.Html;
    using Allors.E2E.Angular.Material.Factory;
    using Allors.E2E.Test;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class SalesInvoiceTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task CreateMinimal()
        {
            var before = new SalesInvoices(this.Transaction).Extent().ToArray();
            var salesInvoiceType = new SalesInvoiceTypes(this.Transaction).SalesInvoice;
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(v => v.Name == "Allors BV");
            var billTo = internalOrganisation.ActiveCustomers.OfType<Organisation>().First(v => v.CurrentContacts.Any());

            var @class = this.M.SalesInvoice;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new SalesinvoiceCreateFormComponent(this.OverlayContainer);

            await form.SalesInvoiceTypeSelect.SelectAsync(salesInvoiceType);
            await form.BillToCustomerAutocomplete.SelectAsync(billTo.Name);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new SalesInvoices(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var salesInvoice = after.Except(before).First();

            Assert.AreEqual(salesInvoiceType, salesInvoice.SalesInvoiceType);
            Assert.AreEqual(billTo, salesInvoice.BillToCustomer);
        }

        [Test]
        public async Task CreateMaximum()
        {
            var before = new SalesInvoices(this.Transaction).Extent().ToArray();
            var salesInvoiceType = new SalesInvoiceTypes(this.Transaction).SalesInvoice;
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(v => v.Name == "Allors BV");
            var billTo = internalOrganisation.ActiveCustomers.OfType<Organisation>().First(v => v.CurrentContacts.Any());
            var billToContactPerson = billTo.CurrentContacts.First();
            var billToShipingContactPerson = billToContactPerson;
            var shipToEndCustomer = billTo;
            var billToEndCustomer = billTo;
            var endCustomerShippingContactPerson = billToContactPerson;
            var endCustomerBillingContactPerson = billToContactPerson;
            var advancePayment = 10m;
            var vatRegime = new VatRegimes(this.Transaction).BelgiumStandard;
            var customerReference = "CustomerReference";

            var @class = this.M.SalesInvoice;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new SalesinvoiceCreateFormComponent(this.OverlayContainer);

            await form.SalesInvoiceTypeSelect.SelectAsync(salesInvoiceType);
            await form.BillToCustomerAutocomplete.SelectAsync(billTo.Name);
            await form.BillToContactPersonSelect.SelectAsync(billToContactPerson);
            await form.ShipToContactPersonSelect.SelectAsync(billToShipingContactPerson);
            await form.ShipToEndCustomerAutocomplete.SelectAsync(shipToEndCustomer.Name);
            await form.ShipToEndCustomerContactPersonSelect.SelectAsync(endCustomerShippingContactPerson);
            await form.BillToEndCustomerAutocomplete.SelectAsync(billToEndCustomer.Name);
            await form.BillToEndCustomerContactPersonSelect.SelectAsync(endCustomerBillingContactPerson);
            await form.AdvancePaymentInput.SetValueAsync(advancePayment.ToString());
            await form.DerivedVatRegimeSelect.SelectAsync(vatRegime);
            await form.CustomerReferenceInput.SetValueAsync(customerReference);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new SalesInvoices(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var salesInvoice = after.Except(before).First();

            Assert.AreEqual(salesInvoiceType, salesInvoice.SalesInvoiceType);
            Assert.AreEqual(billTo, salesInvoice.BillToCustomer);
            Assert.AreEqual(billToContactPerson, salesInvoice.BillToContactPerson);
            Assert.AreEqual(billToShipingContactPerson, salesInvoice.ShipToContactPerson);
            Assert.AreEqual(shipToEndCustomer, salesInvoice.ShipToEndCustomer);
            Assert.AreEqual(billToEndCustomer, salesInvoice.BillToEndCustomer);
            Assert.AreEqual(endCustomerShippingContactPerson, salesInvoice.ShipToEndCustomerContactPerson);
            Assert.AreEqual(endCustomerBillingContactPerson, salesInvoice.BillToEndCustomerContactPerson);
            Assert.AreEqual(advancePayment, salesInvoice.AdvancePayment);
            Assert.AreEqual(vatRegime, salesInvoice.DerivedVatRegime);
            Assert.AreEqual(customerReference, salesInvoice.CustomerReference);
        }
    }
}
