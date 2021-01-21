// <copyright file="OwnCreditCardTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using Allors.Database.Derivations;
    using Xunit;

    public class OwnCreditCardTests : DomainTest, IClassFixture<Fixture>
    {
        public OwnCreditCardTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenOwnCreditCard_WhenDeriving_ThenCreditCardRelationMustExist()
        {
            var creditCard = new CreditCardBuilder(this.Session)
                .WithCardNumber("4012888888881881")
                .WithExpirationYear(this.Session.Now().Year + 1)
                .WithExpirationMonth(03)
                .WithNameOnCard("M.E. van Knippenberg")
                .WithCreditCardCompany(new CreditCardCompanyBuilder(this.Session).WithName("Visa").Build())
                .Build();

            this.Session.Commit();

            var builder = new OwnCreditCardBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithCreditCard(creditCard);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenOwnCreditCard_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            var creditCard = new CreditCardBuilder(this.Session)
                .WithCardNumber("4012888888881881")
                .WithExpirationYear(this.Session.Now().Year + 1)
                .WithExpirationMonth(03)
                .WithNameOnCard("M.E. van Knippenberg")
                .WithCreditCardCompany(new CreditCardCompanyBuilder(this.Session).WithName("Visa").Build())
                .Build();

            var paymentMethod = new OwnCreditCardBuilder(this.Session)
                .WithCreditCard(creditCard)
                .Build();

            this.InternalOrganisation.AddPaymentMethod(paymentMethod);

            this.Session.Derive();

            Assert.True(paymentMethod.IsActive);
        }

        [Fact]
        public void GivenOwnCreditCardForSingleton_WhenDeriving_ThenExpiredCardIsBlocked()
        {
            var creditCard = new CreditCardBuilder(this.Session)
                .WithCardNumber("4012888888881881")
                .WithExpirationYear(this.Session.Now().Year + 1)
                .WithExpirationMonth(03)
                .WithNameOnCard("M.E. van Knippenberg")
                .WithCreditCardCompany(new CreditCardCompanyBuilder(this.Session).WithName("Visa").Build())
                .Build();

            var paymentMethod = new OwnCreditCardBuilder(this.Session)
                .WithCreditCard(creditCard)
                .Build();

            this.InternalOrganisation.AddPaymentMethod(paymentMethod);

            this.Session.Derive();
            Assert.True(paymentMethod.IsActive);

            creditCard.ExpirationYear = this.Session.Now().Year;
            creditCard.ExpirationMonth = this.Session.Now().Month;

            this.Session.Derive();
            Assert.False(paymentMethod.IsActive);
        }

        [Fact]
        public void GivenOwnCreditCard_WhenDeriving_ThenGeneralLedgerAccountAndJournalAtMostOne()
        {
            this.InternalOrganisation.DoAccounting = true;

            this.Session.Derive(false);

            var generalLedgerAccount = new GeneralLedgerAccountBuilder(this.Session)
                .WithAccountNumber("0001")
                .WithName("GeneralLedgerAccount")
                .WithBalanceSheetAccount(true)
                .Build();

            var internalOrganisationGlAccount = new OrganisationGlAccountBuilder(this.Session)
                .WithGeneralLedgerAccount(generalLedgerAccount)
                .Build();

            var journal = new JournalBuilder(this.Session).WithDescription("journal").Build();

            var creditCard = new CreditCardBuilder(this.Session)
                .WithCardNumber("4012888888881881")
                .WithExpirationYear(this.Session.Now().Year + 1)
                .WithExpirationMonth(03)
                .WithNameOnCard("M.E. van Knippenberg")
                .WithCreditCardCompany(new CreditCardCompanyBuilder(this.Session).WithName("Visa").Build())
                .Build();

            var collectionMethod = new OwnCreditCardBuilder(this.Session)
                .WithCreditCard(creditCard)
                .WithGeneralLedgerAccount(internalOrganisationGlAccount)
                .Build();

            (this.InternalOrganisation).AddAssignedActiveCollectionMethod(collectionMethod);

            this.Session.Commit();

            var internalOrganisation = this.InternalOrganisation;
            internalOrganisation.DoAccounting = true;
            internalOrganisation.DefaultCollectionMethod = collectionMethod;

            Assert.False(this.Session.Derive(false).HasErrors);

            collectionMethod.Journal = journal;

            Assert.True(this.Session.Derive(false).HasErrors);

            collectionMethod.RemoveGeneralLedgerAccount();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenOwnCreditCardForSingletonThatDoesAccounting_WhenDeriving_ThenEitherGeneralLedgerAccountOrJournalMustExist()
        {
            this.InternalOrganisation.DoAccounting = true;

            this.Session.Derive(false);

            var generalLedgerAccount = new GeneralLedgerAccountBuilder(this.Session)
                .WithAccountNumber("0001")
                .WithName("GeneralLedgerAccount")
                .WithBalanceSheetAccount(true)
                .Build();

            var internalOrganisationGlAccount = new OrganisationGlAccountBuilder(this.Session)
                .WithGeneralLedgerAccount(generalLedgerAccount)
                .Build();

            var journal = new JournalBuilder(this.Session).WithDescription("journal").Build();

            var creditCard = new CreditCardBuilder(this.Session)
                .WithCardNumber("4012888888881881")
                .WithExpirationYear(this.Session.Now().Year + 1)
                .WithExpirationMonth(03)
                .WithNameOnCard("M.E. van Knippenberg")
                .WithCreditCardCompany(new CreditCardCompanyBuilder(this.Session).WithName("Visa").Build())
                .Build();

            var paymentMethod = new OwnCreditCardBuilder(this.Session)
                .WithCreditCard(creditCard)
                .Build();

            this.InternalOrganisation.AddPaymentMethod(paymentMethod);

            Assert.True(this.Session.Derive(false).HasErrors);

            paymentMethod.Journal = journal;

            Assert.False(this.Session.Derive(false).HasErrors);

            paymentMethod.RemoveJournal();
            paymentMethod.GeneralLedgerAccount = internalOrganisationGlAccount;

            Assert.False(this.Session.Derive(false).HasErrors);
        }
    }

    public class OwnCreditCardDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public OwnCreditCardDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedInternalOrganisationDerivedCollectionMethodsThrowValidation()
        {
            this.InternalOrganisation.DoAccounting = true;

            var ownCreditCard = new OwnCreditCardBuilder(this.Session).Build();
            this.Session.Derive(false);

            this.InternalOrganisation.AddPaymentMethod(ownCreditCard);

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("AssertAtLeastOne: OwnCreditCard.GeneralLedgerAccount\nOwnCreditCard.Journal"));
        }

        [Fact]
        public void ChangedGeneralLedgerAccountThrowValidation()
        {
            var ownCreditCard = new OwnCreditCardBuilder(this.Session).WithJournal(new JournalBuilder(this.Session).Build()).Build();
            this.Session.Derive(false);

            ownCreditCard.GeneralLedgerAccount = new OrganisationGlAccountBuilder(this.Session).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: OwnCreditCard.GeneralLedgerAccount\nOwnCreditCard.Journal"));
        }

        [Fact]
        public void ChangedJournalThrowValidation()
        {
            var ownCreditCard = new OwnCreditCardBuilder(this.Session).WithGeneralLedgerAccount(new OrganisationGlAccountBuilder(this.Session).Build()).Build();
            this.Session.Derive(false);

            ownCreditCard.Journal = new JournalBuilder(this.Session).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("AssertExistsAtMostOne: OwnCreditCard.GeneralLedgerAccount\nOwnCreditCard.Journal"));
        }

        [Fact]
        public void ChangedCreditCardDeriveIsActive()
        {
            var ownCreditCard = new OwnCreditCardBuilder(this.Session).WithGeneralLedgerAccount(new OrganisationGlAccountBuilder(this.Session).Build()).Build();
            this.Session.Derive(false);

            ownCreditCard.CreditCard = new CreditCardBuilder(this.Session).WithExpirationYear(this.Session.Now().AddYears(-1).Year).WithExpirationMonth(this.Session.Now().Month).Build();
            this.Session.Derive(false);

            Assert.False(ownCreditCard.IsActive);
        }

        [Fact]
        public void ChangedCreditCardExpirationYearDeriveIsActive()
        {
            var ownCreditCard = new OwnCreditCardBuilder(this.Session).WithGeneralLedgerAccount(new OrganisationGlAccountBuilder(this.Session).Build()).Build();
            this.Session.Derive(false);

            ownCreditCard.CreditCard = new CreditCardBuilder(this.Session).WithExpirationYear(this.Session.Now().AddYears(1).Year).WithExpirationMonth(this.Session.Now().Month).Build();
            this.Session.Derive(false);

            ownCreditCard.CreditCard.ExpirationYear = this.Session.Now().AddYears(-1).Year;
            this.Session.Derive(false);

            Assert.False(ownCreditCard.IsActive);
        }
    }
}
