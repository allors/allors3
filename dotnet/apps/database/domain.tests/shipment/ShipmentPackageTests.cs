// <copyright file="ShipmentPackageTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class ShipmentPackageTests : DomainTest, IClassFixture<Fixture>
    {
        public ShipmentPackageTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenShipmentPackageBuilder_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            var package = new ShipmentPackageBuilder(this.Transaction).Build();

            Assert.True(package.ExistCreationDate);
        }

        [Fact]
        public void GivenShipmentPackage_WhenCreated_ThenSequenceNumberIsDerived()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            var package = new ShipmentPackageBuilder(this.Transaction).Build();
            shipment.AddShipmentPackage(package);
            this.Derive();

            Assert.Equal(1, package.SequenceNumber);

            var secondPackage = new ShipmentPackageBuilder(this.Transaction).Build();
            shipment.AddShipmentPackage(secondPackage);
            this.Derive();

            Assert.Equal(2, secondPackage.SequenceNumber);
        }
    }

    public class ShipmentPackageRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public ShipmentPackageRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSequenceNumberDeriveDocuments()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            var package = new ShipmentPackageBuilder(this.Transaction).Build();
            shipment.AddShipmentPackage(package);
            this.Derive();

            Assert.True(package.ExistDocuments);
        }

        [Fact]
        public void ChangedItemIssuanceShipmentItemDeriveDocuments()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateShipmentPackage = true;

            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            var pickList = new PickListBuilder(this.Transaction)
                .WithPickListState(new PickListStates(this.Transaction).Picked)
                .Build();
            this.Derive();

            var pickListItem = new PickListItemBuilder(this.Transaction).Build();
            pickList.AddPickListItem(pickListItem);
            this.Derive();

            var package = new ShipmentPackageBuilder(this.Transaction).Build();
            shipment.AddShipmentPackage(package);
            this.Derive();

            new ItemIssuanceBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithPickListItem(pickListItem).Build();
            this.Derive();

            Assert.True(package.ExistPackagingContents);
        }

        [Fact]
        public void ChangedPickListPickListStateDeriveDocuments()
        {
            this.InternalOrganisation.StoresWhereInternalOrganisation.First().AutoGenerateShipmentPackage = true;

            var shipment = new CustomerShipmentBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            shipment.AddShipmentItem(shipmentItem);
            this.Derive();

            var pickList = new PickListBuilder(this.Transaction).Build();
            this.Derive();

            var pickListItem = new PickListItemBuilder(this.Transaction).Build();
            pickList.AddPickListItem(pickListItem);
            this.Derive();

            var package = new ShipmentPackageBuilder(this.Transaction).Build();
            shipment.AddShipmentPackage(package);
            this.Derive();

            new ItemIssuanceBuilder(this.Transaction).WithShipmentItem(shipmentItem).WithPickListItem(pickListItem).Build();
            this.Derive();

            pickList.PickListState = new PickListStates(this.Transaction).Picked;
            this.Derive();

            Assert.True(package.ExistPackagingContents);
        }
    }
}
