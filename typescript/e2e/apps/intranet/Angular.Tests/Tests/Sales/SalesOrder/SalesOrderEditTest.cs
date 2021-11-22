// <copyright file="SalesOrderEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.salesorder.list;
using libs.workspace.angular.apps.src.lib.objects.salesorder.overview;

namespace Tests.SalesOrderTests
{
    using System.Linq;
    using Allors;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Order")]
    public class SalesOrderEditTest : Test, IClassFixture<Fixture>
    {
        private readonly SalesOrderListComponent salesOrderListPage;
        private readonly Organisation internalOrganisation;

        public SalesOrderEditTest(Fixture fixture)
            : base(fixture)
        {
            this.internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");

            this.Login();
            this.salesOrderListPage = this.Sidenav.NavigateToSalesOrders();
        }

        /**
         * MinimalWithInternalOrganisation
         **/
        [Fact]
        public void EditWithInternalOrganisation()
        {
            var before = new SalesOrders(this.Transaction).Extent().ToArray();

            var expected = this.internalOrganisation.CreateInternalSalesOrder(this.Transaction.Faker());

            this.Transaction.Derive();

            var expectedBillToCustomer = expected.BillToCustomer?.DisplayName();
            var expectedBillToContactMechanism = expected.DerivedBillToContactMechanism;
            var expectedBillToContactPerson = expected.BillToContactPerson;
            var expectedShipToCustomer = expected.ShipToCustomer?.DisplayName();
            var expectedShipToAddressDisplayName = expected.DerivedShipToAddress;
            var expectedShipToContactPerson = expected.ShipToContactPerson;
            var expectedShipFromAddressDisplayName = expected.DerivedShipFromAddress?.DisplayName();
            var expectedCustomerReference = expected.CustomerReference;
            var expectedDescription = expected.Description;
            var expectedInternalComment = expected.InternalComment;

            var salesOrder = before.First();
            var id = salesOrder.Id;

            this.salesOrderListPage.Table.DefaultAction(salesOrder);
            var salesOrderOverview = new SalesOrderOverviewComponent(this.salesOrderListPage.Driver, this.M);
            var salesOrderOverviewDetail = salesOrderOverview.SalesorderOverviewDetail.Click();

            salesOrderOverviewDetail.BillToCustomer.Select(expected.BillToCustomer?.DisplayName());

            this.Driver.WaitForAngular();

            salesOrderOverviewDetail.DerivedBillToContactMechanism.Select(expected.DerivedBillToContactMechanism);
            salesOrderOverviewDetail.BillToContactPerson.Select(expected.BillToContactPerson);
            salesOrderOverviewDetail.DerivedShipFromAddress.Select(expected.DerivedShipFromAddress);
            salesOrderOverviewDetail.ShipToCustomer.Select(expected.ShipToCustomer?.DisplayName());

            this.Driver.WaitForAngular();

            salesOrderOverviewDetail.DerivedShipToAddress.Select(expected.DerivedShipToAddress);
            salesOrderOverviewDetail.ShipToContactPerson.Select(expected.ShipToContactPerson);
            salesOrderOverviewDetail.CustomerReference.Set(expected.CustomerReference);
            salesOrderOverviewDetail.Description.Set(expected.Description);
            salesOrderOverviewDetail.InternalComment.Set(expected.InternalComment);

            this.Transaction.Rollback();
            salesOrderOverviewDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SalesOrders(this.Transaction).Extent().ToArray();
            salesOrder = (SalesOrder)this.Transaction.Instantiate(id);

            Assert.Equal(after.Length, before.Length);

            Assert.Equal(expectedBillToCustomer, salesOrder.BillToCustomer?.DisplayName());
            Assert.Equal(expectedBillToContactMechanism, salesOrder.DerivedBillToContactMechanism);
            Assert.Equal(expectedBillToContactPerson, salesOrder.BillToContactPerson);
            Assert.Equal(expectedShipToAddressDisplayName, salesOrder.DerivedShipToAddress);
            Assert.Equal(expectedShipFromAddressDisplayName, salesOrder.DerivedShipFromAddress?.DisplayName());
            Assert.Equal(expectedShipToCustomer, salesOrder.ShipToCustomer?.DisplayName());
            Assert.Equal(expectedShipToContactPerson, salesOrder.ShipToContactPerson);
            Assert.Equal(expectedCustomerReference, salesOrder.CustomerReference);
            Assert.Equal(expectedDescription, salesOrder.Description);
            Assert.Equal(expectedInternalComment, salesOrder.InternalComment);
        }

