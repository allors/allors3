// <copyright file="OrganisationFaceToFaceCommunicationCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.organisation.list;
using libs.workspace.angular.apps.src.lib.objects.organisation.overview;

namespace Tests.NonSerialisedInventoryItemTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using libs.workspace.angular.apps.src.lib.objects.good.list;
    using libs.workspace.angular.apps.src.lib.objects.nonserialisedinventoryitem.edit;
    using libs.workspace.angular.apps.src.lib.objects.unifiedgood.overview;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Product")]
    public class NonSerialsedInventoryItemEditTest : Test, IClassFixture<Fixture>
    {
        private readonly GoodListComponent goodListPage;

        public NonSerialsedInventoryItemEditTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.goodListPage = this.Sidenav.NavigateToGoods();
        }

        [Fact]
        public void Edit()
        {
            var before = new NonSerialisedInventoryItems(this.Transaction).Extent().ToArray();
            var first = new NonSerialisedInventoryItems(this.Transaction).Extent().First();
            var id = first.Id;

            var firstGood = new NonSerialisedInventoryItems(this.Transaction).Extent().Select(v => v.Part).OfType<UnifiedGood>().First();

            var internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var expected = new NonSerialisedInventoryItemBuilder(this.Transaction).WithDefaults(firstGood).Build();
            this.Transaction.Derive();

            var expectedPartLocation = expected.PartLocation;

            this.goodListPage.Table.DefaultAction(firstGood);
            var goodOverview = new UnifiedGoodOverviewComponent(this.goodListPage.Driver, this.M);
            var goodDetail = goodOverview.NonserialisedinventoryitemOverviewPanel.Click();

            var temp = firstGood.InventoryItemsWherePart.OfType<NonSerialisedInventoryItem>().First();

            goodDetail.Table.DefaultAction(temp);
            var editor = new NonSerialisedInventoryItemEditComponent(this.goodListPage.Driver, this.M);

            editor
                .PartLocation.Set(expected.PartLocation);

            this.Transaction.Rollback();
            editor.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new NonSerialisedInventoryItems(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length);

            var nonSerialisedInventoryItemAfter = after.First(v => v.Id == id);

            Assert.Equal(expectedPartLocation, nonSerialisedInventoryItemAfter.PartLocation);
        }
    }
}
