// <copyright file="AccountingTransactionTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Threading;
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
            this.InternalOrganisation.ExportAccounting = true;

            var transaction = new AccountingTransactionBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, transaction.Revocations);
        }

        [Fact]
        public void OnInternalOrganisationExportAccountingFalseDeriveDeletePermission()
        {
            var transaction = new AccountingTransactionBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Contains(this.deleteRevocation, transaction.Revocations);
        }

        [Fact]
        public void OnChangedAccountingTransactionExportedIsFalseDeriveDeletePermission()
        {
            this.InternalOrganisation.ExportAccounting = true;

            var transaction = new AccountingTransactionBuilder(this.Transaction).Build();
            this.Derive();

            transaction.Exported = false;
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, transaction.Revocations);
        }

        [Fact]
        public void OnChangedAccountingTransactionExportedIsTrueDeriveDeletePermission()
        {
            this.InternalOrganisation.ExportAccounting = true;

            var transaction = new AccountingTransactionBuilder(this.Transaction).Build();
            this.Derive();

            transaction.Exported = true;
            this.Derive();

            Assert.Contains(this.deleteRevocation, transaction.Revocations);
        }
    }
}
