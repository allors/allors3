// <copyright file="NonUnifiedGoodCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.good.list;

namespace Tests.NonUnifiedGoodTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Product")]
    public class NonUnifiedGoodCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly GoodListComponent goods;

        public NonUnifiedGoodCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.goods = this.Sidenav.NavigateToGoods();
        }

        [Fact]
        public void Create()
        {
            var before = new NonUnifiedGoods(this.Transaction).Extent().ToArray();

            var internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var expected = new NonUnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(internalOrganisation).Build();

            var expectedPart = new NonUnifiedParts(this.Transaction).Extent().FirstOrDefault();

            this.Transaction.Derive();

            var expectedName = expected.Name;
            var expectedDescription = expected.Description;
            var expectedPartName = expectedPart.Name;

            var nonUnifiedGoodCreate = this.goods.CreateNonUnifiedGood();

            nonUnifiedGoodCreate
                .Name.Set(expected.Name)
                .Description.Set(expected.Description)
                .Part.Select(expectedPart.Name);

            this.Transaction.Rollback();
            nonUnifiedGoodCreate.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new NonUnifiedGoods(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var good = after.Except(before).First();

            Assert.Equal(expectedName, good.Name);
            Assert.Equal(expectedDescription, good.Description);
            Assert.Equal(expectedPartName, good.Part.Name);
        }
    }
}
