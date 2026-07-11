// <copyright file="CustomerShipmentTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.E2E.Objects
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
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
            // Build a dedicated shipment with a guaranteed ShipToContactPerson, independent of the shared
            // population (whose single WithDefaults shipment takes its contact from a random customer that
            // may be a B2C person with no CurrentContacts). Repoint ShipTo to a B2B customer that has a
            // contact — done post-build via direct role assignment, not on the builder.
            var internalOrganisation = new Organisations(this.Transaction).FindBy(this.M.Organisation.Name, "Allors BV");
            var customer = internalOrganisation.ActiveCustomers.OfType<Organisation>().First(v => v.CurrentContacts.Any());

            var shipment = new CustomerShipmentBuilder(this.Transaction).WithDefaults(internalOrganisation).Build();
            this.Transaction.Derive();

            shipment.ShipToParty = customer;
            shipment.ShipToContactPerson = customer.CurrentContacts.First();
            this.Transaction.Derive();
            this.Transaction.Commit();

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
