// <copyright file="PersonEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Objects
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.E2E.Angular;
    using Allors.E2E.Angular.Html;
    using Allors.E2E.Angular.Material.Factory;
    using Allors.E2E.Test;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class ProductQuotesTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task CreateMinimal()
        {
            var before = new RequestsForQuote(this.Transaction).Extent().ToArray();
            var organisation = new Organisations(this.Transaction).Extent().First();

            var contactMechanism = organisation.CurrentPartyContactMechanisms.First().ContactMechanism;
            var contactPerson = organisation.CurrentContacts.First();

            var @class = this.M.RequestForQuote;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new ProductquoteCreateFormComponent(this.OverlayContainer);

            form.ReceiverAutocomplete.SelectAsync(organisation.DisplayName);
            form.FullfillContactMechanismSelect.SelectAsync(contactMechanism);
            form.ContactPersonSelect.SelectAsync(contactPerson);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new RequestsForQuote(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var productQuotes = after.Except(before).First();
            

        }
    }
}
