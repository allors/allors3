// <copyright file="VatRegimeTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    [Trait("Category", "Security")]
    public class VatRegimeDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public VatRegimeDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deleteRevocation = new Revocations(this.Transaction).VatRegimeDeleteRevocation;

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Revocation deleteRevocation;

        [Fact]
        public void OnVatRegimeDeriveDeletePermission()
        {
            Assert.DoesNotContain(this.deleteRevocation, new VatRegimes(this.Transaction).DutchReducedTariff.Revocations);
        }

        [Fact]
        public void OnAddedSalesInvoiceAssignedVatRegimeDeriveDeletePermission()
        {
            var vatRegime = new VatRegimes(this.Transaction).DutchReducedTariff;

            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            invoice.AssignedVatRegime = vatRegime;
            this.Derive();

            Assert.Contains(this.deleteRevocation, vatRegime.Revocations);
        }

        [Fact]
        public void OnRemovedSalesInvoiceAssignedVatRegimeDeriveDeletePermission()
        {
            var vatRegime = new VatRegimes(this.Transaction).DutchReducedTariff;

            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            invoice.AssignedVatRegime = vatRegime;
            this.Derive();

            Assert.Contains(this.deleteRevocation, vatRegime.Revocations);

            invoice.RemoveAssignedVatRegime();
            this.Derive();

            Assert.Contains(this.deleteRevocation, vatRegime.Revocations); // contained in invoiceversion
        }
    }
}
