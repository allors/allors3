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

    public class NonUnifiedGoodTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task CreateNonUnifiedGoodMinimal()
        {
            var before = new NonUnifiedGoods(this.Transaction).Extent().ToArray();
            var part = new NonUnifiedParts(this.Transaction).Extent().First();

            var @class = this.M.NonUnifiedGood;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new NonunifiedgoodCreateFormComponent(this.OverlayContainer);

            await form.NameInput.SetValueAsync("TempName");
            await form.PartAutocomplete.SelectAsync(part.DisplayName);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new NonUnifiedGoods(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var nonUnifiedGood = after.Except(before).First();

            Assert.AreEqual("TempName", nonUnifiedGood.Name);
            Assert.AreEqual(part, nonUnifiedGood.Part);
        }

        [Test]
        public async Task CreateNonUnifiedGoodMaximal()
        {
            var before = new NonUnifiedGoods(this.Transaction).Extent().ToArray();
            var part = new NonUnifiedParts(this.Transaction).Extent().First();
            var categorie = new ProductCategories(this.Transaction).Extent().First();

            var @class = this.M.NonUnifiedGood;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new NonunifiedgoodCreateFormComponent(this.OverlayContainer);

            await form.NameInput.SetValueAsync("TempName");
            await form.PartAutocomplete.SelectAsync(part.DisplayName);
            await form.DescriptionTextarea.SetAsync("Dit is een test description");
            //TODO: Koen Categories

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new NonUnifiedGoods(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var nonUnifiedGood = after.Except(before).First();

            Assert.AreEqual("TempName", nonUnifiedGood.Name);
            Assert.AreEqual(part, nonUnifiedGood.Part);
            Assert.AreEqual("Dit is een test description", nonUnifiedGood.Description);
        }

    }
}
