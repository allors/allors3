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

    public class UnifiedGoodTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task CreateUnifiedGoodMinimal()
        {
            var before = new UnifiedGoods(this.Transaction).Extent().ToArray();
            var inventoryItemKind = new InventoryItemKinds(this.Transaction).NonSerialised;

            var @class = this.M.UnifiedGood;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new UnifiedgoodCreateFormComponent(this.OverlayContainer);

            await form.NameInput.SetValueAsync("Driesjes");
            await form.InventoryItemKindSelect.SelectAsync(inventoryItemKind);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new UnifiedGoods(this.Transaction).Extent().ToArray();

            ClassicAssert.AreEqual(before.Length + 1, after.Length);

            var unifiedGood = after.Except(before).First();

            ClassicAssert.AreEqual("Driesjes", unifiedGood.Name);
            ClassicAssert.AreEqual(inventoryItemKind, unifiedGood.InventoryItemKind);
        }

        [Test]
        public async Task CreateUnifiedGoodMaximal()
        {
            var before = new UnifiedGoods(this.Transaction).Extent().ToArray();
            var inventoryItemKind = new InventoryItemKinds(this.Transaction).NonSerialised;
            var productType = new ProductTypes(this.Transaction).Extent().First();

            var @class = this.M.UnifiedGood;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new UnifiedgoodCreateFormComponent(this.OverlayContainer);

            await form.NameInput.SetValueAsync("TempName");
            await form.InventoryItemKindSelect.SelectAsync(inventoryItemKind);
            await form.ProductTypeSelect.SelectAsync(productType);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new UnifiedGoods(this.Transaction).Extent().ToArray();

            ClassicAssert.AreEqual(before.Length + 1, after.Length);

            var unifiedGood = after.Except(before).First();

            ClassicAssert.AreEqual("TempName", unifiedGood.Name);
            ClassicAssert.AreEqual(inventoryItemKind, unifiedGood.InventoryItemKind);
            ClassicAssert.AreEqual(productType, unifiedGood.ProductType);
        }

        [Test]
        public async Task EditUnifiedGoodPreservesCategories()
        {
            var good = new UnifiedGoods(this.Transaction).Extent()
                .First(v => v.ProductCategoriesWhereProduct.Count() >= 2);
            var originalCategories = good.ProductCategoriesWhereProduct.ToArray();

            var @class = this.M.UnifiedGood;

            var url = this.Application.GetOverview(@class).RouteInfo.FullPath.Replace(":id", $"{good.Strategy.ObjectId}");
            await this.Page.GotoAsync(url);
            await this.Page.WaitForAngular();

            var overview = new UnifiedgoodOverviewPageComponent(this.AppRoot);
            await overview.AllorsMaterialDynamicViewDetailPanelComponent.ClickAsync();
            await this.Page.WaitForAngular();

            var editForm = new UnifiedgoodEditFormComponent(this.AppRoot);
            await editForm.NameInput.SetValueAsync("Edited UnifiedGood");
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

    }
}
