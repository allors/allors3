// <copyright file="VatClauseTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    [Trait("Category", "Security")]
    public class VatClauseDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public VatClauseDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deleteRevocation = new Revocations(this.Transaction).VatClauseDeleteRevocation;

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Revocation deleteRevocation;

        [Fact]
        public void OnVatClauseDeriveDeletePermission()
        {
            Assert.DoesNotContain(this.deleteRevocation, new VatClauses(this.Transaction).BeArt14Par2.Revocations);
        }

        [Fact]
        public void OnAddedSalesInvoiceAssignedVatClauseDeriveDeletePermission()
        {
            var vatClause = new VatClauses(this.Transaction).BeArt14Par2;

            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            invoice.AssignedVatClause = vatClause;
            this.Derive();

            Assert.Contains(this.deleteRevocation, vatClause.Revocations);
        }

        [Fact]
        public void OnRemovedSalesInvoiceAssignedVatClauseDeriveDeletePermission()
        {
            var vatClause = new VatClauses(this.Transaction).BeArt14Par2;

            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            invoice.AssignedVatClause = vatClause;
            this.Derive();

            Assert.Contains(this.deleteRevocation, vatClause.Revocations);

            invoice.RemoveAssignedVatClause();
            this.Derive();

            Assert.Contains(this.deleteRevocation, vatClause.Revocations); // contained in invoiceversion
        }
    }
}
