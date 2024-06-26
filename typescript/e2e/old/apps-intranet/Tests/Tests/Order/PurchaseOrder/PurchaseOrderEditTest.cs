// <copyright file="PurchaseOrderEditTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.purchaseorder.list;
using libs.workspace.angular.apps.src.lib.objects.purchaseorder.overview;

namespace Tests.PurchaseOrderTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Order")]
    public class PurchaseOrderEditTest : Test, IClassFixture<Fixture>
    {
        private readonly PurchaseOrderListComponent purchaseOrderListPage;
        private readonly Organisation internalOrganisation;

        public PurchaseOrderEditTest(Fixture fixture)
            : base(fixture)
        {
            this.internalOrganisation = new Organisations(this.Transaction).FindBy(M.Organisation.Name, "Allors BVBA");

            this.Login();
            this.purchaseOrderListPage = this.Sidenav.NavigateToPurchaseOrders();
        }

        /**
         * MinimalWithDefaults
         **/
        [Fact]
        public void EditWithDefaults()
        {
            var before = new PurchaseOrders(this.Transaction).Extent().ToArray();

            var expected = new PurchaseOrderBuilder(this.Transaction).WithDefaults(this.internalOrganisation).Build();

            Assert.True(expected.ExistTakenViaSupplier);
            Assert.True(expected.ExistTakenViaContactPerson);
            Assert.True(expected.ExistBillToContactPerson);
            Assert.True(expected.ExistShipToContactPerson);
            Assert.True(expected.ExistStoredInFacility);
            Assert.True(expected.ExistCustomerReference);
            Assert.True(expected.ExistDescription);
            Assert.True(expected.ExistComment);
            Assert.True(expected.ExistInternalComment);

            this.Transaction.Derive();

            var expectedTakenViaSupplier = expected.TakenViaSupplier;
            var expectedTakenViaContactMechanism = expected.DerivedTakenViaContactMechanism;
            var expectedTakenViaContactPerson = expected.TakenViaContactPerson;
            var expectedBillToContactMechanism = expected.DerivedBillToContactMechanism;
            var expectedBillToContactPerson = expected.BillToContactPerson;
            var expectedShipToAddress = expected.DerivedShipToAddress;
            var expectedShipToContactPerson = expected.ShipToContactPerson;
            var expectedStoredInFacility = expected.StoredInFacility;
            var expectedCustomerReference = expected.CustomerReference;
            var expectedDescription = expected.Description;
            var expectedComment = expected.Comment;
            var expectedInternalComment = expected.InternalComment;

            var purchaseOrder = before.First();
            var id = purchaseOrder.Id;

            this.purchaseOrderListPage.Table.DefaultAction(purchaseOrder);
            var purchaseOrderOverview = new PurchaseOrderOverviewComponent(this.purchaseOrderListPage.Driver, this.M);
            var purchaseOrderOverviewDetail = purchaseOrderOverview.PurchaseorderOverviewDetail.Click();

            purchaseOrderOverviewDetail.TakenViaSupplier.Select(expected.TakenViaSupplier.DisplayName());
            purchaseOrderOverviewDetail.DerivedTakenViaContactMechanism.Select(expected.DerivedTakenViaContactMechanism);
            purchaseOrderOverviewDetail.TakenViaContactPerson.Select(expected.TakenViaContactPerson);
            purchaseOrderOverviewDetail.DerivedBillToContactMechanism.Select(expected.DerivedBillToContactMechanism);
            purchaseOrderOverviewDetail.BillToContactPerson.Select(expected.BillToContactPerson);
            purchaseOrderOverviewDetail.DerivedShipToAddress.Select(expected.DerivedShipToAddress);
            purchaseOrderOverviewDetail.ShipToContactPerson.Select(expected.ShipToContactPerson);
            purchaseOrderOverviewDetail.StoredInFacility.Select(expected.StoredInFacility);
            purchaseOrderOverviewDetail.CustomerReference.Set(expected.CustomerReference);
            purchaseOrderOverviewDetail.Description.Set(expected.Description);
            purchaseOrderOverviewDetail.Comment.Set(expected.Comment);
            purchaseOrderOverviewDetail.InternalComment.Set(expected.InternalComment);

            this.Transaction.Rollback();
            purchaseOrderOverviewDetail.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new PurchaseOrders(this.Transaction).Extent().ToArray();
            purchaseOrder = (PurchaseOrder)this.Transaction.Instantiate(id);

            Assert.Equal(after.Length, before.Length);

            Assert.Equal(expectedTakenViaSupplier, purchaseOrder.TakenViaSupplier);
            Assert.Equal(expectedTakenViaContactMechanism, purchaseOrder.DerivedTakenViaContactMechanism);
            Assert.Equal(expectedTakenViaContactPerson, purchaseOrder.TakenViaContactPerson);
            Assert.Equal(expectedBillToContactMechanism, purchaseOrder.DerivedBillToContactMechanism);
            Assert.Equal(expectedBillToContactPerson, purchaseOrder.BillToContactPerson);
            Assert.Equal(expectedShipToAddress, purchaseOrder.DerivedShipToAddress);
            Assert.Equal(expectedShipToContactPerson, purchaseOrder.ShipToContactPerson);
            Assert.Equal(expectedStoredInFacility.Name, purchaseOrder.StoredInFacility.Name);
            Assert.Equal(expectedCustomerReference, purchaseOrder.CustomerReference);
            Assert.Equal(expectedDescription, purchaseOrder.Description);
            Assert.Equal(expectedComment, purchaseOrder.Comment);
            Assert.Equal(expectedInternalComment, purchaseOrder.InternalComment);
        }
    }
}
