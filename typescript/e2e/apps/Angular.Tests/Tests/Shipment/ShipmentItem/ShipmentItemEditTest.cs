// <copyright file="ShipmentItemCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.customershipment.overview;
using libs.workspace.angular.apps.src.lib.objects.shipment.list;

namespace Tests.ShipmentItemTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Shipment")]
    public class ShipmentItemEditTest : Test, IClassFixture<Fixture>
    {
        private readonly ShipmentListComponent shipmentListPage;
        private CustomerShipment customerShipment;
        private Organisation internalOrganisation;

        public ShipmentItemEditTest(Fixture fixture)
            : base(fixture)
        {
            this.internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");

            var customerShipments = new CustomerShipments(this.Transaction).Extent();
            customerShipments.Filter.AddEquals(M.CustomerShipment.ShipFromParty, internalOrganisation);
            this.customerShipment = customerShipments.FirstOrDefault();

            this.Login();
            this.shipmentListPage = this.Sidenav.NavigateToShipments();
        }

        [Fact]
        public void CreateCustomerShipmentItemForSerialisedUnifiedGood()
        {
            var before = customerShipment.ShipmentItems.ToArray();

            var goods = new UnifiedGoods(this.Transaction).Extent();
            goods.Filter.AddEquals(M.UnifiedGood.InventoryItemKind, new InventoryItemKinds(this.Transaction).Serialised);
            var serializedGood = goods.FirstOrDefault();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithForSaleDefaults(this.internalOrganisation).Build();
            serializedGood.AddSerialisedItem(serialisedItem);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.shipmentListPage.Table.DefaultAction(customerShipment);
            var shipmentOverview = new CustomerShipmentOverviewComponent(this.shipmentListPage.Driver, this.M);
            var shipmentItemOverview = shipmentOverview.ShipmentitemOverviewPanel.Click();

            var shipmentItemCreate = shipmentItemOverview.CreateShipmentItem();
            shipmentItemCreate
                .Good.Select(serializedGood.Name)
                .ShipmentItemSerialisedItem_1.Select(serialisedItem)
                .NextSerialisedItemAvailability.Select(new SerialisedItemAvailabilities(this.Transaction).Sold)
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = customerShipment.ShipmentItems.ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(serializedGood.Name, actual.Good.Name);
            Assert.Equal(serialisedItem.Name, actual.SerialisedItem.Name);
            Assert.Equal(1, actual.Quantity);
        }

        [Fact]
        public void CreateCustomerShipmentItemForNonSerialisedUnifiedGood()
        {
            var before = customerShipment.ShipmentItems.ToArray();

            var goods = new UnifiedGoods(this.Transaction).Extent();
            goods.Filter.AddEquals(M.UnifiedGood.InventoryItemKind, new InventoryItemKinds(this.Transaction).NonSerialised);
            var nonSerializedGood = goods.FirstOrDefault();

            this.shipmentListPage.Table.DefaultAction(customerShipment);
            var shipmentOverview = new CustomerShipmentOverviewComponent(this.shipmentListPage.Driver, this.M);
            var shipmentItemOverview = shipmentOverview.ShipmentitemOverviewPanel.Click();

            var shipmentItemCreate = shipmentItemOverview.CreateShipmentItem();
            shipmentItemCreate
                .Good.Select(nonSerializedGood.Name)
                .Quantity.Set("5")
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = customerShipment.ShipmentItems.ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(nonSerializedGood.Name, actual.Good.Name);
            Assert.Equal(5, actual.Quantity);
        }

        [Fact]
        public void EditCustomerShipmentItemForSerialisedUnifiedGood()
        {
            var before = customerShipment.ShipmentItems.ToArray();

            var goods = new UnifiedGoods(this.Transaction).Extent();
            goods.Filter.AddEquals(M.UnifiedGood.InventoryItemKind, new InventoryItemKinds(this.Transaction).Serialised);
            var serializedGood = goods.FirstOrDefault();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithForSaleDefaults(this.internalOrganisation).Build();
            serializedGood.AddSerialisedItem(serialisedItem);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.shipmentListPage.Table.DefaultAction(customerShipment);
            var shipmentOverview = new CustomerShipmentOverviewComponent(this.shipmentListPage.Driver, this.M);
            var shipmentItemOverview = shipmentOverview.ShipmentitemOverviewPanel.Click();

            var shipmentItemCreate = shipmentItemOverview.CreateShipmentItem();
            shipmentItemCreate
                .Good.Select(serializedGood.Name)
                .ShipmentItemSerialisedItem_1.Select(serialisedItem)
                .NextSerialisedItemAvailability.Select(new SerialisedItemAvailabilities(this.Transaction).Sold)
                .SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = customerShipment.ShipmentItems.ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(serializedGood.Name, actual.Good.Name);
            Assert.Equal(serialisedItem.Name, actual.SerialisedItem.Name);
            Assert.Equal(1, actual.Quantity);
        }
    }
}
