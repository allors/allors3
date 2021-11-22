// <copyright file="CustomerShipmentCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.customershipment.create;
using libs.workspace.angular.apps.src.lib.objects.shipment.list;

namespace Tests.CatalogueTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using libs.workspace.angular.apps.src.lib.objects.catalogue.list;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Product")]
    public class CatalogueCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly CataloguesListComponent catalogueListPage;

        public CatalogueCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.catalogueListPage = this.Sidenav.NavigateToCatalogues();
        }

        [Fact]
        public void Create()
        {
            var before = new Catalogues(this.Transaction).Extent().ToArray();

            var expected = new CatalogueBuilder(this.Transaction).WithDefaults().Build();

            this.Transaction.Derive();

            var expectedCatalogueName = expected.Name;
            var expectedCatalogueDescription = expected.Description;
            var expectedCatalogueScope = expected.CatScope;

            var catalogueListComponent = catalogueListPage.CreateCatalogue();

            catalogueListComponent
                .Name.Set(expected.Name)
                .Description.Set(expected.Description)
                .CatScope.Select(expected.CatScope);

            this.Transaction.Rollback();
            catalogueListComponent.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new Catalogues(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            Assert.Equal(expectedCatalogueName, actual.Name);
            Assert.Equal(expectedCatalogueDescription, actual.Description);
            Assert.Equal(expectedCatalogueScope, actual.CatScope);
        }
    }
}
