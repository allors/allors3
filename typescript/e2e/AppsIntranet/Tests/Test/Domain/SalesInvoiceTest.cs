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
        public async Task CreateSalesInvoiceMinimal()
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

            ClassicAssert.AreEqual(before.Length + 1, after.Length);

            var salesInvoice = after.Except(before).First();

            ClassicAssert.AreEqual(salesInvoiceType, salesInvoice.SalesInvoiceType);
            ClassicAssert.AreEqual(billTo, salesInvoice.BillToCustomer);
        }

        [Test]
        public async Task CreateSalesInvoiceMaximal()
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

            ClassicAssert.AreEqual(before.Length + 1, after.Length);

            var salesInvoice = after.Except(before).First();

            ClassicAssert.AreEqual(salesInvoiceType, salesInvoice.SalesInvoiceType);
            ClassicAssert.AreEqual(billTo, salesInvoice.BillToCustomer);
            ClassicAssert.AreEqual(billToContactPerson, salesInvoice.BillToContactPerson);
            ClassicAssert.AreEqual(billToShipingContactPerson, salesInvoice.ShipToContactPerson);
            ClassicAssert.AreEqual(shipToEndCustomer, salesInvoice.ShipToEndCustomer);
            ClassicAssert.AreEqual(billToEndCustomer, salesInvoice.BillToEndCustomer);
            ClassicAssert.AreEqual(endCustomerShippingContactPerson, salesInvoice.ShipToEndCustomerContactPerson);
            ClassicAssert.AreEqual(endCustomerBillingContactPerson, salesInvoice.BillToEndCustomerContactPerson);
            ClassicAssert.AreEqual(advancePayment, salesInvoice.AdvancePayment);
            ClassicAssert.AreEqual(vatRegime, salesInvoice.DerivedVatRegime);
            ClassicAssert.AreEqual(customerReference, salesInvoice.CustomerReference);
        }
    }
}
