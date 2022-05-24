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

    public class PositionTypeRateTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task CreateMinimal()
        {
            var before = new PositionTypeRates(this.Transaction).Extent().ToArray();
            var rateType = new RateTypes(this.Transaction).StandardRate;
            var frequency = new TimeFrequencies(this.Transaction).Day;
            var date = DateTimeFactory.CreateDate(System.DateTime.Now);

            var @class = this.M.PositionTypeRate;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new PositiontyperateFormComponent(this.OverlayContainer);

            await form.RateTypeSelect.SetAsync(rateType);
            await form.FromDateDatepicker.SetAsync(date);
            await form.FrequencySelect.SetAsync(frequency);
            await form.RateInput.SetAsync("10");

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new PositionTypeRates(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var positionTypeRate = after.Except(before).First();

            Assert.AreEqual(rateType, positionTypeRate.RateType);
            Assert.AreEqual(date, positionTypeRate.FromDate);
            Assert.AreEqual(frequency, positionTypeRate.Frequency);
            Assert.AreEqual(10D, positionTypeRate.Rate);
        }

        [Test]
        public async Task CreateMaximum()
        {
            var before = new PositionTypeRates(this.Transaction).Extent().ToArray();
            var rateType = new RateTypes(this.Transaction).StandardRate;
            var frequency = new TimeFrequencies(this.Transaction).Day;
            var date = DateTimeFactory.CreateDate(System.DateTime.Now);

            var @class = this.M.PositionTypeRate;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new PositiontyperateFormComponent(this.OverlayContainer);

            await form.RateTypeSelect.SetAsync(rateType);
            await form.FromDateDatepicker.SetAsync(date);
            await form.FrequencySelect.SetAsync(frequency);
            await form.RateInput.SetAsync("10");
            await form.CostInput.SetAsync("5");
            await form.ThroughDateDatepicker.SetAsync(date);
            // TODO: Koen position
            
            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new PositionTypeRates(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var positionTypeRate = after.Except(before).First();

            Assert.AreEqual(rateType, positionTypeRate.RateType);
            Assert.AreEqual(date, positionTypeRate.FromDate);
            Assert.AreEqual(frequency, positionTypeRate.Frequency);
            Assert.AreEqual(10D, positionTypeRate.Rate);
            Assert.AreEqual(5D, positionTypeRate.Cost);
            Assert.AreEqual(date, positionTypeRate.ThroughDate);
        }
    }
}
