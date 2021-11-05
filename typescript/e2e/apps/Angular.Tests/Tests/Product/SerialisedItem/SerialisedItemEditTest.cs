// <copyright file="CustomerShipmentCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.customershipment.create;
using libs.workspace.angular.apps.src.lib.objects.shipment.list;

namespace Tests.SerialisedItemTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using libs.workspace.angular.apps.src.lib.objects.serialiseditem.list;
    using libs.workspace.angular.apps.src.lib.objects.serialiseditem.overview;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Product")]
    public class SerialisedItemEditTest : Test, IClassFixture<Fixture>
    {
        private readonly SerialisedItemListComponent serialisedItemComponent;

        public SerialisedItemEditTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.serialisedItemComponent = this.Sidenav.NavigateToSerialisedAssets();
        }

        [Fact]
        public void Edit()
        {
            var before = new SerialisedItems(this.Transaction).Extent().ToArray();
            var first = new SerialisedItems(this.Transaction).Extent().First();
            var id = first.Id;

            var internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var expected = new SerialisedItemBuilder(this.Transaction).WithDefaults(internalOrganisation).Build();
            this.Transaction.Derive();

            var expectedName = expected.Name;
            var expectedSerialNumber = expected.SerialNumber;

            this.serialisedItemComponent.Table.DefaultAction(first);
            var personOverview = new SerialisedItemOverviewComponent(this.serialisedItemComponent.Driver, this.M);
            var serialisedItemDetail = personOverview.SerialiseditemOverviewDetail.Click();

            serialisedItemDetail
                .Name.Set(expected.Name)
                .SerialNumber.Set(expected.SerialNumber);

            this.Transaction.Rollback();
            serialisedItemDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SerialisedItems(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length);

            var serialisedItem = after.First(v => v.Id == id);

            Assert.Equal(expectedName, serialisedItem.Name);
            Assert.Equal(expectedSerialNumber, serialisedItem.SerialNumber);
        }
    }
}
