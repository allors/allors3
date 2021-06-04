// <copyright file="CashTests.cs" company="Allors bvba">
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

    public class CashTests : DomainTest, IClassFixture<Fixture>
    {
        public CashTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenCashPaymentMethod_WhenDeriving_ThenGeneralLedgerAccountAndJournalAtMostOne()
        {
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

            this.Transaction.Commit();

            var cash = new CashBuilder(this.Transaction)
                .WithDescription("description")
                .WithGeneralLedgerAccount(internalOrganisationGlAccount)
                .Build();

            var internalOrganisation = this.InternalOrganisation;
            internalOrganisation.DoAccounting = true;
            internalOrganisation.DefaultCollectionMethod = cash;

            Assert.False(this.Transaction.Derive(false).HasErrors);

            cash.Journal = journal;

            Assert.True(this.Transaction.Derive(false).HasErrors);

            cash.RemoveGeneralLedgerAccount();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenCashPaymentMethodForSingletonThatDoesAccounting_WhenDeriving_ThenEitherGeneralLedgerAccountOrJournalMustExist()
        {
            var internalOrganisation = this.InternalOrganisation;

            var generalLedgerAccount = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceNumber("ReferenceNumber")
                .WithReferenceCode("ReferenceCode")
                .WithSortCode("SortCode")
                .WithName("GeneralLedgerAccount")
                .WithBalanceType(new BalanceTypes(this.Transaction).Balance)
                .Build();

            var internalOrganisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithInternalOrganisation(this.InternalOrganisation)
                .WithGeneralLedgerAccount(generalLedgerAccount)
                .Build();

            var journal = new JournalBuilder(this.Transaction)
                .WithName("journal")
                .Build();

            this.Transaction.Commit();

            var cash = new CashBuilder(this.Transaction)
                .WithDescription("description")
                .Build();

            internalOrganisation.DoAccounting = true;
            internalOrganisation.AddAssignedActiveCollectionMethod(cash);

            Assert.True(this.Transaction.Derive(false).HasErrors);

            cash.Journal = journal;

            Assert.False(this.Transaction.Derive(false).HasErrors);

            cash.RemoveJournal();
            cash.GeneralLedgerAccount = internalOrganisationGlAccount;

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }
    }

    public class CashRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public CashRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedInternalOrganisationDerivedCollectionMethodsThrowValidation()
        {
            this.InternalOrganisation.DoAccounting = true;

            var cash = new CashBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            this.InternalOrganisation.DefaultCollectionMethod = cash;

            var errors = this.Transaction.Derive(false).Errors.Cast<DerivationErrorAtLeastOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.Cash.GeneralLedgerAccount,
                this.M.Cash.Journal,
            }, errors.SelectMany(v => v.RoleTypes));
        }

        [Fact]
        public void ChangedGeneralLedgerAccountThrowValidation()
        {
            var cash = new CashBuilder(this.Transaction).WithJournal(new JournalBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            cash.GeneralLedgerAccount = new OrganisationGlAccountBuilder(this.Transaction).Build();

            var errors = this.Transaction.Derive(false).Errors.Cast<DerivationErrorAtMostOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.Cash.GeneralLedgerAccount,
                this.M.Cash.Journal,
            }, errors.SelectMany(v => v.RoleTypes));
        }

        [Fact]
        public void ChangedJournalThrowValidation()
        {
            var cash = new CashBuilder(this.Transaction).WithGeneralLedgerAccount(new OrganisationGlAccountBuilder(this.Transaction).Build()).Build();
            this.Transaction.Derive(false);

            cash.Journal = new JournalBuilder(this.Transaction).Build();

            var errors = this.Transaction.Derive(false).Errors.Cast<DerivationErrorAtMostOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.Cash.GeneralLedgerAccount,
                this.M.Cash.Journal,
            }, errors.SelectMany(v => v.RoleTypes));
        }
    }
}
