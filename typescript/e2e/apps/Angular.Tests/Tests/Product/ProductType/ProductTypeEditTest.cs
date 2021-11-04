
// <copyright file="PartyContactMechanismEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;

namespace Tests.PartyContactMachanismTests
{
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using libs.workspace.angular.apps.src.lib.objects.good.list;
    using libs.workspace.angular.apps.src.lib.objects.producttype.list;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class ProductTypeEditTest : Test, IClassFixture<Fixture>
    {
        private readonly ProductTypesOverviewComponent productTypes;

        public ProductTypeEditTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.productTypes = this.Sidenav.NavigateToProductTypes();
        }

        [Fact]
        public void Edit()
        {
            var before = new ProductTypes(this.Transaction).Extent().ToArray();

            var expected = new ProductTypeBuilder(this.Transaction)
                .WithName("test")
                .Build();

            this.Transaction.Derive();

            var expectedName = expected.Name;

            var productTypesDetail = this.productTypes.CreateProductType();

            productTypesDetail
                .Name.Set(expectedName);

            this.Transaction.Rollback();
            productTypesDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new ProductTypes(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var productType = after.Except(before).First();

            Assert.Equal(expectedName, productType.Name);
        }
    }
}
