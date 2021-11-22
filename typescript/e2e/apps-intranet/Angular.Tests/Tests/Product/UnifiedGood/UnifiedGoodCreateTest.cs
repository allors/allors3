// <copyright file="NonUnifiedGoodCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.good.list;

namespace Tests.UnifiedGoodTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Product")]
    public class UnifiedGoodCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly GoodListComponent goods;

        public UnifiedGoodCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.goods = this.Sidenav.NavigateToGoods();
        }

        [Fact]
        public void Create()
        {
            var before = new UnifiedGoods(this.Transaction).Extent().ToArray();

            var internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var expected = new UnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(internalOrganisation).Build();

            this.Transaction.Derive();

            var expectedName = expected.Name;
            var expectedInventoryItemKind = expected.InventoryItemKind;

            var nonUnifiedGoodCreate = this.goods.CreateUnifiedGood();

            nonUnifiedGoodCreate
                .Name.Set(expected.Name)
                .InventoryItemKind.Select(expected.InventoryItemKind);

            this.Transaction.Rollback();
            nonUnifiedGoodCreate.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new UnifiedGoods(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var good = after.Except(before).First();

            Assert.Equal(expectedName, good.Name);
            Assert.Equal(expectedInventoryItemKind, good.InventoryItemKind);
        }
    }
}
