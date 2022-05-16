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

    public class ProductTypeTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task CreateMinimal()
        {
            var before = new ProductTypes(this.Transaction).Extent().ToArray();

            var @class = this.M.ProductType;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new ProducttypeFormComponent(this.OverlayContainer);

            await form.NameInput.SetValueAsync("Joren");

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new ProductTypes(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var productType = after.Except(before).First();

            Assert.AreEqual("Joren", productType.Name);
        }

        [Test]
        public async Task CreateMaximum()
        {
            var before = new ProductTypes(this.Transaction).Extent().ToArray();
            var serialisedItemCharacteristicType = new SerialisedItemCharacteristicTypes(this.Transaction).Extent().First();

            var @class = this.M.ProductType;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new ProducttypeFormComponent(this.OverlayContainer);

            await form.NameInput.SetValueAsync("Joren");
            await form.SerialisedItemCharacteristicTypesSelect.SelectAsync(serialisedItemCharacteristicType);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new ProductTypes(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var productType = after.Except(before).First();

            Assert.AreEqual("Joren", productType.Name);
            Assert.AreEqual(1, productType.SerialisedItemCharacteristicTypes.Count());
            Assert.Contains(serialisedItemCharacteristicType, productType.SerialisedItemCharacteristicTypes.ToArray());
        }
    }
}
