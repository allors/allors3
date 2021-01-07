// <copyright file="PurchaseOrderApprovalLevel2Tests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using TestPopulation;
    using Resources;
    using Xunit;

    public class PurchaseOrderApprovalLevel2Tests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderApprovalLevel2Tests(Fixture fixture) : base(fixture)
        {

        }

        [Fact]
        public void ChangedPurchaseOrderDeriveTitle()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).WithDefaults(this.InternalOrganisation).Build();
            this.Session.Derive(false);

            var approval = new PurchaseOrderApprovalLevel2Builder(this.Session).WithPurchaseOrder(purchaseOrder).Build();
            this.Session.Derive(false);

            Assert.Equal(approval.Title, "Approval of " + purchaseOrder.WorkItemDescription);
        }

        [Fact]
        public void ChangedPurchaseOrderDeriveWorkItem()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).WithDefaults(this.InternalOrganisation).Build();
            this.Session.Derive(false);

            var approval = new PurchaseOrderApprovalLevel2Builder(this.Session).WithPurchaseOrder(purchaseOrder).Build();
            this.Session.Derive(false);

            Assert.Equal(approval.WorkItem, purchaseOrder);
        }

        [Fact]
        public void ChangedPurchaseOrderDeriveDeriveDateClosed()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var approval = new PurchaseOrderApprovalLevel2Builder(this.Session).WithPurchaseOrder(purchaseOrder).Build();
            this.Session.Derive(false);

            Assert.True(approval.ExistDateClosed);
        }

        [Fact]
        public void ChangedPurchaseOrderPurchaseOrderStateDeriveDateClosed()
        {
            var purchaseOrder = this.InternalOrganisation.CreatePurchaseOrderWithBothItems(this.Session.Faker());

            var supplierRelationship = purchaseOrder.TakenViaSupplier.SupplierRelationshipsWhereSupplier.First(v => v.InternalOrganisation == purchaseOrder.OrderedBy);
            supplierRelationship.NeedsApproval = true;
            supplierRelationship.ApprovalThresholdLevel2 = 2;

            this.Session.Derive(false);

            purchaseOrder.SetReadyForProcessing();
            this.Session.Derive(false);

            Assert.False(purchaseOrder.PurchaseOrderApprovalsLevel2WherePurchaseOrder.First().ExistDateClosed);
        }

        [Fact]
        public void OnCreatedPurchaseOrderApprovalLevel2DeriveEmptyParticipants()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Session).WithDefaults(this.InternalOrganisation).Build();

            this.Session.Derive(false);

            var approval = new PurchaseOrderApprovalLevel2Builder(this.Session).WithPurchaseOrder(purchaseOrder).Build();

            this.Session.Derive(false);

            Assert.Empty(approval.Participants);
        }

        [Fact]
        public void OnCreatedPurchaseInvoiceApprovalLevel2DeriveParticipants()
        {
            var purchaseOrder = this.InternalOrganisation.CreatePurchaseOrderWithNonSerializedItem(this.Session.Faker());

            var supplierRelationship = purchaseOrder.TakenViaSupplier.SupplierRelationshipsWhereSupplier.First(v => v.InternalOrganisation == purchaseOrder.OrderedBy);
            supplierRelationship.NeedsApproval = true;
            supplierRelationship.ApprovalThresholdLevel2 = 1;

            this.Session.Derive(false);

            purchaseOrder.SetReadyForProcessing();

            this.Session.Derive(false);

            Assert.NotEmpty(purchaseOrder.PurchaseOrderApprovalsLevel2WherePurchaseOrder.First().Participants);
        }
    }
}
