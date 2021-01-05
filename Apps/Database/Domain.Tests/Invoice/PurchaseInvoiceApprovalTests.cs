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

    [Trait("Category", "Approval")]
    public class PurchaseInvoiceApprovalDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceApprovalDerivationTests(Fixture fixture) : base(fixture) {}

        [Fact]
        public void OnCreatedPurchaseInvoiceApprovalDeriveTitle()
        {
            var purchaseInvoice = this.InternalOrganisation.CreatePurchaseInvoiceWithSerializedItem();

            this.Session.Derive(false);

            var approval = new PurchaseInvoiceApprovalBuilder(this.Session).WithPurchaseInvoice(purchaseInvoice).Build();

            this.Session.Derive(false);

            Assert.Equal(approval.Title, "Approval of " + purchaseInvoice.WorkItemDescription);
        }

        [Fact]
        public void OnCreatedPurchaseInvoiceApprovalDeriveWorkItem()
        {
            var purchaseInvoice = this.InternalOrganisation.CreatePurchaseInvoiceWithSerializedItem();

            this.Session.Derive(false);

            var approval = new PurchaseInvoiceApprovalBuilder(this.Session).WithPurchaseInvoice(purchaseInvoice).Build();

            this.Session.Derive(false);

            Assert.Equal(approval.WorkItem, purchaseInvoice);
        }

        [Fact]
        public void OnCreatedPurchaseInvoiceApprovalDeriveDateClosedExists()
        {
            var purchaseInvoice = this.InternalOrganisation.CreatePurchaseInvoiceWithSerializedItem();

            this.Session.Derive(false);

            var approval = new PurchaseInvoiceApprovalBuilder(this.Session).WithPurchaseInvoice(purchaseInvoice).Build();

            this.Session.Derive(false);

            Assert.True(approval.ExistDateClosed);
        }

        [Fact]
        public void OnCreatedPurchaseInvoiceApprovalDeriveDateClosedNotExists()
        {
            var purchaseInvoice = this.InternalOrganisation.CreatePurchaseInvoiceWithSerializedItem();

            this.Session.Derive(false);

            purchaseInvoice.Confirm();

            this.Session.Derive(false);

            Assert.False(purchaseInvoice.PurchaseInvoiceApprovalsWherePurchaseInvoice.First().ExistDateClosed);
        }

        [Fact]
        public void OnCreatedPurchaseInvoiceApprovalDeriveEmptyParticipants()
        {
            var purchaseInvoice = this.InternalOrganisation.CreatePurchaseInvoiceWithSerializedItem();

            this.Session.Derive(false);

            var approval = new PurchaseInvoiceApprovalBuilder(this.Session).WithPurchaseInvoice(purchaseInvoice).Build();

            this.Session.Derive(false);

            Assert.Empty(approval.Participants);
        }

        [Fact]
        public void OnCreatedPurchaseInvoiceApprovalDeriveParticipants()
        {
            var purchaseInvoice = this.InternalOrganisation.CreatePurchaseInvoiceWithSerializedItem();

            this.Session.Derive(false);

            purchaseInvoice.Confirm();

            this.Session.Derive(false);

            Assert.NotEmpty(purchaseInvoice.PurchaseInvoiceApprovalsWherePurchaseInvoice.First().Participants);
        }

        [Fact]
        public void OnChangedPurchaseInvoiceApprovalDeriveParticipants()
        {
            var purchaseInvoice = this.InternalOrganisation.CreatePurchaseInvoiceWithSerializedItem();

            this.Session.Derive(false);

            purchaseInvoice.Confirm();

            this.Session.Derive(false);

            var approval = purchaseInvoice.PurchaseInvoiceApprovalsWherePurchaseInvoice.First();
            approval.Approve();

            this.Session.Derive(false);

            Assert.Empty(purchaseInvoice.PurchaseInvoiceApprovalsWherePurchaseInvoice.First().Participants);
        }

    }
}
