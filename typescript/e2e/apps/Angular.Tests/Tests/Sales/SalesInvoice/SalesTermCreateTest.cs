// <copyright file="EmailAddressEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;
using libs.workspace.angular.apps.src.lib.objects.emailaddress.edit;
using libs.workspace.angular.apps.src.lib.objects.salesinvoice.list;
using libs.workspace.angular.apps.src.lib.objects.person.overview;

namespace Tests.SalesInvoiceTests
{
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using libs.workspace.angular.apps.src.lib.objects.salesinvoice.overview;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class SalesTermCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly SalesInvoiceListComponent salesInvoiceListPage;

        public SalesTermCreateTest(Fixture fixture) : base(fixture)
        {
            this.Login();
            this.salesInvoiceListPage = this.Sidenav.NavigateToSalesInvoices();
        }

        [Fact]
        public void Create()
        {
            var before = new SalesTerms(this.Transaction).Extent().ToArray();

            var expectedSalesInvoice = new SalesInvoices(this.Transaction).Extent().FirstOrDefault();
            var expected = new OrderTermBuilder(this.Transaction).WithDefaults().Build();

            this.Transaction.Derive();

            var expectedTermType = expected.TermType;
            var expectedTermValue = expected.TermValue;

            this.salesInvoiceListPage.Table.DefaultAction(expectedSalesInvoice);
            var salesInvoiceDetails = new SalesInvoiceOverviewComponent(this.salesInvoiceListPage.Driver, this.M);
            var salesOrderTermDetail = salesInvoiceDetails.SalestermOverviewPanel.Click().CreateOrderTerm();

            salesOrderTermDetail
                .TermType.Select(expected.TermType)
                .TermValue.Set(expected.TermValue);

            this.Transaction.Rollback();
            salesOrderTermDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SalesTerms(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var salesInvoice = after.Except(before).First();

            Assert.Equal(expectedTermType, salesInvoice.TermType);
            Assert.Equal(expectedTermValue, salesInvoice.TermValue);
        }
    }
}
