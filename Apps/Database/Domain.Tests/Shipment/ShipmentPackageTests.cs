// <copyright file="ShipmentPackageTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class ShipmentPackageTests : DomainTest, IClassFixture<Fixture>
    {
        public ShipmentPackageTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenShipmentPackageBuilder_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            var package = new ShipmentPackageBuilder(this.Session).Build();

            Assert.True(package.ExistCreationDate);
        }

        [Fact]
        public void GivenShipmentPackage_WhenCreated_ThenSequenceNumberIsDerived()
        {
            var shipment = new CustomerShipmentBuilder(this.Session).Build();
            var package = new ShipmentPackageBuilder(this.Session).Build();
            shipment.AddShipmentPackage(package);
            this.Session.Derive(false);

            Assert.Equal(1, package.SequenceNumber);

            var secondPackage = new ShipmentPackageBuilder(this.Session).Build();
            shipment.AddShipmentPackage(secondPackage);
            this.Session.Derive(false);

            Assert.Equal(2, secondPackage.SequenceNumber);
        }
    }

    public class ShipmentPackageDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public ShipmentPackageDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSequenceNumberDeriveDocuments()
        {
            var shipment = new CustomerShipmentBuilder(this.Session).Build();
            var package = new ShipmentPackageBuilder(this.Session).Build();
            shipment.AddShipmentPackage(package);
            this.Session.Derive(false);

            Assert.True(package.ExistDocuments);
        }

        [Fact]
        public void ChangedItemIssuanceShipmentItemDeriveDocuments()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.AutoGenerateShipmentPackage = true;

            var shipment = new CustomerShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            var pickList = new PickListBuilder(this.Session)
                .WithPickListState(new PickListStates(this.Session).Picked)
                .Build();
            this.Session.Derive(false);

            var pickListItem = new PickListItemBuilder(this.Session).Build();
            pickList.AddPickListItem(pickListItem);
            this.Session.Derive(false);

            var package = new ShipmentPackageBuilder(this.Session).Build();
            shipment.AddShipmentPackage(package);
            this.Session.Derive(false);

            new ItemIssuanceBuilder(this.Session).WithShipmentItem(shipmentItem).WithPickListItem(pickListItem).Build();
            this.Session.Derive(false);

            Assert.True(package.ExistPackagingContents);
        }

        [Fact]
        public void ChangedPickListPickListStateDeriveDocuments()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First.AutoGenerateShipmentPackage = true;

            var shipment = new CustomerShipmentBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Session.Derive(false);

            var pickList = new PickListBuilder(this.Session).Build();
            this.Session.Derive(false);

            var pickListItem = new PickListItemBuilder(this.Session).Build();
            pickList.AddPickListItem(pickListItem);
            this.Session.Derive(false);

            var package = new ShipmentPackageBuilder(this.Session).Build();
            shipment.AddShipmentPackage(package);
            this.Session.Derive(false);

            new ItemIssuanceBuilder(this.Session).WithShipmentItem(shipmentItem).WithPickListItem(pickListItem).Build();
            this.Session.Derive(false);

            pickList.PickListState = new PickListStates(this.Session).Picked;
            this.Session.Derive(false);

            Assert.True(package.ExistPackagingContents);
        }
    }
}
