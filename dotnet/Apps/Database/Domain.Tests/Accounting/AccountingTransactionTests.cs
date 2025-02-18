// <copyright file="AccountingTransactionTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    [Trait("Category", "Security")]
    public class AccountingTransactionDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public AccountingTransactionDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deleteRevocation = new Revocations(this.Transaction).AccountingTransactionDeleteRevocation;

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Revocation deleteRevocation;

        [Fact]
        public void OnInternalOrganisationExportAccountingTrueDeriveDeletePermission()
        {
            var transaction = new AccountingTransactionBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, transaction.Revocations);
        }

        [Fact]
        public void OnInternalOrganisationExportAccountingFalseDeriveDeletePermission()
        {
            this.InternalOrganisation.ExportAccounting = false;

            var transaction = new AccountingTransactionBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Contains(this.deleteRevocation, transaction.Revocations);
        }

        [Fact]
        public void OnChangedAccountingTransactionExportedIsFalseDeriveDeletePermission()
        {
            var transaction = new AccountingTransactionBuilder(this.Transaction).Build();
            this.Derive();

            transaction.Exported = false;
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, transaction.Revocations);
        }

        [Fact]
        public void OnChangedAccountingTransactionExportedIsTrueDeriveDeletePermission()
        {
            var transaction = new AccountingTransactionBuilder(this.Transaction).Build();
            this.Derive();

            transaction.Exported = true;
            this.Derive();

            Assert.Contains(this.deleteRevocation, transaction.Revocations);
        }
    }
}
