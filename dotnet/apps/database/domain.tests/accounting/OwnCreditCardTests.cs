// <copyright file="OwnCreditCardTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Derivations.Errors;
    using Meta;
    using Xunit;

    public class OwnCreditCardTests : DomainTest, IClassFixture<Fixture>
    {
        public OwnCreditCardTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenOwnCreditCard_WhenDeriving_ThenCreditCardRelationMustExist()
        {
            var creditCard = new CreditCardBuilder(this.Transaction)
                .WithCardNumber("4012888888881881")
                .WithExpirationYear(this.Transaction.Now().Year + 1)
                .WithExpirationMonth(03)
                .WithNameOnCard("Name")
                .WithCreditCardCompany(new CreditCardCompanyBuilder(this.Transaction).WithName("Visa").Build())
                .Build();

            this.Transaction.Commit();

            var builder = new OwnCreditCardBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithCreditCard(creditCard);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenOwnCreditCardForSingleton_WhenDeriving_ThenCreditCardIsActivePaymentMethod()
        {
            var creditCard = new CreditCardBuilder(this.Transaction)
                .WithCardNumber("4012888888881881")
                .WithExpirationYear(this.Transaction.Now().Year + 1)
                .WithExpirationMonth(03)
                .WithNameOnCard("Name")
                .WithCreditCardCompany(new CreditCardCompanyBuilder(this.Transaction).WithName("Visa").Build())
                .Build();

            var paymentMethod = new OwnCreditCardBuilder(this.Transaction)
                .WithCreditCard(creditCard)
                .Build();

            this.InternalOrganisation.AddPaymentMethod(paymentMethod);

            this.Transaction.Derive(false);

            Assert.True(paymentMethod.IsActive);
        }

        [Fact]
        public void GivenOwnCreditCardForSingleton_WhenDeriving_ThenExpiredCardIsBlocked()
        {
            var creditCard = new CreditCardBuilder(this.Transaction)
                .WithCardNumber("4012888888881881")
                .WithExpirationYear(this.Transaction.Now().Year + 1)
                .WithExpirationMonth(03)
                .WithNameOnCard("Name")
                .WithCreditCardCompany(new CreditCardCompanyBuilder(this.Transaction).WithName("Visa").Build())
                .Build();

            var paymentMethod = new OwnCreditCardBuilder(this.Transaction)
                .WithCreditCard(creditCard)
                .Build();

            this.InternalOrganisation.AddPaymentMethod(paymentMethod);

            this.Transaction.Derive(false);
            Assert.True(paymentMethod.IsActive);

            creditCard.ExpirationYear = this.Transaction.Now().Year;
            creditCard.ExpirationMonth = this.Transaction.Now().Month;

            this.Transaction.Derive(false);
            Assert.False(paymentMethod.IsActive);
        }

        [Fact]
        public void GivenOwnCreditCard_WhenDeriving_ThenGeneralLedgerAccountAndJournalAtMostOne()
        {
            this.InternalOrganisation.DoAccounting = true;

            this.Transaction.Derive(false);

            var generalLedgerAccount = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceNumber("ReferenceNumber")
                .WithReferenceCode("ReferenceCode")
                .WithSortCode("SortCode")
                .WithName("GeneralLedgerAccount")
                .WithBalanceType(new BalanceTypes(this.Transaction).Balance)
                .Build();

            var internalOrganisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithGeneralLedgerAccount(generalLedgerAccount)
                .Build();

            var journal = new JournalBuilder(this.Transaction).WithName("journal").Build();

            var creditCard = new CreditCardBuilder(this.Transaction)
                .WithCardNumber("4012888888881881")
                .WithExpirationYear(this.Transaction.Now().Year + 1)
                .WithExpirationMonth(03)
                .WithNameOnCard("Name")
                .WithCreditCardCompany(new CreditCardCompanyBuilder(this.Transaction).WithName("Visa").Build())
                .Build();

            var collectionMethod = new OwnCreditCardBuilder(this.Transaction)
                .WithCreditCard(creditCard)
                .WithGeneralLedgerAccount(internalOrganisationGlAccount)
                .Build();

            (this.InternalOrganisation).AddAssignedActiveCollectionMethod(collectionMethod);

            this.Transaction.Commit();

            var internalOrganisation = this.InternalOrganisation;
            internalOrganisation.DoAccounting = true;
            internalOrganisation.DefaultCollectionMethod = collectionMethod;

            Assert.False(this.Transaction.Derive(false).HasErrors);

            collectionMethod.Journal = journal;

            Assert.True(this.Transaction.Derive(false).HasErrors);

            collectionMethod.RemoveGeneralLedgerAccount();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenOwnCreditCardForSingletonThatDoesAccounting_WhenDeriving_ThenEitherGeneralLedgerAccountOrJournalMustExist()
        {
            this.InternalOrganisation.DoAccounting = true;

            this.Transaction.Derive(false);

            var generalLedgerAccount = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceNumber("ReferenceNumber")
                .WithReferenceCode("ReferenceCode")
                .WithSortCode("SortCode")
                .WithName("GeneralLedgerAccount")
                .WithBalanceType(new BalanceTypes(this.Transaction).Balance)
                .Build();

            var internalOrganisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithGeneralLedgerAccount(generalLedgerAccount)
                .Build();

            var journal = new JournalBuilder(this.Transaction).WithName("journal").Build();

            var creditCard = new CreditCardBuilder(this.Transaction)
                .WithCardNumber("4012888888881881")
                .WithExpirationYear(this.Transaction.Now().Year + 1)
                .WithExpirationMonth(03)
                .WithNameOnCard("Name")
                .WithCreditCardCompany(new CreditCardCompanyBuilder(this.Transaction).WithName("Visa").Build())
                .Build();

            var paymentMethod = new OwnCreditCardBuilder(this.Transaction)
                .WithCreditCard(creditCard)
                .Build();

            this.InternalOrganisation.AddPaymentMethod(paymentMethod);

            Assert.True(this.Transaction.Derive(false).HasErrors);

            paymentMethod.Journal = journal;

            Assert.False(this.Transaction.Derive(false).HasErrors);

            paymentMethod.RemoveJournal();
            paymentMethod.GeneralLedgerAccount = internalOrganisationGlAccount;

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }
    }

    public class OwnCreditCardRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public OwnCreditCardRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedInternalOrganisationDerivedCollectionMethodsThrowValidation()
        {
            this.InternalOrganisation.DoAccounting = true;

            var ownCreditCard = new OwnCreditCardBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            this.InternalOrganisation.AddPaymentMethod(ownCreditCard);

            var errors = this.Transaction.Derive(false).Errors.Cast<DerivationErrorAtLeastOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.OwnBankAccount.GeneralLedgerAccount,
                this.M.OwnBankAccount.Journal,
            }, errors.SelectMany(v => v.RoleTypes));
        }

        [Fact]
        public void ChangedGeneralLedgerAccountThrowValidation()
        {
            var ownCreditCard = new OwnCreditCardBuilder(this.Transaction).WithJournal(new JournalBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            ownCreditCard.GeneralLedgerAccount = new OrganisationGlAccountBuilder(this.Transaction).Build();

            var errors = this.Transaction.Derive(false).Errors.Cast<DerivationErrorAtMostOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.OwnCreditCard.GeneralLedgerAccount,
                this.M.OwnCreditCard.Journal,
            }, errors.SelectMany(v => v.RoleTypes));
        }

        [Fact]
        public void ChangedJournalThrowValidation()
        {
            var ownCreditCard = new OwnCreditCardBuilder(this.Transaction).WithGeneralLedgerAccount(new OrganisationGlAccountBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            ownCreditCard.Journal = new JournalBuilder(this.Transaction).Build();

            var errors = this.Transaction.Derive(false).Errors.Cast<DerivationErrorAtMostOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.OwnCreditCard.GeneralLedgerAccount,
                this.M.OwnCreditCard.Journal,
            }, errors.SelectMany(v => v.RoleTypes));
        }

        [Fact]
        public void ChangedCreditCardDeriveIsActive()
        {
            var ownCreditCard = new OwnCreditCardBuilder(this.Transaction).WithGeneralLedgerAccount(new OrganisationGlAccountBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            ownCreditCard.CreditCard = new CreditCardBuilder(this.Transaction).WithExpirationYear(this.Transaction.Now().AddYears(-1).Year).WithExpirationMonth(this.Transaction.Now().Month).Build();
            this.Transaction.Derive(false);

            Assert.False(ownCreditCard.IsActive);
        }

        [Fact]
        public void ChangedCreditCardExpirationYearDeriveIsActive()
        {
            var ownCreditCard = new OwnCreditCardBuilder(this.Transaction).WithGeneralLedgerAccount(new OrganisationGlAccountBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            ownCreditCard.CreditCard = new CreditCardBuilder(this.Transaction).WithExpirationYear(this.Transaction.Now().AddYears(1).Year).WithExpirationMonth(this.Transaction.Now().Month).Build();
            this.Transaction.Derive(false);

            ownCreditCard.CreditCard.ExpirationYear = this.Transaction.Now().AddYears(-1).Year;
            this.Transaction.Derive(false);

            Assert.False(ownCreditCard.IsActive);
        }
    }
}
