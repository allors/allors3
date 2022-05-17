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
        public async Task CreateMinimal()
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

            Assert.AreEqual(before.Length + 1, after.Length);

            var nonUnifiedPart = after.Except(before).First();

            Assert.AreEqual("TempName", nonUnifiedPart.Name);
            Assert.AreEqual(facility, nonUnifiedPart.DefaultFacility);
            Assert.AreEqual(inventoryItemKind, nonUnifiedPart.InventoryItemKind);
        }

        [Test]
        public async Task CreateMaximum()
        {
            var before = new NonUnifiedParts(this.Transaction).Extent().ToArray();
            var facility = new Facilities(this.Transaction).Extent().First();
            var inventoryItemKind = new InventoryItemKinds(this.Transaction).Extent().First();
            var brand = new Brands(this.Transaction).Extent().First();

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
            //TODO: Koen Brand en Model staan niet in form

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new NonUnifiedParts(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var nonUnifiedPart = after.Except(before).First();

            Assert.AreEqual("TempName", nonUnifiedPart.Name);
            Assert.AreEqual(facility, nonUnifiedPart.DefaultFacility);
            Assert.AreEqual(inventoryItemKind, nonUnifiedPart.InventoryItemKind);
        }

    }
}
