// <copyright file="NonUnifiedGoodEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.good.list;
using libs.workspace.angular.apps.src.lib.objects.nonunifiedgood.overview;

namespace Tests.NonUnifiedGood
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Product")]
    public class NonUnifiedGoodEditTest : Test, IClassFixture<Fixture>
    {
        private readonly GoodListComponent goods;

        public NonUnifiedGoodEditTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.goods = this.Sidenav.NavigateToGoods();
        }

        [Fact]
        public void Edit()
        {
            var before = new NonUnifiedGoods(this.Session).Extent().ToArray();

            var internalOrganisation = new OrganisationBuilder(this.Session).WithInternalOrganisationDefaults().Build();

            var expected = new NonUnifiedGoodBuilder(this.Session).WithNonSerialisedDefaults(internalOrganisation).Build();

            this.Session.Derive();

            var expectedName = expected.Name;
            var expectedDescription = expected.Description;

            var nonUnifiedGood = before.First();
            var id = nonUnifiedGood.Id;

            this.goods.Table.DefaultAction(nonUnifiedGood);
            var goodDetails = new NonUnifiedGoodOverviewComponent(this.goods.Driver, this.M);
            var goodOverviewDetail = goodDetails.NonunifiedgoodOverviewDetail.Click();

            goodOverviewDetail
                .Name.Set(expected.Name)
                .Description.Set(expected.Description);

            this.Session.Rollback();
            goodOverviewDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Session.Rollback();

            var after = new NonUnifiedGoods(this.Session).Extent().ToArray();

            var good = after.First(v => v.Id.Equals(id));

            Assert.Equal(after.Length, before.Length);
            Assert.Equal(expectedName, good.Name);
            Assert.Equal(expectedDescription, good.Description);
        }
    }
}
