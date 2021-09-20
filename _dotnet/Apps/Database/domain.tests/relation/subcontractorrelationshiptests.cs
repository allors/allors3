// <copyright file="SubContractorRelationshipTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using TestPopulation;
    using System.Linq;
    using Xunit;

    public class SubContractorRelationshipTests : DomainTest, IClassFixture<Fixture>
    {
        private Person contact;
        private Organisation subcontractor;
        private SubContractorRelationship subContractorRelationship;
        private OrganisationContactRelationship organisationContactRelationship;

        public SubContractorRelationshipTests(Fixture fixture) : base(fixture)
        {
            this.subcontractor = this.InternalOrganisation.CreateSubContractor(this.Transaction.Faker());

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.subContractorRelationship = this.subcontractor.SubContractorRelationshipsWhereSubContractor.First();
            this.organisationContactRelationship = this.subcontractor.OrganisationContactRelationshipsWhereOrganisation.First();
            this.contact = this.organisationContactRelationship.Contact;
        }

        [Fact]
        public void GivenSubContractorRelationshipBuilder_WhenBuild_ThenSubAccountNumerIsValidElevenTestNumber()
        {
            this.InternalOrganisation.SettingsForAccounting.SubAccountCounter.Value = 1000;

            this.Transaction.Commit();

            var subcontractor1 = new OrganisationBuilder(this.Transaction).WithDefaults().Build();
            new SubContractorRelationshipBuilder(this.Transaction).WithSubContractor(subcontractor1).Build();

            this.Transaction.Derive();

            var subContractor1Financial = subcontractor1.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, this.InternalOrganisation));

            this.Transaction.Derive();

            Assert.Equal(1007, subContractor1Financial.SubAccountNumber);

            var subcontractor2 = new OrganisationBuilder(this.Transaction).WithDefaults().Build();
            new SubContractorRelationshipBuilder(this.Transaction).WithSubContractor(subcontractor2).Build();

            this.Transaction.Derive();

            var subContractor2Financial = subcontractor2.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, this.InternalOrganisation));

            this.Transaction.Derive();

            Assert.Equal(1015, subContractor2Financial.SubAccountNumber);

            var subcontractor3 = new OrganisationBuilder(this.Transaction).WithDefaults().Build();
            new SubContractorRelationshipBuilder(this.Transaction).WithSubContractor(subcontractor3).Build();

            this.Transaction.Derive();

            var subContractor3Financial = subcontractor3.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, this.InternalOrganisation));

            this.Transaction.Derive();

            Assert.Equal(1023, subContractor3Financial.SubAccountNumber);
        }

        [Fact]
        public void GivenSubContractorRelationship_WhenDeriving_ThenSubAccountNumberMustBeUniqueWithinInternalOrganisation()
        {
            var belgium = new Countries(this.Transaction).CountryByIsoCode["BE"];
            var euro = belgium.Currency;

            var bank = new BankBuilder(this.Transaction).WithCountry(belgium).WithName("ING België").WithBic("BBRUBEBB").Build();

            var ownBankAccount = new OwnBankAccountBuilder(this.Transaction)
                .WithDescription("BE23 3300 6167 6391")
                .WithBankAccount(new BankAccountBuilder(this.Transaction).WithBank(bank).WithCurrency(euro).WithIban("BE23 3300 6167 6391").WithNameOnAccount("Koen").Build())
                .Build();

            var internalOrganisation2 = new OrganisationBuilder(this.Transaction)
                .WithIsInternalOrganisation(true)
                .WithDoAccounting(true)
                .WithName("internalOrganisation2")
                .WithDefaultCollectionMethod(ownBankAccount)
                .Build();

            this.Derive();

            var subcontractor2 = internalOrganisation2.CreateSubContractor(this.Transaction.Faker());

            this.Transaction.Derive();

            var subContractorRelationship2 = subcontractor2.SubContractorRelationshipsWhereSubContractor.First();

            this.Transaction.Derive();

            var partyFinancial2 = subcontractor2.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, internalOrganisation2));

            partyFinancial2.SubAccountNumber = 19;

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenSubContractorRelationship_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new SubContractorRelationshipBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithSubContractor(this.subcontractor);
            builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenContractorOrganisation_WhenOrganisationContactRelationshipIsCreated_ThenPersonIsAddedToUserGroup()
        {
            this.InstantiateObjects(this.Transaction);

            Assert.Single(this.subContractorRelationship.SubContractor.ContactsUserGroup.Members);
            Assert.Contains(this.contact, this.subContractorRelationship.SubContractor.ContactsUserGroup.Members);
        }

        [Fact]
        public void GivenSubContractorRelationship_WhenRelationshipPeriodIsNotValid_ThenContactIsNotInContactsUserGroup()
        {
            this.InstantiateObjects(this.Transaction);

            Assert.Single(this.subContractorRelationship.SubContractor.ContactsUserGroup.Members);
            Assert.Contains(this.contact, this.subContractorRelationship.SubContractor.ContactsUserGroup.Members);

            this.organisationContactRelationship.FromDate = this.Transaction.Now().AddDays(+1);
            this.organisationContactRelationship.RemoveThroughDate();

            this.Transaction.Derive();

            Assert.Empty(this.subContractorRelationship.SubContractor.ContactsUserGroup.Members);

            this.organisationContactRelationship.FromDate = this.Transaction.Now().AddSeconds(-1);
            this.organisationContactRelationship.RemoveThroughDate();

            this.Transaction.Derive();

            Assert.Single(this.subContractorRelationship.SubContractor.ContactsUserGroup.Members);
            Assert.Contains(this.contact, this.subContractorRelationship.SubContractor.ContactsUserGroup.Members);

            this.organisationContactRelationship.FromDate = this.Transaction.Now().AddDays(-2);
            this.organisationContactRelationship.ThroughDate = this.Transaction.Now().AddDays(-1);

            this.Transaction.Derive();

            Assert.Empty(this.subContractorRelationship.SubContractor.ContactsUserGroup.Members);
        }

        [Fact]
        public void GivenSubContractorRelationship_WhenContactForOrganisationEnds_ThenContactIsRemovedfromContactsUserGroup()
        {
            this.InstantiateObjects(this.Transaction);

            var contact2 = new PersonBuilder(this.Transaction).WithLastName("contact2").Build();
            var contactRelationship2 = new OrganisationContactRelationshipBuilder(this.Transaction)
                .WithOrganisation(this.subcontractor)
                .WithContact(contact2)
                .WithFromDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(2, this.subContractorRelationship.SubContractor.ContactsUserGroup.Members.Count());
            Assert.Contains(this.contact, this.subContractorRelationship.SubContractor.ContactsUserGroup.Members);
            Assert.Contains(contact2, this.subContractorRelationship.SubContractor.ContactsUserGroup.Members);

            contactRelationship2.ThroughDate = this.Transaction.Now().AddDays(-1);

            this.Transaction.Derive();

            Assert.Single(this.subContractorRelationship.SubContractor.ContactsUserGroup.Members);
            Assert.Contains(this.contact, this.subContractorRelationship.SubContractor.ContactsUserGroup.Members);
            Assert.DoesNotContain(contact2, this.subContractorRelationship.SubContractor.ContactsUserGroup.Members);
        }

        [Fact]
        public void GivenActiveSubContractorRelationship_WhenDeriving_ThenInternalOrganisationSuppliersContainsSupplier() => Assert.Contains(this.subcontractor, this.InternalOrganisation.ActiveSubContractors);

        [Fact]
        public void GivenSubContractorRelationshipToCome_WhenDeriving_ThenInternalOrganisationSuppliersDosNotContainSupplier()
        {
            this.subContractorRelationship.FromDate = this.Transaction.Now().AddDays(1);
            this.Transaction.Derive();

            Assert.DoesNotContain(this.subcontractor, this.InternalOrganisation.ActiveSubContractors);
        }

        [Fact]
        public void GivenSubContractorRelationshipThatHasEnded_WhenDeriving_ThenInternalOrganisationSuppliersDosNotContainSupplier()
        {
            this.subContractorRelationship.FromDate = this.Transaction.Now().AddDays(-10);
            this.subContractorRelationship.ThroughDate = this.Transaction.Now().AddDays(-1);

            this.Transaction.Derive();

            Assert.DoesNotContain(this.subcontractor, this.InternalOrganisation.ActiveSubContractors);
        }

        private void InstantiateObjects(ITransaction transaction)
        {
            this.contact = (Person)transaction.Instantiate(this.contact);
            this.subcontractor = (Organisation)transaction.Instantiate(this.subcontractor);
            this.subContractorRelationship = (SubContractorRelationship)transaction.Instantiate(this.subContractorRelationship);
            this.organisationContactRelationship = (OrganisationContactRelationship)transaction.Instantiate(this.organisationContactRelationship);
        }
    }

    public class SubContractorRelationshipRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SubContractorRelationshipRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSubContractorDeriveParties()
        {
            var relationship = new SubContractorRelationshipBuilder(this.Transaction).Build();
            this.Derive();

            var supplier = new OrganisationBuilder(this.Transaction).Build();
            relationship.SubContractor = supplier;
            this.Derive();

            Assert.Contains(supplier, relationship.Parties);
        }

        [Fact]
        public void ChangedInternalOrganisationDeriveParties()
        {
            var relationship = new SubContractorRelationshipBuilder(this.Transaction).Build();
            this.Derive();

            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            relationship.Contractor = internalOrganisation;
            this.Derive();

            Assert.Contains(internalOrganisation, relationship.Parties);
        }
    }
}
