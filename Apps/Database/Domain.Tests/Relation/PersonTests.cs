// <copyright file="PersonTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using TestPopulation;
    using Xunit;

    public class PersonTests : DomainTest, IClassFixture<Fixture>
    {
        public PersonTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPerson_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new PersonBuilder(this.Transaction);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenPerson_WhenEmployed_ThenIsEmployeeEqualsTrue()
        {
            var employment = new EmploymentBuilder(this.Transaction)
                .WithEmployee(this.Purchaser)
                .WithFromDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            Assert.True(this.Purchaser.AppsIsActiveEmployee(this.Transaction.Now()));
        }

        [Fact]
        public void GivenPerson_WhenActiveContactRelationship_ThenPersonCurrentOrganisationContactRelationshipsContainsPerson()
        {
            var contact = new PersonBuilder(this.Transaction).WithLastName("organisationContact").Build();
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").Build();

            new OrganisationContactRelationshipBuilder(this.Transaction)
                .WithContact(contact)
                .WithOrganisation(organisation)
                .WithFromDate(this.Transaction.Now().Date)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(contact, contact.CurrentOrganisationContactRelationships[0].Contact);
            Assert.Empty(contact.InactiveOrganisationContactRelationships);
        }

        [Fact]
        public void GivenPerson_WhenInActiveContactRelationship_ThenPersonInactiveOrganisationContactRelationshipsContainsPerson()
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

            Assert.Equal(contact, contact.InactiveOrganisationContactRelationships[0].Contact);
            Assert.Empty(contact.CurrentOrganisationContactRelationships);
        }

        [Fact]
        public void GivenPerson_WhenEmployed_ThenTimeSheetSynced()
        {
            var person = new PersonBuilder(this.Transaction).WithFirstName("Good").WithLastName("Employee").Build();
            var employer = new InternalOrganisations(this.Transaction).Extent().First;

            var employment = new EmploymentBuilder(this.Transaction)
                .WithEmployee(person)
                .WithFromDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            Assert.NotNull(person.TimeSheetWhereWorker);
        }

        [Fact]
        public void GivenPerson_WhenSubContractor_ThenTimeSheetSynced()
        {
            var subContractor = this.InternalOrganisation.CreateSubContractor(this.Transaction.Faker());

            this.Transaction.Derive();
            this.Transaction.Commit();

            var organisationContactRelationship = subContractor.OrganisationContactRelationshipsWhereOrganisation.First();
            var contact = organisationContactRelationship.Contact;

            Assert.NotNull(contact.TimeSheetWhereWorker);
        }

        [Fact]
        public void GivenPerson_WhenInContactRelationship_ThenCurrentOrganisationContactMechanismsIsDerived()
        {
            var contact = new PersonBuilder(this.Transaction).WithLastName("organisationContact").Build();
            var organisation1 = new OrganisationBuilder(this.Transaction).WithName("organisation1").Build();
            var organisation2 = new OrganisationBuilder(this.Transaction).WithName("organisation2").Build();

            new OrganisationContactRelationshipBuilder(this.Transaction)
                .WithContact(contact)
                .WithOrganisation(organisation1)
                .WithFromDate(this.Transaction.Now().Date.AddDays(-1))
                .Build();

            var contactMechanism1 = new TelecommunicationsNumberBuilder(this.Transaction).WithAreaCode("111").WithContactNumber("222").Build();
            var partyContactMechanism1 = new PartyContactMechanismBuilder(this.Transaction).WithContactMechanism(contactMechanism1).Build();
            organisation1.AddPartyContactMechanism(partyContactMechanism1);

            this.Transaction.Derive();

            Assert.Single(contact.CurrentOrganisationContactMechanisms);
            Assert.Contains(contactMechanism1, contact.CurrentOrganisationContactMechanisms);

            partyContactMechanism1.ThroughDate = partyContactMechanism1.FromDate;

            this.Transaction.Derive();

            Assert.Empty(contact.CurrentOrganisationContactMechanisms);

            partyContactMechanism1.RemoveThroughDate();

            new OrganisationContactRelationshipBuilder(this.Transaction)
                .WithContact(contact)
                .WithOrganisation(organisation2)
                .WithFromDate(this.Transaction.Now().Date.AddDays(-1))
                .Build();

            var contactMechanism2 = new TelecommunicationsNumberBuilder(this.Transaction).WithAreaCode("222").WithContactNumber("333").Build();
            var partyContactMechanism2 = new PartyContactMechanismBuilder(this.Transaction).WithContactMechanism(contactMechanism2).Build();
            organisation2.AddPartyContactMechanism(partyContactMechanism2);

            this.Transaction.Derive();

            Assert.Equal(2, contact.CurrentOrganisationContactMechanisms.Count);
            Assert.Contains(contactMechanism1, contact.CurrentOrganisationContactMechanisms);
            Assert.Contains(contactMechanism2, contact.CurrentOrganisationContactMechanisms);
        }
    }

    public class PersonDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PersonDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSalutationDeriveGender()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            person.Salutation = new Salutations(this.Transaction).Mr;
            this.Transaction.Derive(false);

            Assert.Equal(new GenderTypes(this.Transaction).Male, person.Gender);
        }

        [Fact]
        public void ChangedFirstNameDerivePartyName()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            person.FirstName = "firstname";
            this.Transaction.Derive(false);

            Assert.Contains("firstname", person.PartyName);
        }

        [Fact]
        public void ChangedMiddleNameDerivePartyName()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            person.MiddleName = "middlename";
            this.Transaction.Derive(false);

            Assert.Contains("middlename", person.PartyName);
        }

        [Fact]
        public void ChangedLastNameDerivePartyName()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            person.LastName = "lastname";
            this.Transaction.Derive(false);

            Assert.Contains("lastname", person.PartyName);
        }

        [Fact]
        public void ChangedUserNameDerivePartyName()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            person.UserName = "username";
            this.Transaction.Derive(false);

            Assert.Contains("username", person.PartyName);
        }

        [Fact]
        public void ChangedDerivationTriggerDeriveVatRegime()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(new VatRegimes(this.Transaction).PrivatePerson, person.VatRegime);
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

            Assert.Contains(contactRelationship, contact.CurrentOrganisationContactRelationships);
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

            Assert.Contains(contactRelationship, contact.CurrentOrganisationContactRelationships);
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

            Assert.DoesNotContain(contactRelationship, contact.CurrentOrganisationContactRelationships);
        }

        [Fact]
        public void ChangedPartyContactMechanismFromDateDeriveCurrentOrganisationContactMechanisms()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            var contact = new PersonBuilder(this.Transaction).Build();
            new OrganisationContactRelationshipBuilder(this.Transaction).WithOrganisation(organisation).WithContact(contact).Build();

            var partyContactMechanism = new PartyContactMechanismBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithContactMechanism(new EmailAddressBuilder(this.Transaction).Build()).Build();
            organisation.AddPartyContactMechanism(partyContactMechanism);

            this.Transaction.Derive(false);

            partyContactMechanism.FromDate = this.Transaction.Now();
            this.Transaction.Derive(false);

            Assert.Contains(partyContactMechanism.ContactMechanism, contact.CurrentOrganisationContactMechanisms);
        }

        [Fact]
        public void ChangedPartyContactMechanismThroughDateDeriveCurrentOrganisationContactMechanisms()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();
            var contact = new PersonBuilder(this.Transaction).Build();
            new OrganisationContactRelationshipBuilder(this.Transaction).WithOrganisation(organisation).WithContact(contact).Build();

            var partyContactMechanism = new PartyContactMechanismBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithContactMechanism(new EmailAddressBuilder(this.Transaction).Build()).Build();
            organisation.AddPartyContactMechanism(partyContactMechanism);

            this.Transaction.Derive(false);

            Assert.Contains(partyContactMechanism.ContactMechanism, contact.CurrentOrganisationContactMechanisms);

            partyContactMechanism.ThroughDate = partyContactMechanism.FromDate;
            this.Transaction.Derive(false);

            Assert.DoesNotContain(partyContactMechanism.ContactMechanism, contact.CurrentOrganisationContactMechanisms);
        }

        [Fact]
        public void ChangedEmploymentFromDateCreateTimeSheet()
        {
            var contact = new PersonBuilder(this.Transaction).Build();
            new OrganisationContactRelationshipBuilder(this.Transaction).WithContact(contact).WithFromDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            new EmploymentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.True(contact.ExistTimeSheetWhereWorker);
        }
    }

    [Trait("Category", "Security")]
    public class PersonSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public PersonSecurityTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Transaction).Get(this.M.Person.ObjectType, this.M.Person.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void GivenLoggedUserIsAdministrator_WhenAccessingSingleton_ThenLoggedInUserIsGrantedAccess()
        {
            var existingAdministrator = this.Administrator;
            var secondAdministrator = new PersonBuilder(this.Transaction).WithLastName("second admin").Build();
            Assert.False(secondAdministrator.IsAdministrator());

            var internalOrganisation = this.InternalOrganisation;

            this.Transaction.Derive();

            User user = this.Administrator;
            this.Transaction.SetUser(user);

            var acl = new DatabaseAccessControlLists(existingAdministrator)[internalOrganisation];
            Assert.True(acl.CanRead(this.M.Organisation.Name));

            acl = new DatabaseAccessControlLists(existingAdministrator)[internalOrganisation];
            Assert.True(acl.CanWrite(this.M.Organisation.Name));

            var administrators = new UserGroups(this.Transaction).Administrators;
            administrators.AddMember(secondAdministrator);

            this.Transaction.Derive();

            Assert.True(secondAdministrator.IsAdministrator());

            acl = new DatabaseAccessControlLists(existingAdministrator)[internalOrganisation];
            Assert.True(acl.CanRead(this.M.Organisation.Name));

            acl = new DatabaseAccessControlLists(existingAdministrator)[internalOrganisation];
            Assert.True(acl.CanWrite(this.M.Organisation.Name));
        }
    }

    [Trait("Category", "Security")]
    public class PersonDeniedPermissionDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PersonDeniedPermissionDerivationTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Transaction).Get(this.M.Person.ObjectType, this.M.Person.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedPersonDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithTimeSheetWhereWorkerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var timeSheet = new TimeSheetBuilder(this.Transaction).WithWorker(person).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithTimeSheetWhereWorkerWithTimeEntriesDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var yesterday = DateTimeFactory.CreateDateTime(this.Transaction.Now().AddDays(-1));
            var laterYesterday = DateTimeFactory.CreateDateTime(yesterday.AddHours(3));

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(yesterday)
                .WithThroughDate(laterYesterday)
                .WithTimeFrequency(new TimeFrequencies(this.Transaction).Day)
                .Build();

            var timeSheet = new TimeSheetBuilder(this.Transaction).WithWorker(person).Build();
            timeSheet.AddTimeEntry(timeEntry);
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithExternalAccountingTransactionsWhereFromPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var externalAccountingTransaction = new CreditLineBuilder(this.Transaction).WithFromParty(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithExternalAccountingTransactionsWhereToPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var externalAccountingTransaction = new CreditLineBuilder(this.Transaction).WithToParty(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithShipmentsWhereShipFromPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var shipment = new PurchaseShipmentBuilder(this.Transaction).WithShipFromParty(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

       [Fact]
        public void OnChangedPersonWithShipmentsWhereShipToPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var shipment = new PurchaseShipmentBuilder(this.Transaction).WithShipToParty(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithPaymentsWhereReceiverDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var receipt = new ReceiptBuilder(this.Transaction).WithReceiver(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

       [Fact]
        public void OnChangedPersonWithPaymentsWhereSenderDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var receipt = new ReceiptBuilder(this.Transaction).WithSender(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithEngagementsWhereBillToPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var engagement = new EngagementBuilder(this.Transaction).WithBillToParty(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
       [Fact]
        public void OnChangedPersonWithEngagementsWherePlacingPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var engagement = new EngagementBuilder(this.Transaction).WithPlacingParty(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

       [Fact]
        public void OnChangedPersonWithPartsWhereManufacturedByDeriveDeletePermission()
        {
            //PartsWhereManufacturedBy
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).WithManufacturedBy(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithPartsWhereSuppliedByDeriveDeletePermission()
        {
            //PartsWhereManufacturedBy
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).WithSuppliedBy(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithPartyFixedAssetAssignmentsWherePartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var partyFixedAssetAssignment = new PartyFixedAssetAssignmentBuilder(this.Transaction).WithParty(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithPickListsWhereShipToPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var pickList = new PickListBuilder(this.Transaction).WithShipToParty(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithQuotesWhereReceiverDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var quotes = new ProposalBuilder(this.Transaction).WithReceiver(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithPurchaseInvoicesWhereBilledFromDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseInvoice= new PurchaseInvoiceBuilder(this.Transaction).WithBilledFrom(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithPurchaseInvoicesWhereShipToCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).WithShipToCustomer(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithPurchaseInvoicesWhereBillToEndCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);
            
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).WithBillToEndCustomer(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithPurchaseInvoicesWhereShipToEndCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).WithShipToEndCustomer(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithRequestsWhereOriginatorDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var request = new RequestForQuoteBuilder(this.Transaction).WithOriginator(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithRequirementsWhereAuthorizerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var requirement = new RequirementBuilder(this.Transaction).WithAuthorizer(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithRequirementsWhereNeededForDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var requirement = new RequirementBuilder(this.Transaction).WithNeededFor(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithRequirementsWhereOriginatorDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var requirement = new RequirementBuilder(this.Transaction).WithOriginator(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithRequirementsWhereServicedByDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var requirement = new RequirementBuilder(this.Transaction).WithServicedBy(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithSalesInvoicesWhereBillToCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithBillToCustomer(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithSalesInvoicesWhereBillToEndCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithBillToEndCustomer(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithSalesInvoicesWhereShipToCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithShipToCustomer(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithSalesInvoicesWhereShipToEndCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithShipToEndCustomer(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithSalesOrdersWhereBillToCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Transaction).WithBillToCustomer(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithSalesOrdersWhereShipToCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Transaction).WithShipToCustomer(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithSalesOrdersWhereShipToEndCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Transaction).WithShipToEndCustomer(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithSalesOrdersWherePlacingCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Transaction).WithPlacingCustomer(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithSalesOrderItemsWhereAssignedShipToPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrderItem = new SalesOrderItemBuilder(this.Transaction).WithAssignedShipToParty(person).Build();
            var salesOrder = new SalesOrderBuilder(this.Transaction).WithSalesOrderItem(salesOrderItem).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithSerialisedItemsWhereSuppliedByDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithAssignedSuppliedBy(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithSerialisedItemsWhereOwnedByDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithOwnedBy(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithSerialisedItemsWhereRentedByDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithRentedBy(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithWorkEffortsWhereCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).WithCustomer(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithWorkEffortPartyAssignmentsWherePartyDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workEffortPartyAssignment = new WorkEffortPartyAssignmentBuilder(this.Transaction).WithParty(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithCashesWherePersonResponsibleDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var cash = new CashBuilder(this.Transaction).WithPersonResponsible(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithCommunicationEventsWhereOwnerDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var communicationEvent = new EmailCommunicationBuilder(this.Transaction).WithOwner(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithEngagementItemsWhereCurrentAssignedProfessionalDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var engagementItem = new CustomEngagementItemBuilder(this.Transaction).WithCurrentAssignedProfessional(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithEmploymentsWhereEmployeeDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var engagementItem = new EmploymentBuilder(this.Transaction).WithEmployee(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        // TODO: Relooking in EngineeringChange
        [Fact]
        public void OnChangedPersonWithEngineeringChangesWhereAuthorizerDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var engineeringChange = new EngineeringChangeBuilder(this.Transaction).WithAuthorizer(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithEngineeringChangesWhereDesignerDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var engineeringChange = new EngineeringChangeBuilder(this.Transaction).WithDesigner(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithEngineeringChangesWhereRequestorDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var engineeringChange = new EngineeringChangeBuilder(this.Transaction).WithRequestor(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithEngineeringChangesWhereTesterDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var engineeringChange = new EngineeringChangeBuilder(this.Transaction).WithTester(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithEventRegistrationsWherePersonDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var eventRegistration = new EventRegistrationBuilder(this.Transaction).WithPerson(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithOwnCreditCardsWhereOwnerDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var eventRegistration = new OwnCreditCardBuilder(this.Transaction).WithOwner(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithPerformanceNotesWhereEmployeeDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var performanceNote = new PerformanceNoteBuilder(this.Transaction).WithEmployee(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithPerformanceNotesWhereGivenByManagerDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var performanceNote = new PerformanceNoteBuilder(this.Transaction).WithGivenByManager(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithPerformanceReviewsWhereEmployeeDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var performanceReview = new PerformanceReviewBuilder(this.Transaction).WithEmployee(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithPerformanceReviewsWhereManagerDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var performanceReview = new PerformanceReviewBuilder(this.Transaction).WithManager(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithPickListsWherePickerDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var pickList = new PickListBuilder(this.Transaction).WithPicker(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPersonWithPositionFulfillmentsWherePersonDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var positionFulfillment = new PositionFulfillmentBuilder(this.Transaction).WithPerson(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPersonWithProfessionalAssignmentsWhereProfessionalDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var professionalAssignment = new ProfessionalAssignmentBuilder(this.Transaction).WithProfessional(person).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
    }
}
