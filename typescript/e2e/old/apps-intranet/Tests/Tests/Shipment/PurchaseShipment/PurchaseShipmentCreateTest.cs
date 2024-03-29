// <copyright file="PurchaseShipmentCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.purchaseshipment.create;
using libs.workspace.angular.apps.src.lib.objects.shipment.list;

namespace Tests.PurchaseShipmentTests
{
    using System.Linq;
    using Allors;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Shipment")]
    public class PurchaseShipmentCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly ShipmentListComponent shipmentListPage;
        private Organisation internalOrganisation;

        public PurchaseShipmentCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");

            for (int i = 0; i < 10; i++)
            {
                this.internalOrganisation.CreateSupplier(this.Transaction.Faker());
            }

            this.Login();
            this.shipmentListPage = this.Sidenav.NavigateToShipments();
        }

        [Fact]
        public void CreateFull()
        {
            var before = new PurchaseShipments(this.Transaction).Extent().ToArray();

            var expected = new PurchaseShipmentBuilder(this.Transaction).WithDefaults(this.internalOrganisation).Build();

            this.Transaction.Derive();

            var expectedShipToPartyPartyName = expected.ShipToParty.DisplayName();
            var expectedShipToAddressDisplayName = expected.ShipToAddress.DisplayName();
            var expectedShipToContactPersonPartyName = expected.ShipToContactPerson.DisplayName();
            var expectedShipFromPartyPartyName = expected.ShipFromParty.DisplayName();
            var expectedShipFromContactPersonPartyName = expected.ShipFromContactPerson.DisplayName();

            var purchaseShipmentCreate = this.shipmentListPage
                .CreatePurchaseShipment()
                .Build(expected);

            purchaseShipmentCreate.AssertFull(expected);

            this.Transaction.Rollback();
            purchaseShipmentCreate.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new PurchaseShipments(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedShipFromPartyPartyName, actual.ShipFromParty.DisplayName());
            Assert.Equal(expectedShipFromContactPersonPartyName, actual.ShipFromContactPerson.DisplayName());
            Assert.Equal(expectedShipToPartyPartyName, actual.ShipToParty.DisplayName());
            Assert.Equal(expectedShipToAddressDisplayName, actual.ShipToAddress.DisplayName());
            Assert.Equal(expectedShipToContactPersonPartyName, actual.ShipToContactPerson.DisplayName());
        }

        [Fact]
        public void CreateMinimal()
        {
            var before = new PurchaseShipments(this.Transaction).Extent().ToArray();

            var expected = new PurchaseShipmentBuilder(this.Transaction).WithDefaults(this.internalOrganisation).Build();

            this.Transaction.Derive();

            var expectedShipToPartyPartyName = expected.ShipToParty.DisplayName();
            var expectedShipFromPartyPartyName = expected.ShipFromParty.DisplayName();

            var purchaseShipmentCreate = this.shipmentListPage
                .CreatePurchaseShipment()
                .Build(expected, true);

            purchaseShipmentCreate.AssertFull(expected);

            this.Transaction.Rollback();
            purchaseShipmentCreate.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new PurchaseShipments(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedShipFromPartyPartyName, actual.ShipFromParty.DisplayName());
            Assert.Equal(expectedShipToPartyPartyName, actual.ShipToParty.DisplayName());
        }
    }
}
