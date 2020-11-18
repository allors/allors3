// <copyright file="OrganisationTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using Allors.Database.Domain.TestPopulation;
    using Xunit;

    public class OrganisationTests : DomainTest, IClassFixture<Fixture>
    {
        public OrganisationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenOrganisation_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new OrganisationBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithName("Organisation");
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenOrganisation_WhenActiveContactRelationship_ThenOrganisationCurrentOrganisationContactRelationshipsContainsOrganisation()
        {
            var contact = new PersonBuilder(this.Session).WithLastName("organisationContact").Build();
            var organisation = new OrganisationBuilder(this.Session).WithName("organisation").Build();

            new CustomerRelationshipBuilder(this.Session)
                .WithCustomer(organisation)
                .WithFromDate(DateTimeFactory.CreateDate(2010, 01, 01))
                .Build();

            new OrganisationContactRelationshipBuilder(this.Session)
                .WithContact(contact)
                .WithOrganisation(organisation)
                .WithFromDate(this.Session.Now().Date)
                .Build();

            this.Session.Derive();

            Assert.Equal(contact.CurrentOrganisationContactRelationships[0].Organisation, organisation);
            Assert.Equal(0, contact.InactiveOrganisationContactRelationships.Count);
        }

        [Fact]
        public void GivenOrganisation_WhenInActiveContactRelationship_ThenOrganisationnactiveOrganisationContactRelationshipsContainsOrganisation()
        {
            var contact = new PersonBuilder(this.Session).WithLastName("organisationContact").Build();
            var organisation = new OrganisationBuilder(this.Session).WithName("organisation").Build();

            new CustomerRelationshipBuilder(this.Session)
                .WithCustomer(organisation)
                .WithFromDate(DateTimeFactory.CreateDate(2010, 01, 01))
                .Build();

            new OrganisationContactRelationshipBuilder(this.Session)
                .WithContact(contact)
                .WithOrganisation(organisation)
                .WithFromDate(this.Session.Now().Date.AddDays(-1))
                .WithThroughDate(this.Session.Now().Date.AddDays(-1))
                .Build();

            this.Session.Derive();

            Assert.Equal(contact.InactiveOrganisationContactRelationships[0].Organisation, organisation);
            Assert.Equal(0, contact.CurrentOrganisationContactRelationships.Count);
        }
    }

    [Trait("Category", "Security")]
    public class OrganisationDeniedPermissionTests : DomainTest, IClassFixture<Fixture>
    {
        public OrganisationDeniedPermissionTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Session).Get(this.M.Organisation.ObjectType, this.M.Organisation.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;


        [Fact]
        public void OnChangeOrganisationDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationIsInternalOrganisationDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).WithIsInternalOrganisation(true).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithExternalAccountingTransactionFromPartyDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var externalAccountingTransaction = new SalesAccountingTransactionBuilder(this.Session)
                .WithFromParty(organisation).Build();

            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithExternalAccountingTransactionToPartyDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var externalAccountingTransaction = new SalesAccountingTransactionBuilder(this.Session)
                .WithToParty(organisation).Build();

            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithShipmentFromPartyDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipment = new TransferBuilder(this.Session).WithShipFromParty(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithShipmentToPartyDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipment = new TransferBuilder(this.Session).WithShipToParty(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPaymentReceiverDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var payment = new ReceiptBuilder(this.Session).WithReceiver(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPaymentSenderDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var payment = new ReceiptBuilder(this.Session).WithSender(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithEmploymentDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var employment = new EmploymentBuilder(this.Session).WithEmployer(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithEngagementBillToPartyDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var engagement = new EngagementBuilder(this.Session).WithBillToParty(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithEngagementPlacingPartyDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var engagement = new EngagementBuilder(this.Session).WithPlacingParty(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPartManufacturedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var part = new NonUnifiedPartBuilder(this.Session).WithManufacturedBy(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPartSuppliedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var part = new NonUnifiedPartBuilder(this.Session).Build();

            var supplierOffering = new SupplierOfferingBuilder(this.Session)
                .WithFromDate(DateTime.Now.AddDays(-1))
                .WithThroughDate(DateTime.Now.AddDays(5))
                .WithSupplier(organisation)
                .WithPart(part)
                .Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithOrganisationGlAccountDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var organisationGlAccount = new OrganisationGlAccountBuilder(this.Session).WithInternalOrganisation(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithOrganisationRollUpDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var organisationRollUp = new OrganisationRollUpBuilder(this.Session).WithParent(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPartyFixedAssetAssignmentDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var partyFixedAssetAssignment = new PartyFixedAssetAssignmentBuilder(this.Session).WithParty(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPickListDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var pickList = new PickListBuilder(this.Session).WithShipToParty(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithQuoteIssuerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session)
                .WithQuoteCounter(new CounterBuilder(this.Session).Build()).Build();
            this.Session.Derive(false);

            var quote = new ProposalBuilder(this.Session).WithIssuer(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithQuoteReceiverDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session)
                .WithQuoteCounter(new CounterBuilder(this.Session).Build()).Build();
            this.Session.Derive(false);

            var quote = new ProposalBuilder(this.Session).WithReceiver(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPurchaseInvoiceBilledToDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session)
                .WithPurchaseInvoiceCounter(new CounterBuilder(this.Session).Build())
                .Build();
            this.Session.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Session).WithBilledTo(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPurchaseInvoiceBilledFromDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session)
                .WithPurchaseInvoiceCounter(new CounterBuilder(this.Session).Build())
                .Build();
            this.Session.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Session).WithBilledFrom(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPurchaseInvoiceShipToCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session)
                .WithPurchaseInvoiceCounter(new CounterBuilder(this.Session).Build())
                .Build();
            this.Session.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Session).WithShipToCustomer(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPurchaseInvoiceBillToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session)
                .WithPurchaseInvoiceCounter(new CounterBuilder(this.Session).Build())
                .Build();
            this.Session.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Session).WithBillToEndCustomer(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPurchaseInvoiceShipToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session)
                .WithPurchaseInvoiceCounter(new CounterBuilder(this.Session).Build())
                .Build();
            this.Session.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Session).WithShipToEndCustomer(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPurchaseOrderTakenViaSupplierDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var purchaseOrder = new PurchaseOrderBuilder(this.Session).WithTakenViaSupplier(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPurchaseOrderTakenViaSubcontractorDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var purchaseOrder = new PurchaseOrderBuilder(this.Session).WithTakenViaSubcontractor(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithRequestOriginatorDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session)
                .WithRequestCounter(new CounterBuilder(this.Session).Build())
                .Build();
            this.Session.Derive(false);

            var request = new RequestForQuoteBuilder(this.Session).WithOriginator(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithRequestRecipientDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session)
                .WithRequestCounter(new CounterBuilder(this.Session).Build())
                .Build();
            this.Session.Derive(false);

            var request = new RequestForQuoteBuilder(this.Session).WithRecipient(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithRequirementAuthorizerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var requirement = new RequirementBuilder(this.Session).WithAuthorizer(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithRequirementNeededForDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var requirement = new RequirementBuilder(this.Session).WithNeededFor(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithRequirementOriginatorDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var requirement = new RequirementBuilder(this.Session).WithOriginator(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithRequirementServicedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var requirement = new RequirementBuilder(this.Session).WithServicedBy(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesInvoiceBilledFromDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Session).WithBilledFrom(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesInvoiceBillToCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Session).WithBillToCustomer(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesInvoiceBillToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Session).WithBillToEndCustomer(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesInvoiceShipToCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Session).WithShipToCustomer(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesInvoiceShipToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Session).WithShipToEndCustomer(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesOrderBillToCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Session).WithBillToCustomer(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesOrderBillToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Session).WithBillToEndCustomer(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesOrderShipToCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Session).WithShipToCustomer(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesOrderShipToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Session).WithShipToEndCustomer(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesOrderPlacingCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Session).WithPlacingCustomer(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesOrderTakenByDeletePermission()
        {
            var salesOrder = new SalesOrderBuilder(this.Session).WithOrganisationExternalDefaults(this.InternalOrganisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, this.InternalOrganisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesOrderItemAssignedShipToPartyDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Session).Build();

            var salesOrderItem = new SalesOrderItemBuilder(this.Session).WithAssignedShipToParty(organisation).Build();

            salesOrder.AddSalesOrderItem(salesOrderItem);
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSerialisedItemSuppliedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Session).WithAssignedSuppliedBy(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSerialisedItemOwnedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Session).WithOwnedBy(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSerialisedItemRentedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Session).WithRentedBy(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSerialisedItemBuyerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Session).WithBuyer(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSerialisedItemSellerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Session).WithSeller(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithWorkTaskCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var workTask = new WorkTaskBuilder(this.Session).WithCustomer(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithWorkTaskExecutedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var workTask = new WorkTaskBuilder(this.Session).WithExecutedBy(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithWorkEffortPartyAssignmentPartyDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Session).Build();
            this.Session.Derive(false);

            var workTask = new WorkEffortPartyAssignmentBuilder(this.Session).WithParty(organisation).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }
    }
}
