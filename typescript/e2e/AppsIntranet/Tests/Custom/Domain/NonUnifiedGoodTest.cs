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

            ClassicAssert.AreEqual(before.Length + 1, after.Length);

            var nonUnifiedGood = after.Except(before).First();

            ClassicAssert.AreEqual("TempName", nonUnifiedGood.Name);
            ClassicAssert.AreEqual(part, nonUnifiedGood.Part);
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

            ClassicAssert.AreEqual(before.Length + 1, after.Length);

            var nonUnifiedGood = after.Except(before).First();

            ClassicAssert.AreEqual("TempName", nonUnifiedGood.Name);
            ClassicAssert.AreEqual(part, nonUnifiedGood.Part);
            ClassicAssert.AreEqual("Dit is een test description", nonUnifiedGood.Description);
        }

        [Test]
        public async Task EditNonUnifiedGoodPreservesCategories()
        {
            var good = new NonUnifiedGoods(this.Transaction).Extent()
                .First(v => v.ProductCategoriesWhereProduct.Count() >= 2);
            var originalCategories = good.ProductCategoriesWhereProduct.ToArray();

            var @class = this.M.NonUnifiedGood;

            var url = this.Application.GetOverview(@class).RouteInfo.FullPath.Replace(":id", $"{good.Strategy.ObjectId}");
            await this.Page.GotoAsync(url);
            await this.Page.WaitForAngular();

            var overview = new NonunifiedgoodOverviewPageComponent(this.AppRoot);
            await overview.AllorsMaterialDynamicViewDetailPanelComponent.ClickAsync();
            await this.Page.WaitForAngular();

            var editForm = new NonunifiedgoodEditFormComponent(this.AppRoot);
            await editForm.DescriptionTextarea.SetAsync("touched to enable save");
            await this.Page.WaitForAngular();

            var saveComponent = new SaveComponent(this.AppRoot);
            await saveComponent.SaveAsync();
            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = good.ProductCategoriesWhereProduct.ToArray();
            foreach (var category in originalCategories)
            {
                ClassicAssert.Contains(category, after, $"category '{category.Name}' was dropped on save");
            }

            ClassicAssert.AreEqual(originalCategories.Length, after.Length);
        }

        [Test]
        public async Task EditNonUnifiedGoodWithoutCategoriesDoesNotThrow()
        {
            // A freshly-created good has no product categories; editing+saving it must not throw.
            var part = new NonUnifiedParts(this.Transaction).Extent().First();

            var @class = this.M.NonUnifiedGood;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var createForm = new NonunifiedgoodCreateFormComponent(this.OverlayContainer);
            await createForm.NameInput.SetValueAsync("CategorylessGood");
            await createForm.PartAutocomplete.SelectAsync(part.DisplayName);
            await new Button(createForm, "text=SAVE").ClickAsync();
            await this.Page.WaitForAngular();

            this.Transaction.Rollback();
            var good = new NonUnifiedGoods(this.Transaction).Extent().First(v => v.Name == "CategorylessGood");
            ClassicAssert.IsEmpty(good.ProductCategoriesWhereProduct);

            var url = this.Application.GetOverview(@class).RouteInfo.FullPath.Replace(":id", $"{good.Strategy.ObjectId}");
            await this.Page.GotoAsync(url);
            await this.Page.WaitForAngular();

            var overview = new NonunifiedgoodOverviewPageComponent(this.AppRoot);
            await overview.AllorsMaterialDynamicViewDetailPanelComponent.ClickAsync();
            await this.Page.WaitForAngular();

            var editForm = new NonunifiedgoodEditFormComponent(this.AppRoot);
            await editForm.DescriptionTextarea.SetAsync("touched to enable save");
            await this.Page.WaitForAngular();

            var saveComponent = new SaveComponent(this.AppRoot);
            await saveComponent.SaveAsync();
            await this.Page.WaitForAngular();

            this.Transaction.Rollback();
            ClassicAssert.IsEmpty(good.ProductCategoriesWhereProduct);
        }

    }
}
