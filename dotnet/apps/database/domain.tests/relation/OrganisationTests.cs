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

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithName("Organisation");
            builder.Build();

            Assert.False(this.Derive().HasErrors);
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
            this.Derive();

            organisation.Name = "name";
            this.Derive();

            Assert.Equal("name", organisation.PartyName);
        }

        [Fact]
        public void ChangedUniqueIdDeriveContactsUserGroup()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            Assert.True(organisation.ExistContactsUserGroup);
        }

        [Fact]
        public void ChangedEmploymentEmployerDeriveActiveEmployees()
        {
            var employee = new PersonBuilder(this.Transaction).Build();
            var employment = new EmploymentBuilder(this.Transaction).WithEmployee(employee).WithFromDate(this.Transaction.Now()).Build();
            this.Derive();

            var employer = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            employment.Employer = employer;
            this.Derive();

            Assert.Contains(employee, employer.ActiveEmployees);
        }

        [Fact]
        public void ChangedEmploymentFromDateDeriveActiveEmployees()
        {
            var employee = new PersonBuilder(this.Transaction).Build();
            var employer = new OrganisationBuilder(this.Transaction).Build();
            var employment = new EmploymentBuilder(this.Transaction).WithEmployee(employee).WithEmployer(employer).Build();
            this.Derive();

            employment.FromDate = this.Transaction.Now();
            this.Derive();

            Assert.Contains(employee, employer.ActiveEmployees);
        }

        [Fact]
        public void ChangedEmploymentThroughDateDeriveActiveEmployees()
        {
            var employee = new PersonBuilder(this.Transaction).Build();
            var employer = new OrganisationBuilder(this.Transaction).Build();
            var employment = new EmploymentBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithEmployee(employee).WithEmployer(employer).Build();
            this.Derive();

            Assert.Contains(employee, employer.ActiveEmployees);

            employment.ThroughDate = employment.FromDate;
            this.Derive();

            Assert.DoesNotContain(employee, employer.ActiveEmployees);
        }

        [Fact]
        public void ChangedCustomerRelationshipEmployerDeriveActiveCustomers()
        {
            var customer = new PersonBuilder(this.Transaction).Build();
            var customerRelationship = new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithFromDate(this.Transaction.Now()).Build();
            this.Derive();

            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            this.Derive();

            customerRelationship.InternalOrganisation = internalOrganisation;
            this.Derive();

            Assert.Contains(customer, internalOrganisation.ActiveCustomers);
        }

        [Fact]
        public void ChangedCustomerRelationshipFromDateDeriveActiveCustomers()
        {
            var customer = new PersonBuilder(this.Transaction).Build();
            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var customerRelationship = new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();
            this.Derive();

            customerRelationship.FromDate = this.Transaction.Now();
            this.Derive();

            Assert.Contains(customer, internalOrganisation.ActiveCustomers);
        }

        [Fact]
        public void ChangedCustomerRelationshipThroughDateDeriveActiveCustomers()
        {
            var customer = new PersonBuilder(this.Transaction).Build();
            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var customerRelationship = new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();
            this.Derive();

            Assert.Contains(customer, internalOrganisation.ActiveCustomers);

            customerRelationship.ThroughDate = customerRelationship.FromDate;
            this.Derive();

            Assert.DoesNotContain(customer, internalOrganisation.ActiveCustomers);
        }

        [Fact]
        public void ChangedSupplierRelationshipEmployerDeriveActiveSuppliers()
        {
            var supplier = new OrganisationBuilder(this.Transaction).Build();
            var supplierRelationship = new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).WithFromDate(this.Transaction.Now()).Build();
            this.Derive();

            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            this.Derive();

            supplierRelationship.InternalOrganisation = internalOrganisation;
            this.Derive();

            Assert.Contains(supplier, internalOrganisation.ActiveSuppliers);
        }

        [Fact]
        public void ChangedSupplierRelationshipFromDateDeriveActiveSuppliers()
        {
            var supplier = new OrganisationBuilder(this.Transaction).Build();
            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var supplierRelationship = new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).WithInternalOrganisation(internalOrganisation).Build();
            this.Derive();

            supplierRelationship.FromDate = this.Transaction.Now();
            this.Derive();

            Assert.Contains(supplier, internalOrganisation.ActiveSuppliers);
        }

        [Fact]
        public void ChangedSupplierRelationshipThroughDateDeriveActiveSuppliers()
        {
            var supplier = new OrganisationBuilder(this.Transaction).Build();
            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var supplierRelationship = new SupplierRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithSupplier(supplier).WithInternalOrganisation(internalOrganisation).Build();
            this.Derive();

            Assert.Contains(supplier, internalOrganisation.ActiveSuppliers);

            supplierRelationship.ThroughDate = supplierRelationship.FromDate;
            this.Derive();

            Assert.DoesNotContain(supplier, internalOrganisation.ActiveSuppliers);
        }

        [Fact]
        public void ChangedSubContractorRelationshipEmployerDeriveActiveSubContractors()
        {
            var subContractor = new OrganisationBuilder(this.Transaction).Build();
            var subContractorRelationship = new SubContractorRelationshipBuilder(this.Transaction).WithSubContractor(subContractor).WithFromDate(this.Transaction.Now()).Build();
            this.Derive();

            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            this.Derive();

            subContractorRelationship.Contractor = internalOrganisation;
            this.Derive();

            Assert.Contains(subContractor, internalOrganisation.ActiveSubContractors);
        }

        [Fact]
        public void ChangedSubContractorRelationshipFromDateDeriveActiveSubContractors()
        {
            var subContractor = new OrganisationBuilder(this.Transaction).Build();
            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var subContractorRelationship = new SubContractorRelationshipBuilder(this.Transaction).WithSubContractor(subContractor).WithContractor(internalOrganisation).Build();
            this.Derive();

            subContractorRelationship.FromDate = this.Transaction.Now();
            this.Derive();

            Assert.Contains(subContractor, internalOrganisation.ActiveSubContractors);
        }

        [Fact]
        public void ChangedSubContractorRelationshipThroughDateDeriveActiveSubContractors()
        {
            var subContractor = new OrganisationBuilder(this.Transaction).Build();
            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var subContractorRelationship = new SubContractorRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithSubContractor(subContractor).WithContractor(internalOrganisation).Build();
            this.Derive();

            Assert.Contains(subContractor, internalOrganisation.ActiveSubContractors);

            subContractorRelationship.ThroughDate = subContractorRelationship.FromDate;
            this.Derive();

            Assert.DoesNotContain(subContractor, internalOrganisation.ActiveSubContractors);
        }

        [Fact]
        public void ChangedOrganisationContactRelationshipEmployerDeriveCurrentOrganisationContactRelationships()
        {
            var contact = new PersonBuilder(this.Transaction).Build();
            var contactRelationship = new OrganisationContactRelationshipBuilder(this.Transaction).WithContact(contact).WithFromDate(this.Transaction.Now()).Build();
            this.Derive();

            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            contactRelationship.Organisation = organisation;
            this.Derive();

            Assert.Contains(contactRelationship, organisation.CurrentOrganisationContactRelationships);
        }

        [Fact]
        public void ChangedOrganisationContactRelationshipFromDateDeriveCurrentOrganisationContactRelationships()
        {
            var contact = new PersonBuilder(this.Transaction).Build();
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            var contactRelationship = new OrganisationContactRelationshipBuilder(this.Transaction).WithContact(contact).WithOrganisation(organisation).Build();
            this.Derive();

            contactRelationship.FromDate = this.Transaction.Now();
            this.Derive();

            Assert.Contains(contactRelationship, organisation.CurrentOrganisationContactRelationships);
        }

        [Fact]
        public void ChangedOrganisationContactRelationshipThroughDateDeriveCurrentOrganisationContactRelationships()
        {
            var contact = new PersonBuilder(this.Transaction).Build();
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            var contactRelationship = new OrganisationContactRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithContact(contact).WithOrganisation(organisation).Build();
            this.Derive();

            Assert.Contains(contactRelationship, organisation.CurrentOrganisationContactRelationships);

            contactRelationship.ThroughDate = contactRelationship.FromDate;
            this.Derive();

            Assert.DoesNotContain(contactRelationship, organisation.CurrentOrganisationContactRelationships);
        }

        [Fact]
        public void ChangedOrganisationContactRelationshipEmployerDeriveCurrentContacts()
        {
            var contact = new PersonBuilder(this.Transaction).Build();
            var contactRelationship = new OrganisationContactRelationshipBuilder(this.Transaction).WithContact(contact).WithFromDate(this.Transaction.Now()).Build();
            this.Derive();

            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            contactRelationship.Organisation = organisation;
            this.Derive();

            Assert.Contains(contact, organisation.CurrentContacts);
        }

        [Fact]
        public void ChangedOrganisationContactRelationshipFromDateDeriveCurrentContacts()
        {
            var contact = new PersonBuilder(this.Transaction).Build();
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            var contactRelationship = new OrganisationContactRelationshipBuilder(this.Transaction).WithContact(contact).WithOrganisation(organisation).Build();
            this.Derive();

            contactRelationship.FromDate = this.Transaction.Now();
            this.Derive();

            Assert.Contains(contact, organisation.CurrentContacts);
        }

        [Fact]
        public void ChangedOrganisationContactRelationshipThroughDateDeriveCurrentContacts()
        {
            var contact = new PersonBuilder(this.Transaction).Build();
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            var contactRelationship = new OrganisationContactRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithContact(contact).WithOrganisation(organisation).Build();
            this.Derive();

            Assert.Contains(contact, organisation.CurrentContacts);

            contactRelationship.ThroughDate = contactRelationship.FromDate;
            this.Derive();

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
            this.Derive();

            Assert.DoesNotContain(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeIsInternalOrganisationDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            organisation.IsInternalOrganisation = true;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeExternalAccountingTransactionFromPartyDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var accountingTransaction = new SalesAccountingTransactionBuilder(this.Transaction).Build();
            this.Derive();

            accountingTransaction.FromParty = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeExternalAccountingTransactionToPartyDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var accountingTransaction = new SalesAccountingTransactionBuilder(this.Transaction).Build();
            this.Derive();

            accountingTransaction.ToParty = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeShipmentShipFromPartyDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var shipment = new TransferBuilder(this.Transaction).Build();
            this.Derive();

            shipment.ShipFromParty = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeShipmentShipToPartyDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var shipment = new TransferBuilder(this.Transaction).Build();
            this.Derive();

            shipment.ShipToParty = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePaymentReceiverDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var payment = new ReceiptBuilder(this.Transaction).Build();
            this.Derive();

            payment.Receiver = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePaymentSenderDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var payment = new ReceiptBuilder(this.Transaction).Build();
            this.Derive();

            payment.Sender = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeEmploymentEmployerDeriveDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var employment = new EmploymentBuilder(this.Transaction).Build();
            this.Derive();

            employment.Employer = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeEngagementBillToPartyDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var engagement = new EngagementBuilder(this.Transaction).Build();
            this.Derive();

            engagement.BillToParty = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeEngagementPlacingPartyDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var engagement = new EngagementBuilder(this.Transaction).Build();
            this.Derive();

            engagement.PlacingParty = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePartManufacturedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var part = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            part.ManufacturedBy = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePartSuppliedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var part = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            part.AddSuppliedBy(organisation);
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationGlAccountInternalOrganisationDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var organisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction).Build();
            this.Derive();

            organisationGlAccount.InternalOrganisation = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeOrganisationRollUpParentDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var organisationRollUp = new OrganisationRollUpBuilder(this.Transaction).Build();
            this.Derive();

            organisationRollUp.Parent = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePartyFixedAssetAssignmentPartyDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var partyFixedAssetAssignment = new PartyFixedAssetAssignmentBuilder(this.Transaction).Build();
            this.Derive();

            partyFixedAssetAssignment.Party = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePickListShipToPartyDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var pickList = new PickListBuilder(this.Transaction).Build();
            this.Derive();

            pickList.ShipToParty = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeQuoteIssuerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var quote = new ProposalBuilder(this.Transaction).Build();
            this.Derive();

            quote.Issuer = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeQuoteReceiverDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var quote = new ProposalBuilder(this.Transaction).Build();
            this.Derive();

            quote.Receiver = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePurchaseInvoiceBilledToDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            purchaseInvoice.BilledTo = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePurchaseInvoiceBilledFromDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            purchaseInvoice.BilledFrom = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePurchaseInvoiceShipToCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            purchaseInvoice.ShipToCustomer = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePurchaseInvoiceBillToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            purchaseInvoice.BillToEndCustomer = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePurchaseInvoiceShipToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            purchaseInvoice.ShipToEndCustomer = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePurchaseOrderTakenViaSupplierDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var purchaseOrder = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            purchaseOrder.TakenViaSupplier = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangePurchaseOrderTakenViaSubcontractorDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var purchaseOrder = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            purchaseOrder.TakenViaSubcontractor = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeRequestOriginatorDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Derive();

            request.Originator = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeRequestRecipientDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Derive();

            request.Recipient = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeRequirementAuthorizerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var requirement = new RequirementBuilder(this.Transaction).Build();
            this.Derive();

            requirement.Authorizer = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeRequirementNeededForDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var requirement = new RequirementBuilder(this.Transaction).Build();
            this.Derive();

            requirement.NeededFor = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeRequirementOriginatorDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var requirement = new RequirementBuilder(this.Transaction).Build();
            this.Derive();

            requirement.Originator = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeRequirementServicedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var requirement = new RequirementBuilder(this.Transaction).Build();
            this.Derive();

            requirement.ServicedBy = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesInvoiceBilledFromDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            salesInvoice.BilledFrom = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesInvoiceBillToCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            salesInvoice.BillToCustomer = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesInvoiceBillToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            salesInvoice.BillToEndCustomer = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesInvoiceShipToCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            salesInvoice.ShipToCustomer = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesInvoiceShipToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            salesInvoice.ShipToEndCustomer = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesOrderBillToCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            salesOrder.BillToCustomer = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesOrderBillToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            salesOrder.BillToEndCustomer = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesOrderShipToCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            salesOrder.ShipToCustomer = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesOrderShipToEndCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            salesOrder.ShipToEndCustomer = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesOrderPlacingCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            salesOrder.PlacingCustomer = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesOrderTakenByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            salesOrder.TakenBy = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, this.InternalOrganisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSalesOrderItemAssignedShipToPartyDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var salesOrderItem = new SalesOrderItemBuilder(this.Transaction).Build();

            salesOrderItem.AssignedShipToParty = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSerialisedItemSuppliedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            serialisedItem.AssignedSuppliedBy = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSerialisedItemOwnedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            serialisedItem.OwnedBy = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSerialisedItemRentedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            serialisedItem.RentedBy = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSerialisedItemBuyerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            serialisedItem.Buyer = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeSerialisedItemSellerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            serialisedItem.Seller = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeWorkTaskCustomerDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            workTask.Customer = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeWorkTaskExecutedByDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            workTask.ExecutedBy = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }

        [Fact]
        public void OnChangeWorkEffortPartyAssignmentPartyDeletePermission()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            this.Derive();

            var partyAssignment = new WorkEffortPartyAssignmentBuilder(this.Transaction).Build();
            this.Derive();

            partyAssignment.Party = organisation;
            this.Derive();

            Assert.Contains(this.deletePermission, organisation.DeniedPermissions);
        }
    }
}
