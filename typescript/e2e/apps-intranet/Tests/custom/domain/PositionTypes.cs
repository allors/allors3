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

    public class PositionTypesTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task CreateMinimal()
        {
            var before = new PositionTypes(this.Transaction).Extent().ToArray();

            var @class = this.M.PositionType;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new PositiontypeFormComponent(this.OverlayContainer);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new PositionTypes(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var positionType = after.Except(before).First();
        }

        [Test]
        public async Task CreateMaximum()
        {
            var before = new PositionTypes(this.Transaction).Extent().ToArray();

            var @class = this.M.PositionType;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new PositiontypeFormComponent(this.OverlayContainer);

            await form.TitleInput.SetAsync("Title");
            await form.DescriptionInput.SetAsync("Description");

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new PositionTypes(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var positionType = after.Except(before).First();

            Assert.AreEqual("Title", positionType.Title);
            Assert.AreEqual("Description", positionType.Description);
        }        
    }
}
