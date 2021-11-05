// <copyright file="PartyContactMechanismEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;

namespace Tests.PurchaseInvoiceTests
{
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using libs.workspace.angular.apps.src.lib.objects.purchaseinvoice.list;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class PurchaseInvoiceCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly PurchaseInvoiceListComponent purchaseInvoices;

        public PurchaseInvoiceCreateTest(Fixture fixture)
            : base(fixture)
        {
            this.Login();
            this.purchaseInvoices = this.Sidenav.NavigateToPurchaseInvoices();
        }

        [Fact]
        public void Edit()
        {
            var before = new PurchaseInvoices(this.Transaction).Extent().ToArray();

            var organisation = new Organisations(this.Transaction).Extent().FirstOrDefault(x => x.ActiveSuppliers.Count() > 0);

            var expected = new PurchaseInvoiceBuilder(this.Transaction)
                .WithExternalB2BInvoiceDefaults(organisation)
                .Build();

            this.Transaction.Derive();

            var expectedPurchasedInvoiceType = expected.PurchaseInvoiceType;
            var expectedBilledFrom = expected.BilledFrom;
            var expectedInvoiceDate = expected.InvoiceDate;
            var expectedActualInvoiceAmount = expected.ActualInvoiceAmount;

            var productCategoryDetail = this.purchaseInvoices.CreatePurchaseInvoice();

            productCategoryDetail
                .PurchaseInvoiceType.Select(expectedPurchasedInvoiceType)
                .BilledFrom.Select(expectedBilledFrom.DisplayName())
                .InvoiceDate.Set(expectedInvoiceDate)
                .ActualInvoiceAmount.Set(expectedActualInvoiceAmount.ToString());

            this.Transaction.Rollback();
            productCategoryDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new PurchaseInvoices(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var purchaseInvoice = after.Except(before).First();

            Assert.Equal(expectedPurchasedInvoiceType, purchaseInvoice.PurchaseInvoiceType);
            Assert.Equal(expectedBilledFrom, purchaseInvoice.BilledFrom);
            Assert.Equal(expectedInvoiceDate.Date, purchaseInvoice.InvoiceDate.Date);
            Assert.Equal(expectedActualInvoiceAmount, purchaseInvoice.ActualInvoiceAmount);
        }
    }
}
