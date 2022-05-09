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
    }
}
