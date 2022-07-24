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

            Assert.AreEqual(before.Length + 1, after.Length);

            var unifiedGood = after.Except(before).First();

            Assert.AreEqual("Driesjes", unifiedGood.Name);
            Assert.AreEqual(inventoryItemKind, unifiedGood.InventoryItemKind);
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

            Assert.AreEqual(before.Length + 1, after.Length);

            var unifiedGood = after.Except(before).First();

            Assert.AreEqual("TempName", unifiedGood.Name);
            Assert.AreEqual(inventoryItemKind, unifiedGood.InventoryItemKind);
            Assert.AreEqual(productType, unifiedGood.ProductType);
        }

    }
}
