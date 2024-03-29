// <copyright file="CustomerShipmentEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.customershipment.overview;
using libs.workspace.angular.apps.src.lib.objects.shipment.list;

namespace Tests.CustomerShipmentTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Shipment")]
    public class CustomerShipmentEditTest : Test, IClassFixture<Fixture>
    {
        private readonly ShipmentListComponent shipmentListPage;
        private Organisation internalOrganisation;

        public CustomerShipmentEditTest(Fixture fixture)
            : base(fixture)
        {
            this.internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");

            this.Login();
            this.shipmentListPage = this.Sidenav.NavigateToShipments();
        }

        [Fact]
        public void Edit()
        {
            var before = new CustomerShipments(this.Transaction).Extent().ToArray();

            var expected = new CustomerShipmentBuilder(this.Transaction).WithDefaults(this.internalOrganisation).Build();

            this.Transaction.Derive();

            var expectedShipToPartyPartyName = expected.ShipToParty?.DisplayName();
            var expectedShipToAddressDisplayName = expected.ShipToAddress?.DisplayName();
            var expectedShipToContactPersonPartyName = expected.ShipToContactPerson?.DisplayName();
            var expectedShipFromAddressDisplayName = expected.ShipFromAddress?.DisplayName();
            var expectedShipFromFacilityName = expected.ShipFromFacility.Name;
            var expectedShipmentMethodName = expected.ShipmentMethod.Name;
            var expectedCarrierName = expected.Carrier.Name;
            var expectedEstimatedShipDate = expected.EstimatedShipDate.Value.Date;
            var expectedEstimatedArrivalDate = expected.EstimatedArrivalDate.Value.Date;
            var expectedHandlingInstruction = expected.HandlingInstruction;
            var expectedComment = expected.Comment;

            var shipment = before.First(v => ((Organisation)v.ShipFromParty).IsInternalOrganisation.Equals(true));
            var id = shipment.Id;

            this.shipmentListPage.Table.DefaultAction(shipment);
            var shipmentOverview = new CustomerShipmentOverviewComponent(this.shipmentListPage.Driver, this.M);
            var shipmentOverviewDetail = shipmentOverview.CustomershipmentOverviewDetail.Click();

            shipmentOverviewDetail
                .ShipToParty.Select(expected.ShipToParty?.DisplayName());

            this.Driver.WaitForAngular();

            shipmentOverviewDetail
                .ShipToAddress.Select(expected.ShipToAddress)
                .ShipFromAddress.Select(expected.ShipFromParty?.ShippingAddress)
                .ShipmentMethod.Select(expected.ShipmentMethod)
                .ShipFromFacility.Select(((Organisation)expected.ShipFromParty)?.FacilitiesWhereOwner?.FirstOrDefault())
                .Carrier.Select(expected.Carrier)
                .EstimatedShipDate.Set(expected.EstimatedShipDate.Value.Date)
                .EstimatedArrivalDate.Set(expected.EstimatedArrivalDate.Value.Date)
                .HandlingInstruction.Set(expected.HandlingInstruction)
                .Comment.Set(expected.Comment);

            if (expected.ExistShipToContactPerson)
            {
                shipmentOverviewDetail.ShipToContactPerson.Select(expected.ShipToContactPerson);
            }

            this.Transaction.Rollback();
            shipmentOverviewDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new CustomerShipments(this.Transaction).Extent().ToArray();
            shipment = (CustomerShipment) this.Transaction.Instantiate(id);

            Assert.Equal(after.Length, before.Length);

            Assert.Equal(expectedShipToPartyPartyName, shipment.ShipToParty?.DisplayName());
            Assert.Equal(expectedShipToAddressDisplayName, shipment.ShipToAddress?.DisplayName());
            Assert.Equal(expectedShipToContactPersonPartyName, shipment.ShipToContactPerson?.DisplayName());
            Assert.Equal(expectedShipFromAddressDisplayName, shipment.ShipFromAddress?.DisplayName());
            Assert.Equal(expectedShipFromFacilityName, shipment.ShipFromFacility.Name);
            Assert.Equal(expectedShipmentMethodName, shipment.ShipmentMethod.Name);
            Assert.Equal(expectedCarrierName, shipment.Carrier.Name);
            Assert.Equal(expectedEstimatedShipDate, shipment.EstimatedShipDate);
            Assert.Equal(expectedEstimatedArrivalDate, shipment.EstimatedArrivalDate);
            Assert.Equal(expectedHandlingInstruction, shipment.HandlingInstruction);
            Assert.Equal(expectedComment, shipment.Comment);
        }
    }
}
