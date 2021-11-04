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
    using libs.workspace.angular.apps.src.lib.objects.organisation.overview;
    using libs.workspace.angular.apps.src.lib.objects.productcategory.list;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class ProductCategoryEditTest : Test, IClassFixture<Fixture>
    {
        private readonly ProductCategoryListComponent productCategories;

        public ProductCategoryEditTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.productCategories = this.Sidenav.NavigateToProductCategories();
        }

        [Fact]
        public void Edit()
        {
            var before = new ProductCategories(this.Transaction).Extent().ToArray();

            var expected = new ProductCategoryBuilder(this.Transaction)
                .WithName("test")
                .WithCatScope(new Scopes(this.Transaction).Public)
                .Build();

            this.Transaction.Derive();

            var expectedName = expected.Name;
            var expectedCatScope = expected.CatScope;

            //this.organisation.Table.DefaultAction(organisation);

            var productCategoryDetail = this.productCategories.CreateProductCategory();

            productCategoryDetail
                .Name.Set(expectedName)
                .CatScope.Select(expectedCatScope);

            this.Transaction.Rollback();
            productCategoryDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new ProductCategories(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var productCategory = after.Except(before).First();

            Assert.Equal(expectedName, productCategory.Name);
            Assert.Equal(expectedCatScope, productCategory.CatScope);
        }
    }
}