        /**
         * MinimalWithExternalOrganisation
         **/
        [Fact]
        public void EditWithExternalOrganisation()
        {
            var before = new SalesOrders(this.Transaction).Extent().ToArray();

            var expected = this.internalOrganisation.CreateB2BSalesOrder(this.Transaction.Faker());

            this.Transaction.Derive();

            var expectedBillToCustomer = expected.BillToCustomer?.DisplayName();
            var expectedBillToContactMechanism = expected.DerivedBillToContactMechanism;
            var expectedBillToContactPerson = expected.BillToContactPerson;
            var expectedShipToCustomer = expected.ShipToCustomer?.DisplayName();
            var expectedShipToAddressDisplayName = expected.DerivedShipToAddress.DisplayName();
            var expectedShipToContactPerson = expected.ShipToContactPerson;
            var expectedShipFromAddressDisplayName = expected.DerivedShipFromAddress?.DisplayName();
            var expectedCustomerReference = expected.CustomerReference;
            var expectedDescription = expected.Description;
            var expectedInternalComment = expected.InternalComment;

            var salesOrder = before.First();
            var id = salesOrder.Id;

            this.salesOrderListPage.Table.DefaultAction(salesOrder);
            var salesOrderOverview = new SalesOrderOverviewComponent(this.salesOrderListPage.Driver, this.M);
            var salesOrderOverviewDetail = salesOrderOverview.SalesorderOverviewDetail.Click();

            salesOrderOverviewDetail.BillToCustomer.Select(expected.BillToCustomer?.DisplayName());

            this.Driver.WaitForAngular();

            salesOrderOverviewDetail.DerivedBillToContactMechanism.Select(expected.DerivedBillToContactMechanism);
            salesOrderOverviewDetail.BillToContactPerson.Select(expected.BillToContactPerson);
            salesOrderOverviewDetail.DerivedShipFromAddress.Select(expected.DerivedShipFromAddress);
            salesOrderOverviewDetail.ShipToCustomer.Select(expected.ShipToCustomer?.DisplayName());

            this.Driver.WaitForAngular();

            salesOrderOverviewDetail.DerivedShipToAddress.Select(expected.DerivedShipToAddress);
            salesOrderOverviewDetail.ShipToContactPerson.Select(expected.ShipToContactPerson);
            salesOrderOverviewDetail.CustomerReference.Set(expected.CustomerReference);
            salesOrderOverviewDetail.Description.Set(expected.Description);
            salesOrderOverviewDetail.InternalComment.Set(expected.InternalComment);

            this.Transaction.Rollback();
            salesOrderOverviewDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SalesOrders(this.Transaction).Extent().ToArray();
            salesOrder = (SalesOrder)this.Transaction.Instantiate(id);

            Assert.Equal(after.Length, before.Length);

            Assert.Equal(expectedBillToCustomer, salesOrder.BillToCustomer?.DisplayName());
            Assert.Equal(expectedBillToContactMechanism, salesOrder.DerivedBillToContactMechanism);
            Assert.Equal(expectedBillToContactPerson, salesOrder.BillToContactPerson);
            Assert.Equal(expectedShipToAddressDisplayName, salesOrder.DerivedShipToAddress.DisplayName());
            Assert.Equal(expectedShipFromAddressDisplayName, salesOrder.DerivedShipFromAddress?.DisplayName());
            Assert.Equal(expectedShipToCustomer, salesOrder.ShipToCustomer?.DisplayName());
            Assert.Equal(expectedShipToContactPerson, salesOrder.ShipToContactPerson);
            Assert.Equal(expectedCustomerReference, salesOrder.CustomerReference);
            Assert.Equal(expectedDescription, salesOrder.Description);
            Assert.Equal(expectedInternalComment, salesOrder.InternalComment);
        }

        /**
        * MinimalWithInternalPerson
        **/
        [Fact]
        public void EditWithExternalPerson()
        {
            var before = new SalesOrders(this.Transaction).Extent().ToArray();

            var expected = this.internalOrganisation.CreateB2CSalesOrder(this.Transaction.Faker());

            this.Transaction.Derive();

            var expectedBillToCustomer = expected.BillToCustomer?.DisplayName();
            var expectedBillToContactMechanism = expected.DerivedBillToContactMechanism;
            var expectedShipToCustomer = expected.ShipToCustomer?.DisplayName();
            var expectedShipToAddressDisplayName = expected.DerivedShipToAddress.DisplayName();
            var expectedShipFromAddressDisplayName = expected.DerivedShipFromAddress?.DisplayName();
            var expectedCustomerReference = expected.CustomerReference;
            var expectedDescription = expected.Description;
            var expectedInternalComment = expected.InternalComment;

            var salesOrder = before.First();
            var id = salesOrder.Id;

            this.salesOrderListPage.Table.DefaultAction(salesOrder);
            var salesOrderOverview = new SalesOrderOverviewComponent(this.salesOrderListPage.Driver, this.M);
            var salesOrderOverviewDetail = salesOrderOverview.SalesorderOverviewDetail.Click();

            salesOrderOverviewDetail.BillToCustomer.Select(expected.BillToCustomer?.DisplayName());

            this.Driver.WaitForAngular();

            salesOrderOverviewDetail.DerivedBillToContactMechanism.Select(expected.DerivedBillToContactMechanism);
            salesOrderOverviewDetail.ShipToCustomer.Select(expected.ShipToCustomer?.DisplayName());

            this.Driver.WaitForAngular();

            salesOrderOverviewDetail.DerivedShipToAddress.Select(expected.DerivedShipToAddress);
            salesOrderOverviewDetail.DerivedShipFromAddress.Select(expected.DerivedShipFromAddress);
            salesOrderOverviewDetail.CustomerReference.Set(expected.CustomerReference);
            salesOrderOverviewDetail.Description.Set(expected.Description);
            salesOrderOverviewDetail.InternalComment.Set(expected.InternalComment);

            this.Transaction.Rollback();
            salesOrderOverviewDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new SalesOrders(this.Transaction).Extent().ToArray();
            salesOrder = (SalesOrder)this.Transaction.Instantiate(id);

            Assert.Equal(after.Length, before.Length);

            Assert.Equal(expectedBillToCustomer, salesOrder.BillToCustomer?.DisplayName());
            Assert.Equal(expectedBillToContactMechanism, salesOrder.DerivedBillToContactMechanism);
            Assert.Equal(expectedShipToCustomer, salesOrder.ShipToCustomer?.DisplayName());
            Assert.Equal(expectedShipToAddressDisplayName, salesOrder.DerivedShipToAddress.DisplayName());
            Assert.Equal(expectedShipFromAddressDisplayName, salesOrder.DerivedShipFromAddress?.DisplayName());
            Assert.Equal(expectedCustomerReference, salesOrder.CustomerReference);
            Assert.Equal(expectedDescription, salesOrder.Description);
            Assert.Equal(expectedInternalComment, salesOrder.InternalComment);
        }
    }
}
