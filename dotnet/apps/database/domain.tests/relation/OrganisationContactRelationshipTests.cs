// <copyright file="OrganisationContactRelationshipTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class OrganisationContactRelationshipTests : DomainTest, IClassFixture<Fixture>
    {
        private OrganisationContactRelationship organisationContactRelationship;
        private Organisation organisation;
        private Person contact;

        public OrganisationContactRelationshipTests(Fixture fixture) : base(fixture)
        {
            this.organisation = (Organisation)this.InternalOrganisation.ActiveCustomers.FirstOrDefault(v => v.GetType().Name == typeof(Organisation).Name);
            this.organisationContactRelationship = this.organisation.OrganisationContactRelationshipsWhereOrganisation.FirstOrDefault();
            this.contact = this.organisationContactRelationship.Contact;

            this.Transaction.Derive();
            this.Transaction.Commit();
        }

        [Fact]
        public void GivenorganisationContactRelationship_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var contact = new PersonBuilder(this.Transaction).WithLastName("organisationContact").Build();
            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            var builder = new OrganisationContactRelationshipBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithContact(contact);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithOrganisation(new OrganisationBuilder(this.Transaction).WithName("organisation").WithLocale(this.Transaction.GetSingleton().DefaultLocale).Build());
            builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenPerson_WhenFirstContactForOrganisationIsCreated_ThenContactUserGroupIsCreated()
        {
            this.InstantiateObjects(this.Transaction);

            var usergroup = this.organisationContactRelationship.Organisation.ContactsUserGroup;
            Assert.Contains(this.organisationContactRelationship.Contact, usergroup.Members);
        }

        [Fact]
        public void GivenNextPerson_WhenContactForOrganisationIsCreated_ThenContactIsAddedToUserGroup()
        {
            this.InstantiateObjects(this.Transaction);

            var usergroup = this.organisationContactRelationship.Organisation.ContactsUserGroup;
            Assert.Single(usergroup.Members);
            Assert.Contains(this.organisationContactRelationship.Contact, usergroup.Members);

            var secondRelationship = new OrganisationContactRelationshipBuilder(this.Transaction)
                .WithContact(new PersonBuilder(this.Transaction).WithLastName("contact 2").Build())
                .WithOrganisation(this.organisation)
                .WithFromDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(2, usergroup.Members.Count);
            Assert.Contains(secondRelationship.Contact, usergroup.Members);
        }

        [Fact]
        public void GivenOrganisationContactRelationship_WhenRelationshipPeriodIsNotValid_ThenContactIsNotInCustomerContactUserGroup()
        {
            this.InstantiateObjects(this.Transaction);

            var usergroup = this.organisationContactRelationship.Organisation.ContactsUserGroup;

            Assert.Single(usergroup.Members);
            Assert.Contains(this.contact, usergroup.Members);

            this.organisationContactRelationship.FromDate = this.Transaction.Now().AddDays(+1);
            this.organisationContactRelationship.RemoveThroughDate();

            this.Transaction.Derive();

            Assert.Empty(usergroup.Members);

            this.organisationContactRelationship.FromDate = this.Transaction.Now();
            this.organisationContactRelationship.RemoveThroughDate();

            this.Transaction.Derive();

            Assert.Single(usergroup.Members);
            Assert.Contains(this.contact, usergroup.Members);

            this.organisationContactRelationship.FromDate = this.Transaction.Now().AddDays(-2);
            this.organisationContactRelationship.ThroughDate = this.Transaction.Now().AddDays(-1);

            this.Transaction.Derive();

            Assert.Empty(usergroup.Members);
        }

        private void InstantiateObjects(ITransaction transaction)
        {
            this.contact = (Person)transaction.Instantiate(this.contact);
            this.organisationContactRelationship = (OrganisationContactRelationship)transaction.Instantiate(this.organisationContactRelationship);
        }
    }

    public class OrganisationContactRelationshipDateRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public OrganisationContactRelationshipDateRuleTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void ChangedFromDateDeriveContactsUserGroupMembers()
        {
            var contact = new PersonBuilder(this.Transaction).Build();

            new OrganisationContactRelationshipBuilder(this.Transaction)
                .WithContact(contact)
                .WithOrganisation(this.InternalOrganisation)
                .Build();
            this.Derive();

            Assert.Contains(contact, this.InternalOrganisation.ContactsUserGroup.Members);
        }

        [Fact]
        public void ChangedThroughDateDeriveContactsUserGroupMembers()
        {
            var contact = new PersonBuilder(this.Transaction).Build();

            var organisationContactRelationship = new OrganisationContactRelationshipBuilder(this.Transaction)
                .WithContact(contact)
                .WithOrganisation(this.InternalOrganisation)
                .Build();
            this.Derive();

            Assert.Contains(contact, this.InternalOrganisation.ContactsUserGroup.Members);

            organisationContactRelationship.ThroughDate = organisationContactRelationship.FromDate;
            this.Derive();

            Assert.DoesNotContain(contact, this.InternalOrganisation.ContactsUserGroup.Members);
        }

        [Fact]
        public void ChangedContactDeriveContactsUserGroupMembers()
        {
            var contact = new PersonBuilder(this.Transaction).Build();

            var organisationContactRelationship = new OrganisationContactRelationshipBuilder(this.Transaction)
                .WithOrganisation(this.InternalOrganisation)
                .Build();
            this.Derive();

            organisationContactRelationship.Contact = contact;
            this.Derive();

            Assert.Contains(contact, this.InternalOrganisation.ContactsUserGroup.Members);
        }
    }

    public class OrganisationContactRelationshipPartyRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public OrganisationContactRelationshipPartyRuleTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void ChangedContactDeriveParties()
        {
            var contact = new PersonBuilder(this.Transaction).Build();

            var organisationContactRelationship = new OrganisationContactRelationshipBuilder(this.Transaction).Build();
            this.Derive();

            organisationContactRelationship.Contact = contact;
            this.Derive();

            Assert.Contains(contact, organisationContactRelationship.Parties);
        }

        [Fact]
        public void ChangedOrganisationDeriveParties()
        {
            var organisation = new OrganisationBuilder(this.Transaction).Build();

            var organisationContactRelationship = new OrganisationContactRelationshipBuilder(this.Transaction).Build();
            this.Derive();

            organisationContactRelationship.Organisation = organisation;
            this.Derive();

            Assert.Contains(organisation, organisationContactRelationship.Parties);
        }
    }
}
