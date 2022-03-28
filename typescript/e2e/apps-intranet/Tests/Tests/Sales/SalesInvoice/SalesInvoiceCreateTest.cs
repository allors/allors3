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

    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Relation")]
    public class SalesInvoiceCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly SalesInvoiceListComponent salesInvoiceListPage;

        public SalesInvoiceCreateTest(Fixture fixture) : base(fixture)
        {
            this.Login();
            this.salesInvoiceListPage = this.Sidenav.NavigateToSalesInvoices();
        }

        [Fact]
        public void Create()
        {
            var before = new SalesInvoices(this.Transaction).Extent().ToArray();

            var internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");
            var expected = new SalesInvoiceBuilder(this.Transaction).WithDefaults(internalOrganisation).Build();

            this.Transaction.Derive();

            var expectedSalesInvoiceType = expected.SalesInvoiceType;
            var expectedBillToCustomer = expected.BillToCustomer;
            var expectedDerivedBillToContactMechanism = expected.AssignedBillToContactMechanism;
            var expectedAdvancePayment = expected.AdvancePayment;

            var salesInvoiceCreate = this.salesInvoiceListPage.CreateSalesInvoice();

            salesInvoiceCreate
                .SalesInvoiceType.Select(expected.SalesInvoiceType)
                .BillToCustomer.Select(expected.BillToCustomer.DisplayName())
                .AdvancePayment.Set(expected.AdvancePayment.ToString())
                .DerivedBillToContactMechanism.Select(expectedDerivedBillToContactMechanism);

            this.Transaction.Rollback();
            salesInvoiceCreate.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SalesInvoices(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var salesInvoice = after.Except(before).First();

            Assert.Equal(expectedSalesInvoiceType, salesInvoice.SalesInvoiceType);
            Assert.Equal(expectedBillToCustomer, salesInvoice.BillToCustomer);
            Assert.Equal(expectedDerivedBillToContactMechanism, salesInvoice.DerivedBillToContactMechanism);
            Assert.Equal(expectedAdvancePayment, salesInvoice.AdvancePayment);
        }
    }
}
