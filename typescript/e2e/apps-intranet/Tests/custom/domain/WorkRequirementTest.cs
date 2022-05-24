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

    public class WorkRequirementTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task CreateMinimal()
        {
            var before = new WorkRequirements(this.Transaction).Extent().ToArray();

            var @class = this.M.WorkRequirement;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new WorkrequirementCreateFormComponent(this.OverlayContainer);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new WorkRequirements(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);
        }

        [Test]
        public async Task CreateMaximum()
        {
            var before = new WorkRequirements(this.Transaction).Extent().ToArray();
            var organisation = new Organisations(this.Transaction).Extent().First(v => v.Name.Equals("Allors BV"));
            var originator = organisation.ActiveCustomers.First(v => v.SerialisedItemsWhereRentedBy.Any());
            var fixedAsset = originator.SerialisedItemsWhereRentedBy.First();
            var priority = new Priorities(this.Transaction).High;
            var location = "Location";
            var reason = "Reason";
            var unServiceable = true;

            var @class = this.M.WorkRequirement;

            var list = this.Application.GetList(@class);
            await this.Page.GotoAsync(list.RouteInfo.FullPath);
            await this.Page.WaitForAngular();

            var factory = new FactoryFabComponent(this.AppRoot);
            await factory.Create(@class);
            await this.Page.WaitForAngular();

            var form = new WorkrequirementCreateFormComponent(this.OverlayContainer);

            await form.OriginatorAutocomplete.SelectAsync(originator.DisplayName);
            await form.FixedAssetSelect.SelectAsync(fixedAsset);
            await form.PriorityRadioGroup.SelectAsync(priority);
            await form.LocationInput.SetAsync(location);
            await form.ReasonTextarea.SetAsync(reason);
            // TODO: Pictures
            await form.UnServiceableSlideToggle.SetAsync(unServiceable);

            var saveComponent = new Button(form, "text=SAVE");
            await saveComponent.ClickAsync();

            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            var after = new WorkRequirements(this.Transaction).Extent().ToArray();

            Assert.AreEqual(before.Length + 1, after.Length);

            var workRequirement = after.Except(before).First();

            Assert.AreEqual(originator, workRequirement.Originator);
            Assert.AreEqual(fixedAsset, workRequirement.FixedAsset);
            Assert.AreEqual(priority, workRequirement.Priority);
            Assert.AreEqual(location, workRequirement.Location);
            Assert.AreEqual(reason, workRequirement.Reason);
            Assert.AreEqual(unServiceable, workRequirement.UnServiceable);
        }
    }
}
