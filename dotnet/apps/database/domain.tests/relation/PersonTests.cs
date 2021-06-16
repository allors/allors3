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

            Assert.False(this.Derive().HasErrors);
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

    public class PersonRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PersonRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSalutationDeriveGender()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            person.Salutation = new Salutations(this.Transaction).Mr;
            this.Derive();

            Assert.Equal(new GenderTypes(this.Transaction).Male, person.Gender);
        }

        [Fact]
        public void ChangedFirstNameDerivePartyName()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            person.FirstName = "firstname";
            this.Derive();

            Assert.Contains("firstname", person.PartyName);
        }

        [Fact]
        public void ChangedMiddleNameDerivePartyName()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            person.MiddleName = "middlename";
            this.Derive();

            Assert.Contains("middlename", person.PartyName);
        }

        [Fact]
        public void ChangedLastNameDerivePartyName()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            person.LastName = "lastname";
            this.Derive();

            Assert.Contains("lastname", person.PartyName);
        }

        [Fact]
        public void ChangedUserNameDerivePartyName()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            person.UserName = "username";
            this.Derive();

            Assert.Contains("username", person.PartyName);
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

            Assert.Contains(contactRelationship, contact.CurrentOrganisationContactRelationships);
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

            Assert.Contains(contactRelationship, contact.CurrentOrganisationContactRelationships);
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

            this.Derive();

            partyContactMechanism.FromDate = this.Transaction.Now();
            this.Derive();

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

            this.Derive();

            Assert.Contains(partyContactMechanism.ContactMechanism, contact.CurrentOrganisationContactMechanisms);

            partyContactMechanism.ThroughDate = partyContactMechanism.FromDate;
            this.Derive();

            Assert.DoesNotContain(partyContactMechanism.ContactMechanism, contact.CurrentOrganisationContactMechanisms);
        }

        [Fact]
        public void ChangedEmploymentFromDateCreateTimeSheet()
        {
            var contact = new PersonBuilder(this.Transaction).Build();
            new OrganisationContactRelationshipBuilder(this.Transaction).WithContact(contact).WithFromDate(this.Transaction.Now()).Build();
            this.Derive();

            new EmploymentBuilder(this.Transaction).Build();
            this.Derive();

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
    public class PersonDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PersonDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Transaction).Get(this.M.Person, this.M.Person.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedPersonDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedTimeSheetWorkerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var timeSheet = new TimeSheetBuilder(this.Transaction).Build();
            this.Derive();

            timeSheet.Worker = person;
            this.Derive();

            Assert.DoesNotContain(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedTimeSheetWhereWorkerWithTimeEntriesDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

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
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedExternalAccountingTransactionFromPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var externalAccountingTransaction = new CreditLineBuilder(this.Transaction).Build();
            this.Derive();

            externalAccountingTransaction.FromParty = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedExternalAccountingTransactionToPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var externalAccountingTransaction = new CreditLineBuilder(this.Transaction).Build();
            this.Derive();

            externalAccountingTransaction.ToParty = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedShipmentShipFromPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var shipment = new PurchaseShipmentBuilder(this.Transaction).Build();
            this.Derive();

            shipment.ShipFromParty = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

       [Fact]
        public void OnChangedShipmentShipToPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var shipment = new PurchaseShipmentBuilder(this.Transaction).Build();
            this.Derive();

            shipment.ShipToParty = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPaymentReceiverDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var receipt = new ReceiptBuilder(this.Transaction).Build();
            this.Derive();

            receipt.Receiver = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

       [Fact]
        public void OnChangedPaymentSenderDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var receipt = new ReceiptBuilder(this.Transaction).Build();
            this.Derive();

            receipt.Sender = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedEngagementBillToPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var engagement = new EngagementBuilder(this.Transaction).Build();
            this.Derive();

            engagement.BillToParty = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
       [Fact]
        public void OnChangedEngagementPlacingPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var engagement = new EngagementBuilder(this.Transaction).Build();
            this.Derive();

            engagement.PlacingParty = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

       [Fact]
        public void OnChangedPartManufacturedByDeriveDeletePermission()
        {
            //PartsWhereManufacturedBy
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            unifiedGood.ManufacturedBy = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPartSuppliedByDeriveDeletePermission()
        {
            //PartsWhereManufacturedBy
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            unifiedGood.AddSuppliedBy(person);
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPartyFixedAssetAssignmentPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var partyFixedAssetAssignment = new PartyFixedAssetAssignmentBuilder(this.Transaction).Build();
            this.Derive();

            partyFixedAssetAssignment.Party = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPickListShipToPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var pickList = new PickListBuilder(this.Transaction).Build();
            this.Derive();

            pickList.ShipToParty = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedQuoteReceiverDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var quote = new ProposalBuilder(this.Transaction).Build();
            this.Derive();

            quote.Receiver = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPurchaseInvoiceBilledFromDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var purchaseInvoice= new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            purchaseInvoice.BilledFrom = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPurchaseInvoiceShipToCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            purchaseInvoice.ShipToCustomer = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPurchaseInvoiceBillToEndCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();
            
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            purchaseInvoice.BillToEndCustomer = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPurchaseInvoiceShipToEndCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            purchaseInvoice.ShipToEndCustomer = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedRequestOriginatorDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Derive();

            request.Originator = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequirementAuthorizerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var requirement = new RequirementBuilder(this.Transaction).Build();
            this.Derive();

            requirement.Authorizer = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedRequirementNeededForDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var requirement = new RequirementBuilder(this.Transaction).Build();
            this.Derive();

            requirement.NeededFor = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequirementOriginatorDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var requirement = new RequirementBuilder(this.Transaction).Build();
            this.Derive();

            requirement.Originator = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedRequirementServicedByDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var requirement = new RequirementBuilder(this.Transaction).Build();
            this.Derive();

            requirement.ServicedBy = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesInvoiceBillToCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            salesInvoice.BillToCustomer = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesInvoiceBillToEndCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            salesInvoice.BillToEndCustomer = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesInvoiceShipToCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            salesInvoice.ShipToCustomer = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedSalesInvoiceShipToEndCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            salesInvoice.ShipToEndCustomer = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesOrderBillToCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            salesOrder.BillToCustomer = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesOrderShipToCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            salesOrder.ShipToCustomer = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesOrderShipToEndCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            salesOrder.ShipToEndCustomer = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedSalesOrderPlacingCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var salesOrder = new SalesOrderBuilder(this.Transaction).Build();
            this.Derive();

            salesOrder.PlacingCustomer = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedSalesOrderItemAssignedShipToPartyDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var salesOrderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            this.Derive();

            salesOrderItem.AssignedShipToParty = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedSerialisedItemSuppliedByDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            serialisedItem.AssignedSuppliedBy = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedSerialisedItemOwnedByDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            serialisedItem.OwnedBy = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedSerialisedItemRentedByDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            serialisedItem.RentedBy = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedWorkEffortCustomerDeriveDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            workEffort.Customer = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedWorkEffortPartyAssignmentPartyDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var workEffortPartyAssignment = new WorkEffortPartyAssignmentBuilder(this.Transaction).Build();
            this.Derive();

            workEffortPartyAssignment.Party = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedCashePersonResponsibleDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var cash = new CashBuilder(this.Transaction).Build();
            this.Derive();

            cash.PersonResponsible = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedCommunicationEventOwnerDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var communicationEvent = new EmailCommunicationBuilder(this.Transaction).Build();
            this.Derive();

            communicationEvent.Owner = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedEngagementItemCurrentAssignedProfessionalDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var engagementItem = new CustomEngagementItemBuilder(this.Transaction).Build();
            this.Derive();

            engagementItem.CurrentAssignedProfessional = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedEmploymentEmployeeDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var employment = new EmploymentBuilder(this.Transaction).Build();
            this.Derive();

            employment.Employee = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedEngineeringChangeAuthorizerDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var engineeringChange = new EngineeringChangeBuilder(this.Transaction).Build();
            this.Derive();

            engineeringChange.Authorizer = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedEngineeringChangeDesignerDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var engineeringChange = new EngineeringChangeBuilder(this.Transaction).WithDesigner(person).Build();
            this.Derive();

            engineeringChange.Designer = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedEngineeringChangeRequestorDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var engineeringChange = new EngineeringChangeBuilder(this.Transaction).Build();
            this.Derive();

            engineeringChange.Requestor = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedEngineeringChangeTesterDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var engineeringChange = new EngineeringChangeBuilder(this.Transaction).Build();
            this.Derive();

            engineeringChange.Tester = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedEventRegistrationPersonDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var eventRegistration = new EventRegistrationBuilder(this.Transaction).Build();
            this.Derive();

            eventRegistration.Person = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedOwnCreditCardOwnerDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var creditCard = new OwnCreditCardBuilder(this.Transaction).Build();
            this.Derive();

            creditCard.Owner = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPerformanceNoteEmployeeDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var performanceNote = new PerformanceNoteBuilder(this.Transaction).Build();
            this.Derive();

            performanceNote.Employee = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPerformanceNoteGivenByManagerDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var performanceNote = new PerformanceNoteBuilder(this.Transaction).Build();
            this.Derive();

            performanceNote.GivenByManager = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPerformanceReviewEmployeeDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var performanceReview = new PerformanceReviewBuilder(this.Transaction).Build();
            this.Derive();

            performanceReview.Employee = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPerformanceReviewManagerDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var performanceReview = new PerformanceReviewBuilder(this.Transaction).Build();
            this.Derive();

            performanceReview.Manager = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPickListPickerDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var pickList = new PickListBuilder(this.Transaction).Build();
            this.Derive();

            pickList.Picker = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedPositionFulfillmentPersonDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var positionFulfillment = new PositionFulfillmentBuilder(this.Transaction).Build();
            this.Derive();

            positionFulfillment.Person = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProfessionalAssignmentProfessionalDeletePermission()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Derive();

            var professionalAssignment = new ProfessionalAssignmentBuilder(this.Transaction).Build();
            this.Derive();

            professionalAssignment.Professional = person;
            this.Derive();

            Assert.Contains(this.deletePermission, person.DeniedPermissions);
        }
    }
}
