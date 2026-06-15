// <copyright file="CustomerShipmentTest.cs" company="Allors bvba">
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

    public class CustomerShipmentTest : Test
    {
        [SetUp]
        public async Task Setup() => await this.LoginAsync("jane@example.com");

        [Test]
        public async Task EditCustomerShipmentPreservesShipToContactPerson()
        {
            var shipment = new CustomerShipments(this.Transaction).Extent()
                .First(v => v.ExistShipToContactPerson);
            var shipToContactPerson = shipment.ShipToContactPerson;

            var @class = this.M.CustomerShipment;

            var url = this.Application.GetOverview(@class).RouteInfo.FullPath.Replace(":id", $"{shipment.Strategy.ObjectId}");
            await this.Page.GotoAsync(url);
            await this.Page.WaitForAngular();

            var overview = new CustomershipmentOverviewPageComponent(this.AppRoot);
            await overview.AllorsMaterialDynamicViewDetailPanelComponent.ClickAsync();
            await this.Page.WaitForAngular();

            var editForm = new CustomershipmentEditFormComponent(this.AppRoot);
            await editForm.HandlingInstructionTextarea.SetAsync("touched to enable save");
            await this.Page.WaitForAngular();

            var saveComponent = new SaveComponent(this.AppRoot);
            await saveComponent.SaveAsync();
            await this.Page.WaitForAngular();

            this.Transaction.Rollback();

            ClassicAssert.AreEqual(shipToContactPerson, shipment.ShipToContactPerson, "ShipToContactPerson was cleared on edit/save");
        }
    }
}
