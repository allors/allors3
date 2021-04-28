// <copyright file="OrganisationTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class OrganisationTests : DomainTest, IClassFixture<Fixture>
    {
        public OrganisationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenOrganisation_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new OrganisationBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithName("Organisation");
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenOrganisation_WhenActiveContactRelationship_ThenOrganisationCurrentOrganisationContactRelationshipsContainsOrganisation()
        {
            var contact = new PersonBuilder(this.Transaction).WithLastName("organisationContact").Build();
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").Build();

            new CustomerRelationshipBuilder(this.Transaction)
                .WithCustomer(organisation)
                .WithFromDate(DateTimeFactory.CreateDate(2010, 01, 01))
                .Build();

            new OrganisationContactRelationshipBuilder(this.Transaction)
                .WithContact(contact)
                .WithOrganisation(organisation)
                .WithFromDate(this.Transaction.Now().Date)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(contact.CurrentOrganisationContactRelationships[0].Organisation, organisation);
            Assert.Empty(contact.InactiveOrganisationContactRelationships);
        }

        [Fact]
        public void GivenOrganisation_WhenInActiveContactRelationship_ThenOrganisationnactiveOrganisationContactRelationshipsContainsOrganisation()
        {
            var contact = new PersonBuilder(this.Transaction).WithLastName("organisationContact").Build();
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").Build();

            new CustomerRelationshipBuilder(this.Transaction)
                .WithCustomer(organisation)
                .WithFromDate(DateTimeFactory.CreateDate(2010, 01, 01))
                .Build();

            new OrganisationContactRelationshipBuilder(this.Transaction)
                .WithContact(contact)
                .WithOrganisation(organisation)
                .WithFromDate(this.Transaction.Now().Date.AddDays(-1))
                .WithThroughDate(this.Transaction.Now().Date.AddDays(-1))
                .Build();

            this.Transaction.Derive();

            Assert.Equal(contact.InactiveOrganisationContactRelationships[0].Organisation, organisation);
            Assert.Empty(contact.CurrentOrganisationContactRelationships);
        }
    }

    public class OrganisationRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public OrganisationRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedNameDerivePartyName()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            organisation.Name = "name";
            this.Transaction.Derive(false);

            Assert.Equal("name", organisation.PartyName);
        }

        [Fact]
        public void ChangedUniqueIdDeriveContactsUserGroup()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.True(organisation.ExistContactsUserGroup);
        }

        [Fact]
        public void ChangedEmploymentEmployerDeriveActiveEmployees()
        {
            var employee = new PersonBuilder(this.Transaction).Build();
            var employment = new EmploymentBuilder(this.Transaction).WithEmployee(employee).WithFromDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var employer = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            employment.Employer = employer;
            this.Transaction.Derive(false);

            Assert.Contains(employee, employer.ActiveEmployees);
        }

        [Fact]
        public void ChangedEmploymentFromDateDeriveActiveEmployees()
        {
            var employee = new PersonBuilder(this.Transaction).Build();
            var employer = new OrganisationBuilder(this.Transaction).Build();
            var employment = new EmploymentBuilder(this.Transaction).WithEmployee(employee).WithEmployer(employer).Build();
            this.Transaction.Derive(false);

            employment.FromDate = this.Transaction.Now();
            this.Transaction.Derive(false);

            Assert.Contains(employee, employer.ActiveEmployees);
        }

        [Fact]
        public void ChangedEmploymentThroughDateDeriveActiveEmployees()
        {
            var employee = new PersonBuilder(this.Transaction).Build();
            var employer = new OrganisationBuilder(this.Transaction).Build();
            var employment = new EmploymentBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithEmployee(employee).WithEmployer(employer).Build();
            this.Transaction.Derive(false);

            Assert.Contains(employee, employer.ActiveEmployees);

            employment.ThroughDate = employment.FromDate;
            this.Transaction.Derive(false);

            Assert.DoesNotContain(employee, employer.ActiveEmployees);
        }

        [Fact]
        public void ChangedCustomerRelationshipEmployerDeriveActiveCustomers()
        {
            var customer = new PersonBuilder(this.Transaction).Build();
            var customerRelationship = new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithFromDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive(false);

            customerRelationship.InternalOrganisation = internalOrganisation;
            this.Transaction.Derive(false);

            Assert.Contains(customer, internalOrganisation.ActiveCustomers);
        }

        [Fact]
        public void ChangedCustomerRelationshipFromDateDeriveActiveCustomers()
        {
            var customer = new PersonBuilder(this.Transaction).Build();
            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var customerRelationship = new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();
            this.Transaction.Derive(false);

            customerRelationship.FromDate = this.Transaction.Now();
            this.Transaction.Derive(false);

            Assert.Contains(customer, internalOrganisation.ActiveCustomers);
        }

        [Fact]
        public void ChangedCustomerRelationshipThroughDateDeriveActiveCustomers()
        {
            var customer = new PersonBuilder(this.Transaction).Build();
            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var customerRelationship = new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(customer, internalOrganisation.ActiveCustomers);

            customerRelationship.ThroughDate = customerRelationship.FromDate;
            this.Transaction.Derive(false);

            Assert.DoesNotContain(customer, internalOrganisation.ActiveCustomers);
        }

        [Fact]
        public void ChangedSupplierRelationshipEmployerDeriveActiveSuppliers()
        {
            var supplier = new OrganisationBuilder(this.Transaction).Build();
            var supplierRelationship = new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).WithFromDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive(false);

            supplierRelationship.InternalOrganisation = internalOrganisation;
            this.Transaction.Derive(false);

            Assert.Contains(supplier, internalOrganisation.ActiveSuppliers);
        }

        [Fact]
        public void ChangedSupplierRelationshipFromDateDeriveActiveSuppliers()
        {
            var supplier = new OrganisationBuilder(this.Transaction).Build();
            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var supplierRelationship = new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).WithInternalOrganisation(internalOrganisation).Build();
            this.Transaction.Derive(false);

            supplierRelationship.FromDate = this.Transaction.Now();
            this.Transaction.Derive(false);

            Assert.Contains(supplier, internalOrganisation.ActiveSuppliers);
        }

        [Fact]
        public void ChangedSupplierRelationshipThroughDateDeriveActiveSuppliers()
        {
            var supplier = new OrganisationBuilder(this.Transaction).Build();
            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var supplierRelationship = new SupplierRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithSupplier(supplier).WithInternalOrganisation(internalOrganisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(supplier, internalOrganisation.ActiveSuppliers);

            supplierRelationship.ThroughDate = supplierRelationship.FromDate;
            this.Transaction.Derive(false);

            Assert.DoesNotContain(supplier, internalOrganisation.ActiveSuppliers);
        }

        [Fact]
        public void ChangedSubContractorRelationshipEmployerDeriveActiveSubContractors()
        {
            var subContractor = new OrganisationBuilder(this.Transaction).Build();
            var subContractorRelationship = new SubContractorRelationshipBuilder(this.Transaction).WithSubContractor(subContractor).WithFromDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive(false);

            subContractorRelationship.Contractor = internalOrganisation;
            this.Transaction.Derive(false);

            Assert.Contains(subContractor, internalOrganisation.ActiveSubContractors);
        }

        [Fact]
        public void ChangedSubContractorRelationshipFromDateDeriveActiveSubContractors()
        {
            var subContractor = new OrganisationBuilder(this.Transaction).Build();
            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var subContractorRelationship = new SubContractorRelationshipBuilder(this.Transaction).WithSubContractor(subContractor).WithContractor(internalOrganisation).Build();
            this.Transaction.Derive(false);

            subContractorRelationship.FromDate = this.Transaction.Now();
            this.Transaction.Derive(false);

            Assert.Contains(subContractor, internalOrganisation.ActiveSubContractors);
        }

        [Fact]
        public void ChangedSubContractorRelationshipThroughDateDeriveActiveSubContractors()
        {
            var subContractor = new OrganisationBuilder(this.Transaction).Build();
            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var subContractorRelationship = new SubContractorRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithSubContractor(subContractor).WithContractor(internalOrganisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(subContractor, internalOrganisation.ActiveSubContractors);

            subContractorRelationship.ThroughDate = subContractorRelationship.FromDate;
            this.Transaction.Derive(false);

            Assert.DoesNotContain(subContractor, internalOrganisation.ActiveSubContractors);
        }

        [Fact]
        public void ChangedOrganisationContactRelationshipEmployerDeriveCurrentOrganisationContactRelationships()
        {
            var contact = new PersonBuilder(this.Transaction).Build();
            var contactRelationship = new OrganisationContactRelationshipBuilder(this.Transaction).WithContact(contact).WithFromDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            contactRelationship.Organisation = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(contactRelationship, organisation.CurrentOrganisationContactRelationships);
        }

        [Fact]
        public void ChangedOrganisationContactRelationshipFromDateDeriveCurrentOrganisationContactRelationships()
        {
            var contact = new PersonBuilder(this.Transaction).Build();
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            var contactRelationship = new OrganisationContactRelationshipBuilder(this.Transaction).WithContact(contact).WithOrganisation(organisation).Build();
            this.Transaction.Derive(false);

            contactRelationship.FromDate = this.Transaction.Now();
            this.Transaction.Derive(false);

            Assert.Contains(contactRelationship, organisation.CurrentOrganisationContactRelationships);
        }

        [Fact]
        public void ChangedOrganisationContactRelationshipThroughDateDeriveCurrentOrganisationContactRelationships()
        {
            var contact = new PersonBuilder(this.Transaction).Build();
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            var contactRelationship = new OrganisationContactRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithContact(contact).WithOrganisation(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(contactRelationship, organisation.CurrentOrganisationContactRelationships);

            contactRelationship.ThroughDate = contactRelationship.FromDate;
            this.Transaction.Derive(false);

            Assert.DoesNotContain(contactRelationship, organisation.CurrentOrganisationContactRelationships);
        }

        [Fact]
        public void ChangedOrganisationContactRelationshipEmployerDeriveCurrentContacts()
        {
            var contact = new PersonBuilder(this.Transaction).Build();
            var contactRelationship = new OrganisationContactRelationshipBuilder(this.Transaction).WithContact(contact).WithFromDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            contactRelationship.Organisation = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(contact, organisation.CurrentContacts);
        }

        [Fact]
        public void ChangedOrganisationContactRelationshipFromDateDeriveCurrentContacts()
        {
            var contact = new PersonBuilder(this.Transaction).Build();
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            var contactRelationship = new OrganisationContactRelationshipBuilder(this.Transaction).WithContact(contact).WithOrganisation(organisation).Build();
            this.Transaction.Derive(false);

            contactRelationship.FromDate = this.Transaction.Now();
            this.Transaction.Derive(false);

            Assert.Contains(contact, organisation.CurrentContacts);
        }

        [Fact]
        public void ChangedOrganisationContactRelationshipThroughDateDeriveCurrentContacts()
        {
            var contact = new PersonBuilder(this.Transaction).Build();
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            var contactRelationship = new OrganisationContactRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithContact(contact).WithOrganisation(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(contact, organisation.CurrentContacts);

            contactRelationship.ThroughDate = contactRelationship.FromDate;
            this.Transaction.Derive(false);

            Assert.DoesNotContain(contact, organisation.CurrentContacts);
        }
    }

    [Trait("Category", "Security")]
    public class OrganisationDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public OrganisationDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Transaction).Get(this.M.Organisation, this.M.Organisation.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangeOrganisationDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeIsInternalOrganisationDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            organisation.IsInternalOrganisation = true;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeExternalAccountingTransactionFromPartyDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var accountingTransaction = new SalesAccountingTransactionBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            accountingTransaction.FromParty = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeExternalAccountingTransactionToPartyDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var accountingTransaction = new SalesAccountingTransactionBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            accountingTransaction.ToParty = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeShipmentShipFromPartyDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var shipment = new TransferBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            shipment.ShipFromParty = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeShipmentShipToPartyDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var shipment = new TransferBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            shipment.ShipToParty = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePaymentReceiverDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var payment = new ReceiptBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            payment.Receiver = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePaymentSenderDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var payment = new ReceiptBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            payment.Sender = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeEmploymentEmployerDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var employment = new EmploymentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            employment.Employer = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeEngagementBillToPartyDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var engagement = new EngagementBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            engagement.BillToParty = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeEngagementPlacingPartyDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var engagement = new EngagementBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            engagement.PlacingParty = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePartManufacturedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var part = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            part.ManufacturedBy = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePartSuppliedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var part = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            part.AddSuppliedBy(organisation);
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationGlAccountInternalOrganisationDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var organisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            organisationGlAccount.InternalOrganisation = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationRollUpParentDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var organisationRollUp = new OrganisationRollUpBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            organisationRollUp.Parent = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePartyFixedAssetAssignmentPartyDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var partyFixedAssetAssignment = new PartyFixedAssetAssignmentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            partyFixedAssetAssignment.Party = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePickListShipToPartyDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var pickList = new PickListBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            pickList.ShipToParty = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeQuoteIssuerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var quote = new ProposalBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            quote.Issuer = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeQuoteReceiverDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var quote = new ProposalBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            quote.Receiver = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePurchaseInvoiceBilledToDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            purchaseInvoice.BilledTo = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePurchaseInvoiceBilledFromDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            purchaseInvoice.BilledFrom = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePurchaseInvoiceShipToCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            purchaseInvoice.ShipToCustomer = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePurchaseInvoiceBillToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            purchaseInvoice.BillToEndCustomer = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePurchaseInvoiceShipToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            purchaseInvoice.ShipToEndCustomer = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePurchaseOrderTakenViaSupplierDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseOrder = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            purchaseOrder.TakenViaSupplier = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePurchaseOrderTakenViaSubcontractorDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseOrder = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            purchaseOrder.TakenViaSubcontractor = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeRequestOriginatorDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            request.Originator = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeRequestRecipientDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            request.Recipient = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeRequirementAuthorizerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var requirement = new RequirementBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            requirement.Authorizer = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeRequirementNeededForDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var requirement = new RequirementBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            requirement.NeededFor = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeRequirementOriginatorDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var requirement = new RequirementBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            requirement.Originator = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeRequirementServicedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var requirement = new RequirementBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            requirement.ServicedBy = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesInvoiceBilledFromDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesInvoice.BilledFrom = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesInvoiceBillToCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesInvoice.BillToCustomer = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesInvoiceBillToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesInvoice.BillToEndCustomer = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesInvoiceShipToCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesInvoice.ShipToCustomer = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesInvoiceShipToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesInvoice.ShipToEndCustomer = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesOrderBillToCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesOrder.BillToCustomer = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesOrderBillToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesOrder.BillToEndCustomer = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesOrderShipToCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesOrder.ShipToCustomer = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesOrderShipToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesOrder.ShipToEndCustomer = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesOrderPlacingCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesOrder.PlacingCustomer = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesOrderTakenByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesOrder.TakenBy = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, this.InternalOrganisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesOrderItemAssignedShipToPartyDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrderItem = new SalesOrderItemBuilder(this.Transaction).Build();

            salesOrderItem.AssignedShipToParty = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSerialisedItemSuppliedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            serialisedItem.AssignedSuppliedBy = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSerialisedItemOwnedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            serialisedItem.OwnedBy = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSerialisedItemRentedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            serialisedItem.RentedBy = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSerialisedItemBuyerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            serialisedItem.Buyer = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSerialisedItemSellerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            serialisedItem.Seller = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeWorkTaskCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            workTask.Customer = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeWorkTaskExecutedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            workTask.ExecutedBy = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeWorkEffortPartyAssignmentPartyDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var partyAssignment = new WorkEffortPartyAssignmentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            partyAssignment.Party = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }
    }
}
