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

    public class RequestForQuoteTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task CreateMinimal()
        {
            var before = new RequestsForQuote(this.Transaction).Extent().ToArray();

            var @class = this.M.RequestForQuote;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new RequestforquoteCreateFormComponent(this.OverlayContainer);

            // TODO: investigate required fields

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new RequestsForQuote(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var request = after.Except(before).First();

            Assert.NotNull(request);
        }

        [Test]
        public async Task CreateMaximum()
        {
            var before = new RequestsForQuote(this.Transaction).Extent().ToArray();
            var organisation = new Organisations(this.Transaction).Extent().First();

            var contactMechanism = organisation.CurrentPartyContactMechanisms.First().ContactMechanism;
            var contactPerson = organisation.CurrentContacts.First();


            var dateNow = System.DateTime.Today;
            var dateTommorow = dateNow.AddDays(1);

            var @class = this.M.RequestForQuote;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new RequestforquoteCreateFormComponent(this.OverlayContainer);

            await form.OriginatorAutocomplete.SelectAsync(organisation.DisplayName);
            await form.FullfillContactMechanismSelect.SelectAsync(contactMechanism);
            await form.ContactPersonSelect.SelectAsync(contactPerson);

            await form.RequestDateDatepicker.SetAsync(dateNow);
            await form.RequiredResponseDateDatepicker.SetAsync(dateTommorow);

            await form.DescriptionTextarea.SetAsync("Description");
            await form.CommentTextarea.SetAsync("Comment");
            await form.InternalCommentTextarea.SetAsync("Internal comment");

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new RequestsForQuote(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var request = after.Except(before).First();
            
            Assert.AreEqual(organisation, request.Originator);
            Assert.AreEqual(contactMechanism, request.FullfillContactMechanism);
            Assert.AreEqual(contactPerson, request.ContactPerson);
            Assert.AreEqual(dateNow.Date, request.RequestDate.Date);
            Assert.AreEqual("Description", request.Description);
            Assert.AreEqual("Comment", request.Comment);
            Assert.AreEqual("Internal comment", request.InternalComment);
        }
    }
}
