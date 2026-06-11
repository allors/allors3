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

    public class NonUnifiedPartTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task CreateNonUnifiedPartMinimal()
        {
            var before = new NonUnifiedParts(this.Transaction).Extent().ToArray();
            var facility = new Facilities(this.Transaction).Extent().First();
            var inventoryItemKind = new InventoryItemKinds(this.Transaction).Extent().First();

            var @class = this.M.NonUnifiedPart;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new NonunifiedpartCreateFormComponent(this.OverlayContainer);

            await form.NameInput.SetValueAsync("TempName");
            await form.DefaultFacilitySelect.SelectAsync(facility);
            await form.InventoryItemKindSelect.SelectAsync(inventoryItemKind);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new NonUnifiedParts(this.Transaction).Extent().ToArray();

            ClassicAssert.AreEqual(before.Length + 1, after.Length);

            var nonUnifiedPart = after.Except(before).First();

            ClassicAssert.AreEqual("TempName", nonUnifiedPart.Name);
            ClassicAssert.AreEqual(facility, nonUnifiedPart.DefaultFacility);
            ClassicAssert.AreEqual(inventoryItemKind, nonUnifiedPart.InventoryItemKind);
        }

        [Test]
        public async Task CreateNonUnifiedPartMaximal()
        {
            var before = new NonUnifiedParts(this.Transaction).Extent().ToArray();
            var facility = new Facilities(this.Transaction).Extent().First();
            var inventoryItemKind = new InventoryItemKinds(this.Transaction).Extent().First();
            var brand = new Brands(this.Transaction).Extent().First();
            var productType = new ProductTypes(this.Transaction).Extent().First();
            var manufacteredBy = new Organisations(this.Transaction).Extent().First(v => v.IsManufacturer);
            var categorie = new PartCategories(this.Transaction).Extent().First();

            var @class = this.M.NonUnifiedPart;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new NonunifiedpartCreateFormComponent(this.OverlayContainer);

            await form.NameInput.SetValueAsync("TempName");
            await form.DefaultFacilitySelect.SelectAsync(facility);
            await form.ProductTypeSelect.SelectAsync(productType);
            await form.InventoryItemKindSelect.SelectAsync(inventoryItemKind);
            await form.ManufacturedByAutocomplete.SelectAsync(manufacteredBy.Name);
            await form.HsCodeInput.SetValueAsync("4202 21");

            //TODO: Koen Brand, Category en Model staan niet in form

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new NonUnifiedParts(this.Transaction).Extent().ToArray();

            ClassicAssert.AreEqual(before.Length + 1, after.Length);

            var nonUnifiedPart = after.Except(before).First();

            ClassicAssert.AreEqual("TempName", nonUnifiedPart.Name);
            ClassicAssert.AreEqual(facility, nonUnifiedPart.DefaultFacility);
            ClassicAssert.AreEqual(inventoryItemKind, nonUnifiedPart.InventoryItemKind);
            ClassicAssert.AreEqual(productType, nonUnifiedPart.ProductType);
            ClassicAssert.AreEqual(inventoryItemKind, nonUnifiedPart.InventoryItemKind);
            ClassicAssert.AreEqual(manufacteredBy.Name, nonUnifiedPart.ManufacturedBy.DisplayName);
            ClassicAssert.AreEqual("4202 21", nonUnifiedPart.HsCode);
        }

        [Test]
        public async Task EditNonUnifiedPartPreservesCategories()
        {
            var part = new NonUnifiedParts(this.Transaction).Extent()
                .First(v => v.PartCategoriesWherePart.Count() >= 2);
            var originalCategories = part.PartCategoriesWherePart.ToArray();

            var @class = this.M.NonUnifiedPart;

            var url = this.Application.GetOverview(@class).RouteInfo.FullPath.Replace(":id", $"{part.Strategy.ObjectId}");
            await this.Page.GotoAsync(url);
            await this.Page.WaitForAngular();

            var overview = new NonunifiedpartOverviewPageComponent(this.AppRoot);
            await overview.AllorsMaterialDynamicViewDetailPanelComponent.ClickAsync();
            await this.Page.WaitForAngular();

            var editForm = new NonunifiedpartEditFormComponent(this.AppRoot);
            await editForm.NameInput.SetValueAsync("Edited NonUnifiedPart");
            await this.Page.WaitForAngular();

            var saveComponent = new SaveComponent(this.AppRoot);
            await saveComponent.SaveAsync();
            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = part.PartCategoriesWherePart.ToArray();
            foreach (var category in originalCategories)
            {
                ClassicAssert.Contains(category, after, $"part category '{category.Name}' was dropped on save");
            }

            ClassicAssert.AreEqual(originalCategories.Length, after.Length);
        }

    }
}
