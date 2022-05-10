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

    public class SerialisedItemCharacteristicsTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task CreateMinimal()
        {
            //TODO: Koen route aanpassen

            var before = new SerialisedItemCharacteristicTypes(this.Transaction).Extent().ToArray();

            var @class = this.M.SerialisedItemCharacteristic;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new SerialiseditemcharacteristictypeFormComponent(this.OverlayContainer);

            await form.NameInput.SetValueAsync("Joren");

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new SerialisedItemCharacteristicTypes(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var serialisedItemCharacteristicType = after.Except(before).First();

            Assert.AreEqual("Joren", serialisedItemCharacteristicType.Name);
        }

        [Test]
        public async Task CreateMaximum()
        {

            var before = new SerialisedItemCharacteristicTypes(this.Transaction).Extent().ToArray();
            var unitOfMeasure = new UnitsOfMeasure(this.Transaction).Kilogram;

            var @class = this.M.SerialisedItemCharacteristic;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new SerialiseditemcharacteristictypeFormComponent(this.OverlayContainer);

            await form.NameInput.SetValueAsync("Joren");
            await form.UnitOfMeasureSelect.SelectAsync(unitOfMeasure);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new SerialisedItemCharacteristicTypes(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var serialisedItemCharacteristicType = after.Except(before).First();

            Assert.AreEqual("Joren", serialisedItemCharacteristicType.Name);
            Assert.AreEqual(unitOfMeasure, serialisedItemCharacteristicType.UnitOfMeasure);
        }
    }
}
