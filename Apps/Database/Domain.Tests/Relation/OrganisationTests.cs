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
    using TestPopulation;
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

    public class OrganisationDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public OrganisationDerivationTests(Fixture fixture) : base(fixture) { }

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
    public class OrganisationDeniedPermissionDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public OrganisationDeniedPermissionDerivationTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Transaction).Get(this.M.Organisation.ObjectType, this.M.Organisation.Delete);

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
        public void OnChangeOrganisationIsInternalOrganisationDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithExternalAccountingTransactionFromPartyDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new SalesAccountingTransactionBuilder(this.Transaction).WithFromParty(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithExternalAccountingTransactionToPartyDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var externalAccountingTransaction = new SalesAccountingTransactionBuilder(this.Transaction)
                .WithToParty(organisation).Build();

            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithShipmentFromPartyDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var shipment = new TransferBuilder(this.Transaction).WithShipFromParty(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithShipmentToPartyDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var shipment = new TransferBuilder(this.Transaction).WithShipToParty(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPaymentReceiverDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var payment = new ReceiptBuilder(this.Transaction).WithReceiver(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPaymentSenderDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var payment = new ReceiptBuilder(this.Transaction).WithSender(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithEmploymentDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var employment = new EmploymentBuilder(this.Transaction).WithEmployer(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithEngagementBillToPartyDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var engagement = new EngagementBuilder(this.Transaction).WithBillToParty(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithEngagementPlacingPartyDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var engagement = new EngagementBuilder(this.Transaction).WithPlacingParty(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPartManufacturedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var part = new NonUnifiedPartBuilder(this.Transaction).WithManufacturedBy(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPartSuppliedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var part = new NonUnifiedPartBuilder(this.Transaction).Build();

            var supplierOffering = new SupplierOfferingBuilder(this.Transaction)
                .WithFromDate(DateTime.Now.AddDays(-1))
                .WithThroughDate(DateTime.Now.AddDays(5))
                .WithSupplier(organisation)
                .WithPart(part)
                .Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithOrganisationGlAccountDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var organisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction).WithInternalOrganisation(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithOrganisationRollUpDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var organisationRollUp = new OrganisationRollUpBuilder(this.Transaction).WithParent(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPartyFixedAssetAssignmentDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var partyFixedAssetAssignment = new PartyFixedAssetAssignmentBuilder(this.Transaction).WithParty(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPickListDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var pickList = new PickListBuilder(this.Transaction).WithShipToParty(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithQuoteIssuerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction)
                .WithQuoteNumberCounter(new CounterBuilder(this.Transaction).Build())
                .WithIsInternalOrganisation(true)
                .Build();
            this.Transaction.Derive(false);

            new ProposalBuilder(this.Transaction).WithIssuer(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithQuoteReceiverDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction)
                .WithQuoteNumberCounter(new CounterBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            var quote = new ProposalBuilder(this.Transaction).WithReceiver(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPurchaseInvoiceBilledToDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction)
                .WithPurchaseInvoiceNumberCounter(new CounterBuilder(this.Transaction).Build())
                .WithIsInternalOrganisation(true)
                .Build();
            this.Transaction.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledTo(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPurchaseInvoiceBilledFromDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction)
                .WithPurchaseInvoiceNumberCounter(new CounterBuilder(this.Transaction).Build())
                .WithIsInternalOrganisation(true)
                .Build();
            this.Transaction.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledTo(this.InternalOrganisation).Build();
            this.Transaction.Derive(false);

            purchaseInvoice.BilledFrom = organisation;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPurchaseInvoiceShipToCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction)
                .WithPurchaseInvoiceNumberCounter(new CounterBuilder(this.Transaction).Build())
                .WithIsInternalOrganisation(true)
                .Build();
            this.Transaction.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).WithShipToCustomer(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPurchaseInvoiceBillToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction)
                .WithPurchaseInvoiceNumberCounter(new CounterBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).WithBillToEndCustomer(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPurchaseInvoiceShipToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction)
                .WithPurchaseInvoiceNumberCounter(new CounterBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).WithShipToEndCustomer(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPurchaseOrderTakenViaSupplierDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseOrder = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithPurchaseOrderTakenViaSubcontractorDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseOrder = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSubcontractor(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithRequestOriginatorDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction)
                .WithRequestNumberCounter(new CounterBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            var request = new RequestForQuoteBuilder(this.Transaction).WithOriginator(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithRequestRecipientDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction)
                .WithRequestNumberCounter(new CounterBuilder(this.Transaction).Build())
                .WithIsInternalOrganisation(true)
                .Build();
            this.Transaction.Derive(false);

            new RequestForQuoteBuilder(this.Transaction).WithRecipient(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithRequirementAuthorizerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var requirement = new RequirementBuilder(this.Transaction).WithAuthorizer(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithRequirementNeededForDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var requirement = new RequirementBuilder(this.Transaction).WithNeededFor(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithRequirementOriginatorDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var requirement = new RequirementBuilder(this.Transaction).WithOriginator(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithRequirementServicedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var requirement = new RequirementBuilder(this.Transaction).WithServicedBy(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesInvoiceBilledFromDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithBilledFrom(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesInvoiceBillToCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithBillToCustomer(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesInvoiceBillToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithBillToEndCustomer(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesInvoiceShipToCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithShipToCustomer(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesInvoiceShipToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithShipToEndCustomer(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesOrderBillToCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Transaction).WithBillToCustomer(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesOrderBillToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Transaction).WithBillToEndCustomer(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesOrderShipToCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Transaction).WithShipToCustomer(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesOrderShipToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Transaction).WithShipToEndCustomer(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesOrderPlacingCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Transaction).WithPlacingCustomer(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesOrderTakenByDeletePermission()
        {
            var salesOrder = new SalesOrderBuilder(this.Transaction).WithOrganisationExternalDefaults(this.InternalOrganisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, this.InternalOrganisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSalesOrderItemAssignedShipToPartyDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();

            var salesOrderItem = new SalesOrderItemBuilder(this.Transaction).WithAssignedShipToParty(organisation).Build();

            salesOrder.AddSalesOrderItem(salesOrderItem);
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSerialisedItemSuppliedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithAssignedSuppliedBy(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSerialisedItemOwnedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithOwnedBy(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSerialisedItemRentedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithRentedBy(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSerialisedItemBuyerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithBuyer(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithSerialisedItemSellerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithSeller(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithWorkTaskCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workTask = new WorkTaskBuilder(this.Transaction).WithCustomer(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithWorkTaskExecutedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workTask = new WorkTaskBuilder(this.Transaction).WithExecutedBy(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationWithWorkEffortPartyAssignmentPartyDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workTask = new WorkEffortPartyAssignmentBuilder(this.Transaction).WithParty(organisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }
    }
}
