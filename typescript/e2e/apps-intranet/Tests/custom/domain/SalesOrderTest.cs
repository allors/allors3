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

    public class SalesOrderTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task CreateMinimal()
        {
            var before = new SalesOrders(this.Transaction).Extent().ToArray();
            var organisation = new Organisations(this.Transaction).Extent().First(v => v.Name.Equals("Allors BV"));
            var customer = organisation.ActiveCustomers.First();

            var @class = this.M.SalesOrder;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new SalesorderCreateFormComponent(this.OverlayContainer);

            await form.ShipToCustomerAutocomplete.SelectAsync(customer.DisplayName);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new SalesOrders(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var salesOrder = after.Except(before).First();

            Assert.AreEqual(customer, salesOrder.ShipToCustomer);
        }

        [Test]
        public async Task CreateMaximum()
        {
            var before = new SalesOrders(this.Transaction).Extent().ToArray();
            var organisation = new Organisations(this.Transaction).Extent().First(v => v.Name.Equals("Allors BV"));
            var customer = organisation.ActiveCustomers.First();

            var contactMechanism = customer.CurrentPartyContactMechanisms.First().ContactMechanism;
            var contactPerson = customer.CurrentContacts.First();
            
            var currency = new Currencies(this.Transaction).Extent().First(x => x.ExistExchangeRatesWhereToCurrency);
            var vatRegime = new VatRegimes(this.Transaction).BelgiumStandard;
            
            //organisation.PartyContactMechanismsWhereParty.First()
            var @class = this.M.SalesOrder;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new SalesorderCreateFormComponent(this.OverlayContainer);

            await form.ShipToCustomerAutocomplete.SelectAsync(customer.DisplayName);
            await form.ShipToContactPersonSelect.SetAsync(contactPerson);
            await form.BillToContactPersonSelect.SetAsync(contactPerson);
            await form.ShipToEndCustomerAutocomplete.SelectAsync(customer.DisplayName);
            await form.ShipToEndCustomerContactPersonSelect.SetAsync(contactPerson);
            await form.BillToEndCustomerContactPersonSelect.SetAsync(contactPerson);
            await form.DerivedVatRegimeSelect.SetAsync(vatRegime);
            await form.CustomerReferenceInput.SetAsync("Customer Reference");
            await form.DerivedCurrencySelect.SetAsync(currency);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new SalesOrders(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var salesOrder = after.Except(before).First();

            Assert.AreEqual(customer, salesOrder.ShipToCustomer);
            Assert.AreEqual(customer, salesOrder.ShipToEndCustomer);
            Assert.AreEqual(customer, salesOrder.BillToCustomer);
            Assert.AreEqual(customer, salesOrder.BillToEndCustomer);
            Assert.AreEqual(contactPerson, salesOrder.ShipToContactPerson);
            Assert.AreEqual(contactPerson, salesOrder.BillToContactPerson);
            Assert.AreEqual(contactPerson, salesOrder.ShipToEndCustomerContactPerson);
            Assert.AreEqual(contactPerson, salesOrder.BillToEndCustomerContactPerson);
            Assert.AreEqual(currency, salesOrder.AssignedCurrency);
            Assert.AreEqual(vatRegime, salesOrder.AssignedVatRegime);
            Assert.AreEqual("Customer Reference", salesOrder.CustomerReference);

        }
    }
}
