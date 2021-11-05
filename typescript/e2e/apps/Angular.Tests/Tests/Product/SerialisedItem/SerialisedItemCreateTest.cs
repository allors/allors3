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
    using libs.workspace.angular.apps.src.lib.objects.person.list;
    using libs.workspace.angular.apps.src.lib.objects.person.overview;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Product")]
    public class SerialisedItemCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly PersonListComponent people;

        public SerialisedItemCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.people = this.Sidenav.NavigateToPeople();
        }

        [Fact]
        public void Create()
        {
            var before = new SerialisedItems(this.Transaction).Extent().ToArray();

            var internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var expected = new SerialisedItemBuilder(this.Transaction).WithDefaults(internalOrganisation).Build();

            var serialised = new InventoryItemKinds(this.Transaction).Serialised;
            var part = new UnifiedGoods(this.Transaction).Extent().First(v => serialised.Equals(v.InventoryItemKind));

            var person = new People(this.Transaction).Extent().First();

            this.Transaction.Derive();

            var expectedName = expected.Name;
            var expectedSerialNumber = expected.SerialNumber;

            this.people.Table.DefaultAction(person);
            var personOverview = new PersonOverviewComponent(this.people.Driver, this.M);
            var serialisedItemDetail = personOverview.SerialiseditemOverviewPanel.Click().CreateSerialisedItem();

            serialisedItemDetail
                .PartWhereItem.Select(part.Name)
                .Name.Set(expected.Name)
                .SerialNumber.Set(expected.SerialNumber);

            this.Transaction.Rollback();
            serialisedItemDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SerialisedItems(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var serialisedItem = after.Except(before).First();

            Assert.Equal(expectedName, serialisedItem.Name);
            Assert.Equal(expectedSerialNumber, serialisedItem.SerialNumber);
        }
    }
}
