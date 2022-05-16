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
    using Allors.E2E.Angular.Material.Sidenav;
    using Allors.E2E.Test;
    using E2E;
    using NUnit.Framework;
    using Task = System.Threading.Tasks.Task;

    public class PersonTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task Create()
        {
            var before = new People(this.Transaction).Extent().ToArray();

            var @class = this.M.Person;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new PersonCreateDialogComponent(this.OverlayContainer);

            await form.FirstNameInput.SetValueAsync("Jos");
            await form.LastNameInput.SetValueAsync("Smos");

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new People(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var person = after.Except(before).First();

            Assert.AreEqual("Jos", person.FirstName);
            Assert.AreEqual("Smos", person.LastName);
        }

        [Test]
        public async Task EditDetail()
        {
            var person = new People(this.Transaction).FindBy(this.M.Person.FirstName, "John");

            var @class = this.M.Person;

            var overview = this.Application.GetOverview(@class);

            var url = overview.RouteInfo.FullPath.Replace(":id", $"{person.Strategy.ObjectId}");
            await this.Page.GotoAsync(url);
            await this.Page.WaitForAngular();

            var detail = this.AppRoot.Locator.Locator("[data-allors-kind='view-detail-panel']");
            await detail.ClickAsync();
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
        public async Task AddEmployment()
        {
            var before = new Employments(this.Transaction).Extent().ToArray();

            var person = new People(this.Transaction).FindBy(this.M.Person.FirstName, "John");
            var organisation = new Organisations(this.Transaction).FindBy(this.M.Organisation.Name, "Acme");

            var @class = this.M.Person;

            var overview = this.Application.GetOverview(@class);
            var url = overview.RouteInfo.FullPath.Replace(":id", $"{person.Strategy.ObjectId}");
            await this.Page.GotoAsync(url);
            await this.Page.WaitForAngular();

            var personOverview = new PersonOverviewPageComponent(this.AppRoot);

            await personOverview.ViewEmployment.Locator.ClickAsync();
            await this.Page.WaitForAngular();

            var edit = personOverview.EditEmployment;

            var objectIds = await edit.Table.GetObjectIds();
            Assert.AreEqual(0, objectIds.Length);

            await edit.FactoryFab.Create(this.M.Employment);

            var form = new EmploymentFormComponent(this.OverlayContainer);
            await form.EmployerAutocomplete.SelectAsync(organisation.Name);

            var saveComponent = new SaveComponent(this.OverlayContainer);
            await saveComponent.SaveAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new Employments(this.Transaction).Extent().ToArray();

            var employments = after.Except(before).ToArray();

            Assert.AreEqual(1, employments.Length);

            var employment = employments[0];

            objectIds = await edit.Table.GetObjectIds();
            Assert.AreEqual(1, objectIds.Length);
            Assert.AreEqual(employment.Strategy.ObjectId.ToString(), objectIds[0]);

            Assert.AreEqual(organisation, employment.Employer);
            Assert.AreEqual(person, employment.Employee);
        }


        [Test]
        public async Task DeleteEmployment()
        {
            var before = new Employments(this.Transaction).Extent().ToArray();

            var person = new People(this.Transaction).FindBy(this.M.Person.FirstName, "Jane");

            var employment = before.First(v => person.Equals(v.Employee));

            var @class = this.M.Person;

            var overview = this.Application.GetOverview(@class);
            var url = overview.RouteInfo.FullPath.Replace(":id", $"{person.Strategy.ObjectId}");
            await this.Page.GotoAsync(url);
            await this.Page.WaitForAngular();

            var personOverview = new PersonOverviewPageComponent(this.AppRoot);

            await personOverview.ViewEmployment.Locator.ClickAsync();
            await this.Page.WaitForAngular();

            var edit = personOverview.EditEmployment;

            await edit.Table.Action(employment, "delete");

            await this.Page.WaitForAngular();

            var dialog = new AllorsMaterialDialogComponent(this.OverlayContainer);
            await dialog.YesButton.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new Employments(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length - 1, after.Length);
        }
    }
}
