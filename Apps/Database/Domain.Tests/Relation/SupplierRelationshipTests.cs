// <copyright file="SupplierRelationshipTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class SupplierRelationshipTests : DomainTest, IClassFixture<Fixture>
    {
        private Person contact;
        private Organisation supplier;
        private SupplierRelationship supplierRelationship;
        private OrganisationContactRelationship organisationContactRelationship;

        public SupplierRelationshipTests(Fixture fixture) : base(fixture)
        {
            this.contact = new PersonBuilder(this.Transaction).WithLastName("contact").Build();
            this.supplier = new OrganisationBuilder(this.Transaction)
                .WithName("supplier")
                .WithLocale(new Locales(this.Transaction).EnglishGreatBritain)

                .Build();

            this.organisationContactRelationship = new OrganisationContactRelationshipBuilder(this.Transaction)
                .WithOrganisation(this.supplier)
                .WithContact(this.contact)
                .WithFromDate(this.Transaction.Now())
                .Build();

            this.supplierRelationship = new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(this.supplier)
                .WithInternalOrganisation(this.InternalOrganisation)
                .WithFromDate(this.Transaction.Now().AddYears(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();
        }

        [Fact]
        public void GivenSupplierRelationshipBuilder_WhenBuild_ThenSubAccountNumerIsValidElevenTestNumber()
        {
            this.InternalOrganisation.SettingsForAccounting.SubAccountCounter.Value = 1000;

            this.Transaction.Commit();

            var supplier1 = new OrganisationBuilder(this.Transaction).WithName("supplier1").Build();
            var supplierRelationship1 = new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier1).Build();

            this.Transaction.Derive();

            var partyFinancial1 = supplier1.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, supplierRelationship1.InternalOrganisation));

            this.Transaction.Derive();

            Assert.Equal(1007, partyFinancial1.SubAccountNumber);

            var supplier2 = new OrganisationBuilder(this.Transaction).WithName("supplier2").Build();
            var supplierRelationship2 = new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier2).Build();

            this.Transaction.Derive();

            var partyFinancial2 = supplier2.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, supplierRelationship2.InternalOrganisation));

            this.Transaction.Derive();

            Assert.Equal(1015, partyFinancial2.SubAccountNumber);

            var supplier3 = new OrganisationBuilder(this.Transaction).WithName("supplier3").Build();
            var supplierRelationship3 = new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier3).Build();

            this.Transaction.Derive();

            var partyFinancial3 = supplier3.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, supplierRelationship3.InternalOrganisation));

            this.Transaction.Derive();

            Assert.Equal(1023, partyFinancial3.SubAccountNumber);
        }

        [Fact]
        public void GivenSupplierRelationship_WhenDeriving_ThenSubAccountNumberMustBeUniqueWithinInternalOrganisation()
        {
            var supplier2 = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();

            this.Transaction.Derive();

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

            this.Transaction.Derive(false);

            var supplierRelationship2 = new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(supplier2)
                .WithInternalOrganisation(internalOrganisation2)
                .WithFromDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            var partyFinancial2 = supplier2.PartyFinancialRelationshipsWhereFinancialParty.First(v => Equals(v.InternalOrganisation, supplierRelationship2.InternalOrganisation));

            partyFinancial2.SubAccountNumber = 19;

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenSupplierRelationship_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            this.InstantiateObjects(this.Transaction);

            var builder = new SupplierRelationshipBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithSupplier(this.supplier);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenSupplierOrganisation_WhenOrganisationContactRelationshipIsCreated_ThenPersonIsAddedToUserGroup()
        {
            this.InstantiateObjects(this.Transaction);

            Assert.Single(this.supplierRelationship.Supplier.ContactsUserGroup.Members);
            Assert.Contains(this.contact, this.supplierRelationship.Supplier.ContactsUserGroup.Members);
        }

        [Fact]
        public void GivenSupplierRelationship_WhenRelationshipPeriodIsNotValid_ThenContactIsNotInContactsUserGroup()
        {
            this.InstantiateObjects(this.Transaction);

            Assert.Single(this.supplierRelationship.Supplier.ContactsUserGroup.Members);
            Assert.Contains(this.contact, this.supplierRelationship.Supplier.ContactsUserGroup.Members);

            this.organisationContactRelationship.FromDate = this.Transaction.Now().AddDays(+1);
            this.organisationContactRelationship.RemoveThroughDate();

            this.Transaction.Derive();

            Assert.Empty(this.supplierRelationship.Supplier.ContactsUserGroup.Members);

            this.organisationContactRelationship.FromDate = this.Transaction.Now().AddSeconds(-1);
            this.organisationContactRelationship.RemoveThroughDate();

            this.Transaction.Derive();

            Assert.Single(this.supplierRelationship.Supplier.ContactsUserGroup.Members);
            Assert.Contains(this.contact, this.supplierRelationship.Supplier.ContactsUserGroup.Members);

            this.organisationContactRelationship.FromDate = this.Transaction.Now().AddDays(-2);
            this.organisationContactRelationship.ThroughDate = this.Transaction.Now().AddDays(-1);

            this.Transaction.Derive();

            Assert.Empty(this.supplierRelationship.Supplier.ContactsUserGroup.Members);
        }

        [Fact]
        public void GivenSupplierRelationship_WhenContactForOrganisationEnds_ThenContactIsRemovedfromContactsUserGroup()
        {
            this.InstantiateObjects(this.Transaction);

            var contact2 = new PersonBuilder(this.Transaction).WithLastName("contact2").Build();
            var contactRelationship2 = new OrganisationContactRelationshipBuilder(this.Transaction)
                .WithOrganisation(this.supplier)
                .WithContact(contact2)
                .WithFromDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(2, this.supplierRelationship.Supplier.ContactsUserGroup.Members.Count);
            Assert.Contains(this.contact, this.supplierRelationship.Supplier.ContactsUserGroup.Members);
            Assert.Contains(contact2, this.supplierRelationship.Supplier.ContactsUserGroup.Members);

            contactRelationship2.ThroughDate = this.Transaction.Now().AddDays(-1);

            this.Transaction.Derive();

            Assert.Single(this.supplierRelationship.Supplier.ContactsUserGroup.Members);
            Assert.Contains(this.contact, this.supplierRelationship.Supplier.ContactsUserGroup.Members);
            Assert.DoesNotContain(contact2, this.supplierRelationship.Supplier.ContactsUserGroup.Members);
        }

        [Fact]
        public void GivenActiveSupplierRelationship_WhenDeriving_ThenInternalOrganisationSuppliersContainsSupplier() => Assert.Contains(this.supplier, this.InternalOrganisation.ActiveSuppliers);

        [Fact]
        public void GivenSupplierRelationshipToCome_WhenDeriving_ThenInternalOrganisationSuppliersDosNotContainSupplier()
        {
            this.supplierRelationship.FromDate = this.Transaction.Now().AddDays(1);
            this.Transaction.Derive();

            Assert.DoesNotContain(this.supplier, this.InternalOrganisation.ActiveSuppliers);
        }

        [Fact]
        public void GivenSupplierRelationshipThatHasEnded_WhenDeriving_ThenInternalOrganisationSuppliersDosNotContainSupplier()
        {
            this.supplierRelationship.FromDate = this.Transaction.Now().AddDays(-10);
            this.supplierRelationship.ThroughDate = this.Transaction.Now().AddDays(-1);

            this.Transaction.Derive();

            Assert.DoesNotContain(this.supplier, this.InternalOrganisation.ActiveSuppliers);
        }

        private void InstantiateObjects(ITransaction transaction)
        {
            this.contact = (Person)transaction.Instantiate(this.contact);
            this.supplier = (Organisation)transaction.Instantiate(this.supplier);
            this.supplierRelationship = (SupplierRelationship)transaction.Instantiate(this.supplierRelationship);
            this.organisationContactRelationship = (OrganisationContactRelationship)transaction.Instantiate(this.organisationContactRelationship);
        }
    }

    public class SupplierRelationshipDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SupplierRelationshipDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSupplierDeriveParties()
        {
            var relationship = new SupplierRelationshipBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var supplier = new OrganisationBuilder(this.Transaction).Build();
            relationship.Supplier = supplier;
            this.Transaction.Derive(false);

            Assert.Contains(supplier, relationship.Parties);
        }

        [Fact]
        public void ChangedInternalOrganisationDeriveParties()
        {
            var relationship = new SupplierRelationshipBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var internalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            relationship.InternalOrganisation = internalOrganisation;
            this.Transaction.Derive(false);

            Assert.Contains(internalOrganisation, relationship.Parties);
        }
    }
}
