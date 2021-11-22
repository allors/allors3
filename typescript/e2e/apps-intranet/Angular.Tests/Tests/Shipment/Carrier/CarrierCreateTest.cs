// <copyright file="CustomerShipmentCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.customershipment.create;
using libs.workspace.angular.apps.src.lib.objects.shipment.list;

namespace Tests.CarrierTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using libs.workspace.angular.apps.src.lib.objects.carrier.list;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Shipment")]
    public class CarrierCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly CarrierListComponent carrierListPage;
        private Organisation internalOrganisation;

        public CarrierCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");

            this.Login();
            this.carrierListPage = this.Sidenav.NavigateToCarriers();
        }

        [Fact]
        public void Create()
        {
            var before = new Carriers(this.Transaction).Extent().ToArray();

            var expected = new CarrierBuilder(this.Transaction).WithName("Bpost").Build();

            this.Transaction.Derive();

            var expectedCarrierName = expected.Name;

            var carrierListComponent = carrierListPage.CreateCarrier();

            carrierListComponent.Name.Set(expected.Name);

            this.Transaction.Rollback();
            carrierListComponent.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Carriers(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedCarrierName, actual.Name);
        }
    }
}
