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
    using libs.workspace.angular.apps.src.lib.objects.good.list;
    using libs.workspace.angular.apps.src.lib.objects.person.list;
    using libs.workspace.angular.apps.src.lib.objects.person.overview;
    using libs.workspace.angular.apps.src.lib.objects.unifiedgood.overview;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Product")]
    public class SupplierOfferingCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly GoodListComponent goodList;

        public SupplierOfferingCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.goodList = this.Sidenav.NavigateToGoods();
        }

        [Fact]
        public void Create()
        {
            var before = new SupplierOfferings(this.Transaction).Extent().ToArray();

            var good = new Goods(this.Transaction).Extent().First();
            var supplier = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA").ActiveSuppliers.First();
            var expected = new SupplierOfferingBuilder(this.Transaction).WithDefaults(supplier).Build();

            this.Transaction.Derive();

            var expectedSupplier = expected.Supplier;
            var expectedFromDate = expected.FromDate;
            var expectedUnitOfMeasure = expected.UnitOfMeasure;
            var expectedPrice = expected.Price;
            var expectedCurrency = expected.Currency;

            this.goodList.Table.DefaultAction(good);
            var unifiedGoodOverview = new UnifiedGoodOverviewComponent(this.goodList.Driver, this.M);
            var supplierOfferingDetail = unifiedGoodOverview.SupplierofferingOverviewPanel.Click().CreateSupplierOffering();

            supplierOfferingDetail
                .Supplier.Select(expected.Supplier.DisplayName())
                .FromDate.Set(expected.FromDate)
                .UnitOfMeasure.Select(expected.UnitOfMeasure)
                .Price.Set(expected.Price.ToString())
                .Currency.Select(expected.Currency);

            this.Transaction.Rollback();
            supplierOfferingDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SupplierOfferings(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var supplierOffering = after.Except(before).First();

            Assert.Equal(expectedSupplier, supplierOffering.Supplier);
            Assert.Equal(expectedFromDate.Date, supplierOffering.FromDate.Date);
            Assert.Equal(expectedUnitOfMeasure, supplierOffering.UnitOfMeasure);
            Assert.Equal(expectedPrice, supplierOffering.Price);
            Assert.Equal(expectedCurrency, supplierOffering.Currency);
        }
    }
}
