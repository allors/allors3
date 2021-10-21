// <copyright file="PurchaseOrderCreateTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using libs.workspace.angular.apps.src.lib.objects.purchaseorder.create;
using libs.workspace.angular.apps.src.lib.objects.purchaseorder.list;

namespace Tests.PurchaseOrderTests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Components;
    using Xunit;

    [Collection("Test collection")]
    [Trait("Category", "Order")]
    public class PurchaseOrderCreateTest : Test, IClassFixture<Fixture>
    {
        private readonly PurchaseOrderListComponent purchaseOrderListPage;
        private readonly Organisation internalOrganisation;

        public PurchaseOrderCreateTest(Fixture fixture)
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
        public void CreateWithDefaults()
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
            var expectedInternalComment = expected.InternalComment;

            var purchaseOrderCreate = this.purchaseOrderListPage
                .CreatePurchaseOrder()
                .BuildForDefaults(expected);

            this.Transaction.Rollback();
            purchaseOrderCreate.SAVE.Click();

            this.Driver.WaitForAngular();
            this.Transaction.Rollback();

            var after = new PurchaseOrders(this.Transaction).Extent().ToArray();

            Assert.Equal(after.Length, before.Length + 1);

            var actual = after.Except(before).First();

            this.Driver.WaitForAngular();

            Assert.Equal(expectedTakenViaSupplier, actual.TakenViaSupplier);
            Assert.Equal(expectedTakenViaContactMechanism, actual.DerivedTakenViaContactMechanism);
            Assert.Equal(expectedTakenViaContactPerson, actual.TakenViaContactPerson);
            Assert.Equal(expectedBillToContactMechanism, actual.DerivedBillToContactMechanism);
            Assert.Equal(expectedBillToContactPerson, actual.BillToContactPerson);
            Assert.Equal(expectedShipToAddress, actual.DerivedShipToAddress);
            Assert.Equal(expectedShipToContactPerson, actual.ShipToContactPerson);
            Assert.Equal(expectedStoredInFacility, actual.StoredInFacility);
            Assert.Equal(expectedCustomerReference, actual.CustomerReference);
            Assert.Equal(expectedDescription, actual.Description);
            Assert.Equal(expectedInternalComment, actual.InternalComment);
        }
    }
}
