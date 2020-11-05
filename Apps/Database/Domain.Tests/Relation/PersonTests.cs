// <copyright file="PersonTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Domain
{
    using System.Linq;
    using Allors.Domain.TestPopulation;
    using Xunit;

    public class PersonTests : DomainTest, IClassFixture<Fixture>
    {
        public PersonTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPerson_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new PersonBuilder(this.Session);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenPerson_WhenEmployed_ThenIsEmployeeEqualsTrue()
        {
            var employment = new EmploymentBuilder(this.Session)
                .WithEmployee(this.Purchaser)
                .WithFromDate(this.Session.Now())
                .Build();

            this.Session.Derive();

            Assert.True(this.Purchaser.AppsIsActiveEmployee(this.Session.Now()));
        }

        [Fact]
        public void GivenPerson_WhenActiveContactRelationship_ThenPersonCurrentOrganisationContactRelationshipsContainsPerson()
        {
            var contact = new PersonBuilder(this.Session).WithLastName("organisationContact").Build();
            var organisation = new OrganisationBuilder(this.Session).WithName("organisation").Build();

            new OrganisationContactRelationshipBuilder(this.Session)
                .WithContact(contact)
                .WithOrganisation(organisation)
                .WithFromDate(this.Session.Now().Date)
                .Build();

            this.Session.Derive();

            Assert.Equal(contact, contact.CurrentOrganisationContactRelationships[0].Contact);
            Assert.Empty(contact.InactiveOrganisationContactRelationships);
        }

        [Fact]
        public void GivenPerson_WhenInActiveContactRelationship_ThenPersonInactiveOrganisationContactRelationshipsContainsPerson()
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

            Assert.Equal(contact, contact.InactiveOrganisationContactRelationships[0].Contact);
            Assert.Empty(contact.CurrentOrganisationContactRelationships);
        }

        [Fact]
        public void GivenPerson_WhenEmployed_ThenTimeSheetSynced()
        {
            var person = new PersonBuilder(this.Session).WithFirstName("Good").WithLastName("Employee").Build();
            var employer = new InternalOrganisations(this.Session).Extent().First;

            var employment = new EmploymentBuilder(this.Session)
                .WithEmployee(person)
                .WithFromDate(this.Session.Now())
                .Build();

            this.Session.Derive();

            Assert.NotNull(person.TimeSheetWhereWorker);
        }

        [Fact]
        public void GivenPerson_WhenSubContractor_ThenTimeSheetSynced()
        {
            var subContractor = this.InternalOrganisation.CreateSubContractor(this.Session.Faker());

            this.Session.Derive();
            this.Session.Commit();

            var organisationContactRelationship = subContractor.OrganisationContactRelationshipsWhereOrganisation.First();
            var contact = organisationContactRelationship.Contact;

            Assert.NotNull(contact.TimeSheetWhereWorker);
        }

        [Fact]
        public void GivenPerson_WhenInContactRelationship_ThenCurrentOrganisationContactMechanismsIsDerived()
        {
            var contact = new PersonBuilder(this.Session).WithLastName("organisationContact").Build();
            var organisation1 = new OrganisationBuilder(this.Session).WithName("organisation1").Build();
            var organisation2 = new OrganisationBuilder(this.Session).WithName("organisation2").Build();

            // Even when relationship is inactive CurrentOrganisationContactMechanisms is maintained
            new OrganisationContactRelationshipBuilder(this.Session)
                .WithContact(contact)
                .WithOrganisation(organisation1)
                .WithFromDate(this.Session.Now().Date.AddDays(-1))
                .WithThroughDate(this.Session.Now().Date.AddDays(-1))
                .Build();

            var contactMechanism1 = new TelecommunicationsNumberBuilder(this.Session).WithAreaCode("111").WithContactNumber("222").Build();
            var partyContactMechanism1 = new PartyContactMechanismBuilder(this.Session).WithContactMechanism(contactMechanism1).Build();
            organisation1.AddPartyContactMechanism(partyContactMechanism1);

            this.Session.Derive();

            Assert.Single(contact.CurrentOrganisationContactMechanisms);
            Assert.Contains(contactMechanism1, contact.CurrentOrganisationContactMechanisms);

            partyContactMechanism1.ThroughDate = partyContactMechanism1.FromDate;

            this.Session.Derive();

            Assert.Empty(contact.CurrentOrganisationContactMechanisms);

            partyContactMechanism1.RemoveThroughDate();

            new OrganisationContactRelationshipBuilder(this.Session)
                .WithContact(contact)
                .WithOrganisation(organisation2)
                .WithFromDate(this.Session.Now().Date.AddDays(-1))
                .Build();

            var contactMechanism2 = new TelecommunicationsNumberBuilder(this.Session).WithAreaCode("222").WithContactNumber("333").Build();
            var partyContactMechanism2 = new PartyContactMechanismBuilder(this.Session).WithContactMechanism(contactMechanism2).Build();
            organisation2.AddPartyContactMechanism(partyContactMechanism2);

            this.Session.Derive();

            Assert.Equal(2, contact.CurrentOrganisationContactMechanisms.Count);
            Assert.Contains(contactMechanism1, contact.CurrentOrganisationContactMechanisms);
            Assert.Contains(contactMechanism2, contact.CurrentOrganisationContactMechanisms);
        }
    }

    [Trait("Category", "Security")]
    public class PersonSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public PersonSecurityTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Session).Get(this.M.Person.ObjectType, this.M.Person.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void GivenLoggedUserIsAdministrator_WhenAccessingSingleton_ThenLoggedInUserIsGrantedAccess()
        {
            var existingAdministrator = this.Administrator;
            var secondAdministrator = new PersonBuilder(this.Session).WithLastName("second admin").Build();
            Assert.False(secondAdministrator.IsAdministrator());

            var internalOrganisation = this.InternalOrganisation;

            this.Session.Derive();

            User user = this.Administrator;
            this.Session.SetUser(user);

            var acl = new DatabaseAccessControlLists(existingAdministrator)[internalOrganisation];
            Assert.True(acl.CanRead(this.M.Organisation.Name));

            acl = new DatabaseAccessControlLists(existingAdministrator)[internalOrganisation];
            Assert.True(acl.CanWrite(this.M.Organisation.Name));

            var administrators = new UserGroups(this.Session).Administrators;
            administrators.AddMember(secondAdministrator);

            this.Session.Derive();

            Assert.True(secondAdministrator.IsAdministrator());

            acl = new DatabaseAccessControlLists(existingAdministrator)[internalOrganisation];
            Assert.True(acl.CanRead(this.M.Organisation.Name));

            acl = new DatabaseAccessControlLists(existingAdministrator)[internalOrganisation];
            Assert.True(acl.CanWrite(this.M.Organisation.Name));
        }

        [Fact]
        public void OnChangedPersonDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithTimeSheetWhereWorkerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var timeSheet = new TimeSheetBuilder(this.Session).WithWorker(person).Build();
            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithTimeSheetWhereWorkerWithTimeEntriesDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var yesterday = DateTimeFactory.CreateDateTime(this.Session.Now().AddDays(-1));
            var laterYesterday = DateTimeFactory.CreateDateTime(yesterday.AddHours(3));

            var timeEntry = new TimeEntryBuilder(this.Session)
                .WithRateType(new RateTypes(this.Session).StandardRate)
                .WithFromDate(yesterday)
                .WithThroughDate(laterYesterday)
                .WithTimeFrequency(new TimeFrequencies(this.Session).Day)
                .Build();

            var timeSheet = new TimeSheetBuilder(this.Session).WithWorker(person).Build();
            timeSheet.AddTimeEntry(timeEntry);
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithExternalAccountingTransactionsWhereFromPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var externalAccountingTransaction = new CreditLineBuilder(this.Session).WithFromParty(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithExternalAccountingTransactionsWhereToPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var externalAccountingTransaction = new CreditLineBuilder(this.Session).WithToParty(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithShipmentsWhereShipFromPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipment = new PurchaseShipmentBuilder(this.Session).WithShipFromParty(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

       [Fact]
        public void OnChangedPersonWithShipmentsWhereShipToPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipment = new PurchaseShipmentBuilder(this.Session).WithShipToParty(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithPaymentsWhereReceiverDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var receipt = new ReceiptBuilder(this.Session).WithReceiver(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

       [Fact]
        public void OnChangedPersonWithPaymentsWhereSenderDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var receipt = new ReceiptBuilder(this.Session).WithSender(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithEngagementsWhereBillToPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var engagement = new EngagementBuilder(this.Session).WithBillToParty(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
       [Fact]
        public void OnChangedPersonWithEngagementsWherePlacingPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var engagement = new EngagementBuilder(this.Session).WithPlacingParty(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

       [Fact]
        public void OnChangedPersonWithPartsWhereManufacturedByDeriveDeletePermission()
        {
            //PartsWhereManufacturedBy
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var unifiedGood = new UnifiedGoodBuilder(this.Session).WithManufacturedBy(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithPartsWhereSuppliedByDeriveDeletePermission()
        {
            //PartsWhereManufacturedBy
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var unifiedGood = new UnifiedGoodBuilder(this.Session).WithSuppliedBy(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithPartyFixedAssetAssignmentsWherePartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var partyFixedAssetAssignment = new PartyFixedAssetAssignmentBuilder(this.Session).WithParty(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithPickListsWhereShipToPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var pickList = new PickListBuilder(this.Session).WithShipToParty(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithQuotesWhereReceiverDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var quotes = new ProposalBuilder(this.Session).WithReceiver(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithPurchaseInvoicesWhereBilledFromDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var purchaseInvoice= new PurchaseInvoiceBuilder(this.Session).WithBilledFrom(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithPurchaseInvoicesWhereShipToCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Session).WithShipToCustomer(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithPurchaseInvoicesWhereBillToEndCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);
            
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Session).WithBillToEndCustomer(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithPurchaseInvoicesWhereShipToEndCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Session).WithShipToEndCustomer(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithPurchaseOrdersWhereTakenViaSupplierDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var purchaseOrder = new PurchaseOrderBuilder(this.Session).WithTakenViaSupplier(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithPurchaseOrdersWhereTakenViaSubcontractorDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var purchaseOrder = new PurchaseOrderBuilder(this.Session).WithTakenViaSubcontractor(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithRequestsWhereOriginatorDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var request = new RequestForQuoteBuilder(this.Session).WithOriginator(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithRequirementsWhereAuthorizerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var requirement = new RequirementBuilder(this.Session).WithAuthorizer(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithRequirementsWhereNeededForDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var requirement = new RequirementBuilder(this.Session).WithNeededFor(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithRequirementsWhereOriginatorDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var requirement = new RequirementBuilder(this.Session).WithOriginator(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithRequirementsWhereServicedByDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var requirement = new RequirementBuilder(this.Session).WithServicedBy(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithSalesInvoicesWhereBillToCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Session).WithBillToCustomer(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithSalesInvoicesWhereBillToEndCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Session).WithBillToEndCustomer(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithSalesInvoicesWhereShipToCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Session).WithShipToCustomer(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithSalesInvoicesWhereShipToEndCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Session).WithShipToEndCustomer(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithSalesOrdersWhereBillToCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Session).WithBillToCustomer(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithSalesOrdersWhereShipToCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Session).WithShipToCustomer(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithSalesOrdersWhereShipToEndCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Session).WithShipToEndCustomer(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithSalesOrdersWherePlacingCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Session).WithPlacingCustomer(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithSalesOrderItemsWhereAssignedShipToPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesOrderItem = new SalesOrderItemBuilder(this.Session).WithAssignedShipToParty(person).Build();
            var salesOrder = new SalesOrderBuilder(this.Session).WithSalesOrderItem(salesOrderItem).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithSerialisedItemsWhereSuppliedByDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Session).WithAssignedSuppliedBy(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithSerialisedItemsWhereOwnedByDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Session).WithOwnedBy(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithSerialisedItemsWhereRentedByDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Session).WithRentedBy(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithWorkEffortsWhereCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Session).WithCustomer(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithWorkEffortPartyAssignmentsWherePartyDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var workEffortPartyAssignment = new WorkEffortPartyAssignmentBuilder(this.Session).WithParty(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithCashesWherePersonResponsibleDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var cash = new CashBuilder(this.Session).WithPersonResponsible(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithCommunicationEventsWhereOwnerDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var communicationEvent = new EmailCommunicationBuilder(this.Session).WithOwner(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithEngagementItemsWhereCurrentAssignedProfessionalDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var engagementItem = new CustomEngagementItemBuilder(this.Session).WithCurrentAssignedProfessional(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithEmploymentsWhereEmployeeDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var engagementItem = new EmploymentBuilder(this.Session).WithEmployee(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithEngineeringChangesWhereAuthorizerDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build(); // CreatedDerivation initial state
            this.Session.Derive(false);

            var engineeringChange = new EngineeringChangeBuilder(this.Session).WithAuthorizer(person).Build();
            
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithEngineeringChangesWhereDesignerDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var engineeringChange = new EngineeringChangeBuilder(this.Session).WithDesigner(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithEngineeringChangesWhereRequestorDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var engineeringChange = new EngineeringChangeBuilder(this.Session).WithRequestor(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithEngineeringChangesWhereTesterDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var engineeringChange = new EngineeringChangeBuilder(this.Session).WithTester(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithEventRegistrationsWherePersonDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var eventRegistration = new EventRegistrationBuilder(this.Session).WithPerson(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithOwnCreditCardsWhereOwnerDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var eventRegistration = new OwnCreditCardBuilder(this.Session).WithOwner(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithPerformanceNotesWhereEmployeeDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var performanceNote = new PerformanceNoteBuilder(this.Session).WithEmployee(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithPerformanceNotesWhereGivenByManagerDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var performanceNote = new PerformanceNoteBuilder(this.Session).WithGivenByManager(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithPerformanceReviewsWhereEmployeeDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var performanceReview = new PerformanceReviewBuilder(this.Session).WithEmployee(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithPerformanceReviewsWhereManagerDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var performanceReview = new PerformanceReviewBuilder(this.Session).WithManager(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithPickListsWherePickerDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var pickList = new PickListBuilder(this.Session).WithPicker(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithPositionFulfillmentsWherePersonDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var positionFulfillment = new PositionFulfillmentBuilder(this.Session).WithPerson(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithProfessionalAssignmentsWhereProfessionalDeletePermission()
        {
            var person = new PersonBuilder(this.Session).Build();
            this.Session.Derive(false);

            var professionalAssignment = new ProfessionalAssignmentBuilder(this.Session).WithProfessional(person).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
    }
}
