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

    public class ProductQuoteTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task CreateProductQuoteMinimal()
        {
            var before = new ProductQuotes(this.Transaction).Extent().ToArray();
            var organisation = new Organisations(this.Transaction).Extent().First(v => v.Name.Equals("Allors BV"));
            var customer = organisation.ActiveCustomers.First();

            var contactMechanism = customer.CurrentPartyContactMechanisms.First().ContactMechanism;
            var contactPerson = customer.CurrentContacts.First();

            var @class = this.M.ProductQuote;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new ProductquoteCreateFormComponent(this.OverlayContainer);

            await form.ReceiverAutocomplete.SelectAsync(customer.DisplayName);
            await form.FullfillContactMechanismSelect.SelectAsync(contactMechanism);
            await form.ContactPersonSelect.SelectAsync(contactPerson);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new ProductQuotes(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var productQuote = after.Except(before).First();

            Assert.AreEqual(customer, productQuote.Receiver);
            Assert.AreEqual(contactMechanism, productQuote.FullfillContactMechanism);
            Assert.AreEqual(contactPerson, productQuote.ContactPerson);
        }

        [Test]
        public async Task CreateProductQuoteMaximal()
        {
            var before = new ProductQuotes(this.Transaction).Extent().ToArray();
            var organisation = new Organisations(this.Transaction).Extent().First(v => v.Name.Equals("Allors BV"));
            var customer = organisation.ActiveCustomers.First();

            var contactMechanism = customer.CurrentPartyContactMechanisms.First().ContactMechanism;
            var contactPerson = customer.CurrentContacts.First();
            var date = DateTimeFactory.CreateDate(System.DateTime.Now);
            var tomorrow = date.AddDays(1);
            var currency = new Currencies(this.Transaction).Extent().First(x => x.ExistExchangeRatesWhereToCurrency);
            var vatRegime = new VatRegimes(this.Transaction).BelgiumStandard;

            var @class = this.M.ProductQuote;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new ProductquoteCreateFormComponent(this.OverlayContainer);

            await form.ReceiverAutocomplete.SelectAsync(customer.DisplayName);
            await form.FullfillContactMechanismSelect.SelectAsync(contactMechanism);
            await form.ContactPersonSelect.SelectAsync(contactPerson);
            await form.ValidFromDateDatepicker.SetAsync(date);
            await form.ValidThroughDateDatepicker.SetAsync(tomorrow);
            await form.IssueDateDatepicker.SetAsync(date);
            await form.RequiredResponseDateDatepicker.SetAsync(tomorrow);
            await form.DerivedCurrencySelect.SetAsync(currency);
            await form.DerivedVatRegimeSelect.SetAsync(vatRegime);
            await form.DescriptionTextarea.SetAsync("Description");
            await form.CommentTextarea.SetAsync("Comment");
            await form.InternalCommentTextarea.SetAsync("Internal Comment");


            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new ProductQuotes(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var productQuote = after.Except(before).First();

            Assert.AreEqual(customer, productQuote.Receiver);
            Assert.AreEqual(contactMechanism, productQuote.FullfillContactMechanism);
            Assert.AreEqual(contactPerson, productQuote.ContactPerson);
            Assert.AreEqual(date, productQuote.ValidFromDate.Value.Date);
            Assert.AreEqual(tomorrow, productQuote.ValidThroughDate.Value.Date);
            Assert.AreEqual(date, productQuote.IssueDate.Date);
            Assert.AreEqual(tomorrow, productQuote.RequiredResponseDate.Value.Date);
            Assert.AreEqual(currency, productQuote.AssignedCurrency);
            Assert.AreEqual(vatRegime, productQuote.AssignedVatRegime);
            Assert.AreEqual("Description", productQuote.Description);
            Assert.AreEqual("Comment", productQuote.Comment);
            Assert.AreEqual("Internal Comment", productQuote.InternalComment);
        }
    }
}
