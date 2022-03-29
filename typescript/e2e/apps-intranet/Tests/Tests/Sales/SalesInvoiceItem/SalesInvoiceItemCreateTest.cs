// <copyright file="EmailAddressEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;
using libs.workspace.angular.apps.src.lib.objects.emailaddress.edit;
using libs.workspace.angular.apps.src.lib.objects.salesinvoice.list;
using libs.workspace.angular.apps.src.lib.objects.salesinvoice.overview;

namespace Tests.SalesInvoiceItemTests
{
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class SalesInvoiceItemCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly SalesInvoiceListComponent salesInvoiceListPage;

        public SalesInvoiceItemCreateTest(Fixture fixture) : base(fixture)
        {
            this.Login();
            this.salesInvoiceListPage = this.Sidenav.NavigateToSalesInvoices();
        }

        [Fact]
        public void CreateWithInvoiceItemTypeProduct()
        {
            var before = new SalesInvoiceItems(this.Transaction).Extent().ToArray();

            var internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var expectedSalesInvoice = new SalesInvoices(this.Transaction).Extent().FirstOrDefault();

            var unifiedGood = internalOrganisation.ProductCategoriesWhereInternalOrganisation.SelectMany(v => v.AllProducts).OfType<UnifiedGood>().FirstOrDefault(v => v.ExistSerialisedItems);

            var expected = new SalesInvoiceItemBuilder(this.Transaction)
                .WithProductItemDefaults(unifiedGood)
                .Build();

            this.Transaction.Derive();

            var expectedInvoiceItemType = expected.InvoiceItemType;
            var expectedAsignedUnitPrice = expected.AssignedUnitPrice;
            var expectedComment = expected.Comment;
            var expectedQuantity = expected.Quantity;

            this.salesInvoiceListPage.Table.DefaultAction(expectedSalesInvoice);
            var salesInvoiceDetails = new SalesInvoiceOverviewComponent(this.salesInvoiceListPage.Driver, this.M);
            var salesInvoiceItemDetail = salesInvoiceDetails.SalesinvoiceitemOverviewPanel.Click().CreateSalesInvoiceItem();

            salesInvoiceItemDetail
                .SalesInvoiceItemInvoiceItemType_1.Select(expected.InvoiceItemType)
                .Product.Select(expected.Product.Name)
                .Quantity.Set(expected.Quantity.ToString())
                .AssignedUnitPrice.Set(expected.AssignedUnitPrice.ToString())
                .Comment.Set(expected.Comment);

            this.Transaction.Rollback();
            salesInvoiceItemDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SalesInvoiceItems(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var requestItem = after.Except(before).First();

            Assert.Equal(expectedInvoiceItemType, requestItem.InvoiceItemType);
            Assert.Equal(expectedAsignedUnitPrice, requestItem.AssignedUnitPrice);
            Assert.Equal(expectedComment, requestItem.Comment);
            Assert.Equal(expectedQuantity, requestItem.Quantity);
        }
    }
}
