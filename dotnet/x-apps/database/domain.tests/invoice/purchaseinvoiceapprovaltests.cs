// <copyright file="PurchaseInvoiceTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using TestPopulation;
    using Xunit;

    public class PurchaseInvoiceApprovalRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceApprovalRuleTests(Fixture fixture) : base(fixture) {}

        [Fact]
        public void OnCreatedPurchaseInvoiceApprovalDeriveTitle()
        {
            var purchaseInvoice = this.InternalOrganisation.CreatePurchaseInvoiceWithSerializedItem();

            this.Derive();

            var approval = new PurchaseInvoiceApprovalBuilder(this.Transaction).WithPurchaseInvoice(purchaseInvoice).Build();

            this.Derive();

            Assert.Equal(approval.Title, "Approval of " + purchaseInvoice.WorkItemDescription);
        }

        [Fact]
        public void OnCreatedPurchaseInvoiceApprovalDeriveWorkItem()
        {
            var purchaseInvoice = this.InternalOrganisation.CreatePurchaseInvoiceWithSerializedItem();

            this.Derive();

            var approval = new PurchaseInvoiceApprovalBuilder(this.Transaction).WithPurchaseInvoice(purchaseInvoice).Build();

            this.Derive();

            Assert.Equal(approval.WorkItem, purchaseInvoice);
        }

        [Fact]
        public void OnCreatedPurchaseInvoiceApprovalDeriveDateClosedExists()
        {
            var purchaseInvoice = this.InternalOrganisation.CreatePurchaseInvoiceWithSerializedItem();

            this.Derive();

            var approval = new PurchaseInvoiceApprovalBuilder(this.Transaction).WithPurchaseInvoice(purchaseInvoice).Build();

            this.Derive();

            Assert.True(approval.ExistDateClosed);
        }

        [Fact]
        public void OnCreatedPurchaseInvoiceApprovalDeriveDateClosedNotExists()
        {
            var purchaseInvoice = this.InternalOrganisation.CreatePurchaseInvoiceWithSerializedItem();

            this.Derive();

            purchaseInvoice.Confirm();

            this.Derive();

            Assert.False(purchaseInvoice.PurchaseInvoiceApprovalsWherePurchaseInvoice.First().ExistDateClosed);
        }

        [Fact]
        public void OnCreatedPurchaseInvoiceApprovalDeriveEmptyParticipants()
        {
            var purchaseInvoice = this.InternalOrganisation.CreatePurchaseInvoiceWithSerializedItem();

            this.Derive();

            var approval = new PurchaseInvoiceApprovalBuilder(this.Transaction).WithPurchaseInvoice(purchaseInvoice).Build();

            this.Derive();

            Assert.Empty(approval.Participants);
        }

        [Fact]
        public void OnCreatedPurchaseInvoiceApprovalDeriveParticipants()
        {
            var purchaseInvoice = this.InternalOrganisation.CreatePurchaseInvoiceWithSerializedItem();

            this.Derive();

            purchaseInvoice.Confirm();

            this.Derive();

            Assert.NotEmpty(purchaseInvoice.PurchaseInvoiceApprovalsWherePurchaseInvoice.First().Participants);
        }

        [Fact]
        public void OnChangedPurchaseInvoiceApprovalDeriveParticipants()
        {
            var purchaseInvoice = this.InternalOrganisation.CreatePurchaseInvoiceWithSerializedItem();

            this.Derive();

            purchaseInvoice.Confirm();

            this.Derive();

            var approval = purchaseInvoice.PurchaseInvoiceApprovalsWherePurchaseInvoice.First();
            approval.Approve();

            this.Derive();

            Assert.Empty(purchaseInvoice.PurchaseInvoiceApprovalsWherePurchaseInvoice.First().Participants);
        }

    }
}
