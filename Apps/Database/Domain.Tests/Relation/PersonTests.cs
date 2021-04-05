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
        public PersonSecurityTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Transaction).Get(this.M.Person, this.M.Person.Delete);

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
        public PersonDeniedPermissionDerivationTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Transaction).Get(this.M.Person, this.M.Person.Delete);

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
        public void OnChangedTimeSheetWorkerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var timeSheet = new TimeSheetBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            timeSheet.Worker = person;
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedTimeSheetWhereWorkerWithTimeEntriesDeriveDeletePermission()
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
        public void OnChangedExternalAccountingTransactionFromPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var externalAccountingTransaction = new CreditLineBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            externalAccountingTransaction.FromParty = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedExternalAccountingTransactionToPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var externalAccountingTransaction = new CreditLineBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            externalAccountingTransaction.ToParty = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedShipmentShipFromPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var shipment = new PurchaseShipmentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            shipment.ShipFromParty = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

       [Fact]
        public void OnChangedShipmentShipToPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var shipment = new PurchaseShipmentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            shipment.ShipToParty = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPaymentReceiverDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var receipt = new ReceiptBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            receipt.Receiver = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

       [Fact]
        public void OnChangedPaymentSenderDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var receipt = new ReceiptBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            receipt.Sender = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedEngagementBillToPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var engagement = new EngagementBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            engagement.BillToParty = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
       [Fact]
        public void OnChangedEngagementPlacingPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var engagement = new EngagementBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            engagement.PlacingParty = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

       [Fact]
        public void OnChangedPartManufacturedByDeriveDeletePermission()
        {
            //PartsWhereManufacturedBy
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            unifiedGood.ManufacturedBy = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPartSuppliedByDeriveDeletePermission()
        {
            //PartsWhereManufacturedBy
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            unifiedGood.AddSuppliedBy(person);
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPartyFixedAssetAssignmentPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var partyFixedAssetAssignment = new PartyFixedAssetAssignmentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            partyFixedAssetAssignment.Party = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPickListShipToPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var pickList = new PickListBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            pickList.ShipToParty = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedQuoteReceiverDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var quote = new ProposalBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            quote.Receiver = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPurchaseInvoiceBilledFromDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseInvoice= new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            purchaseInvoice.BilledFrom = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPurchaseInvoiceShipToCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            purchaseInvoice.ShipToCustomer = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPurchaseInvoiceBillToEndCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);
            
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            purchaseInvoice.BillToEndCustomer = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPurchaseInvoiceShipToEndCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            purchaseInvoice.ShipToEndCustomer = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedRequestOriginatorDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            request.Originator = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequirementAuthorizerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var requirement = new RequirementBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            requirement.Authorizer = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedRequirementNeededForDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var requirement = new RequirementBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            requirement.NeededFor = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequirementOriginatorDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var requirement = new RequirementBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            requirement.Originator = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedRequirementServicedByDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var requirement = new RequirementBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            requirement.ServicedBy = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesInvoiceBillToCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesInvoice.BillToCustomer = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesInvoiceBillToEndCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesInvoice.BillToEndCustomer = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesInvoiceShipToCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesInvoice.ShipToCustomer = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedSalesInvoiceShipToEndCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesInvoice.ShipToEndCustomer = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesOrderBillToCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesOrder.BillToCustomer = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesOrderShipToCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesOrder.ShipToCustomer = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesOrderShipToEndCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesOrder.ShipToEndCustomer = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedSalesOrderPlacingCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesOrder.PlacingCustomer = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedSalesOrderItemAssignedShipToPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesOrderItem.AssignedShipToParty = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedSerialisedItemSuppliedByDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            serialisedItem.AssignedSuppliedBy = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedSerialisedItemOwnedByDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            serialisedItem.OwnedBy = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedSerialisedItemRentedByDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            serialisedItem.RentedBy = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedWorkEffortCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            workEffort.Customer = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedWorkEffortPartyAssignmentPartyDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workEffortPartyAssignment = new WorkEffortPartyAssignmentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            workEffortPartyAssignment.Party = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedCashePersonResponsibleDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var cash = new CashBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            cash.PersonResponsible = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedCommunicationEventOwnerDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var communicationEvent = new EmailCommunicationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            communicationEvent.Owner = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedEngagementItemCurrentAssignedProfessionalDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var engagementItem = new CustomEngagementItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            engagementItem.CurrentAssignedProfessional = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedEmploymentEmployeeDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var employment = new EmploymentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            employment.Employee = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedEngineeringChangeAuthorizerDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var engineeringChange = new EngineeringChangeBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            engineeringChange.Authorizer = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedEngineeringChangeDesignerDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var engineeringChange = new EngineeringChangeBuilder(this.Transaction).WithDesigner(person).Build();
            this.Transaction.Derive(false);

            engineeringChange.Designer = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedEngineeringChangeRequestorDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var engineeringChange = new EngineeringChangeBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            engineeringChange.Requestor = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedEngineeringChangeTesterDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var engineeringChange = new EngineeringChangeBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            engineeringChange.Tester = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedEventRegistrationPersonDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var eventRegistration = new EventRegistrationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            eventRegistration.Person = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedOwnCreditCardOwnerDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var creditCard = new OwnCreditCardBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            creditCard.Owner = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPerformanceNoteEmployeeDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var performanceNote = new PerformanceNoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            performanceNote.Employee = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPerformanceNoteGivenByManagerDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var performanceNote = new PerformanceNoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            performanceNote.GivenByManager = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPerformanceReviewEmployeeDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var performanceReview = new PerformanceReviewBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            performanceReview.Employee = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPerformanceReviewManagerDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var performanceReview = new PerformanceReviewBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            performanceReview.Manager = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPickListPickerDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var pickList = new PickListBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            pickList.Picker = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPositionFulfillmentPersonDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var positionFulfillment = new PositionFulfillmentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            positionFulfillment.Person = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProfessionalAssignmentProfessionalDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var professionalAssignment = new ProfessionalAssignmentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            professionalAssignment.Professional = person;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
    }
}
