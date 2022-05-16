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
    using Allors.E2E.Angular.Info;
    using Allors.E2E.Angular.Material.Factory;
    using Allors.E2E.Test;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class PersonTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task CreateMinimal()
        {
            var before = new People(this.Transaction).Extent().ToArray();

            var @class = this.M.Person;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new PersonFormComponent(this.OverlayContainer);

            await form.FirstNameInput.SetValueAsync("Jos");

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new People(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var person = after.Except(before).First();

            Assert.AreEqual("Jos", person.FirstName);
        }

        [Test]
        public async Task EditDetail()
        {
            var person = new People(this.Transaction).Extent().First();

            var @class = this.M.Person;

            var url = this.Application.GetOverview(@class).RouteInfo.FullPath.Replace(":id", $"{person.Strategy.ObjectId}");
            await this.Page.GotoAsync(url);
            await this.Page.WaitForAngular();

            var overview = new PersonOverviewPageComponent(this.AppRoot);
            await overview.AllorsMaterialDynamicViewDetailPanelComponent.ClickAsync();
            await this.Page.WaitForAngular();

            var form = new PersonFormComponent(this.AppRoot);
            await form.FirstNameInput.SetValueAsync("Jenny");
            await form.LastNameInput.SetValueAsync("Penny");

            var saveComponent = new SaveComponent(this.AppRoot);
            await saveComponent.SaveAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            Assert.AreEqual("Jenny", person.FirstName);
            Assert.AreEqual("Penny", person.LastName);
        }

        [Test]
        public async Task AddEmailCommunication()
        {
            var before = new EmailCommunications(this.Transaction).Extent().ToArray();

            var person = new People(this.Transaction).Extent().First(v => v.CurrentPartyContactMechanisms.Any(v => v.ContactMechanism is EmailAddress));

            var @class = this.M.Person;

            var overview = this.Application.GetOverview(@class);
            var url = overview.RouteInfo.FullPath.Replace(":id", $"{person.Strategy.ObjectId}");
            await this.Page.GotoAsync(url);
            await this.Page.WaitForAngular();

            var personOverview = new PersonOverviewPageComponent(this.AppRoot);

            await personOverview.ViewCommunicationEvent.Locator.ClickAsync();
            await this.Page.WaitForAngular();

            var edit = personOverview.EditCommunicationEvent;

            var objectIds = await edit.Table.GetObjectIds();
            Assert.AreEqual(0, objectIds.Length);

            await edit.FactoryFab.Create(this.M.EmailCommunication);

            var form = new EmailcommunicationFormComponent(this.OverlayContainer);
            await form.FromPartySelect.SelectAsync(person);
            await form.ToPartySelect.SelectAsync(person);
            await form.ToEmailSelect.SelectAsync(0);
            await form.SubjectTemplateInput.SetAsync("You got mail");

            var saveComponent = new SaveComponent(this.OverlayContainer);
            await saveComponent.SaveAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new EmailCommunications(this.Transaction).Extent().ToArray();

            var emailCommunications = after.Except(before).ToArray();

            Assert.AreEqual(1, emailCommunications.Length);

            var emailCommunication = emailCommunications[0];

            objectIds = await edit.Table.GetObjectIds();
            Assert.AreEqual(1, objectIds.Length);
            Assert.AreEqual(emailCommunication.Strategy.ObjectId.ToString(), objectIds[0]);

            Assert.AreEqual(person, emailCommunication.FromParty);
        }


        //[Test]
        //public async Task DeleteEmployment()
        //{
        //    var before = new Employments(this.Transaction).Extent().ToArray();

        //    var person = new People(this.Transaction).FindBy(this.M.Person.FirstName, "Jane");

        //    var employment = before.First(v => person.Equals(v.Employee));

        //    var @class = this.M.Person;

        //    var overview = this.Application.GetOverview(@class);
        //    var url = overview.RouteInfo.FullPath.Replace(":id", $"{person.Strategy.ObjectId}");
        //    await this.Page.GotoAsync(url);
        //    await this.Page.WaitForAngular();

        //    var personOverview = new PersonOverviewPageComponent(this.AppRoot);

        //    await personOverview.ViewEmployment.Locator.ClickAsync();
        //    await this.Page.WaitForAngular();

        //    var edit = personOverview.EditEmployment;

        //    await edit.Table.Action(employment, "delete");

        //    await this.Page.WaitForAngular();

        //    var dialog = new AllorsMaterialDialogComponent(this.OverlayContainer);
        //    await dialog.YesButton.ClickAsync();

        //    await this.Page.WaitForAngular();

        //    this.Transaction.Rollback();

        //    var after = new Employments(this.Transaction).Extent().ToArray();

        //    Assert.AreEqual(before.Length - 1, after.Length);
        //}
    }
}
