// <copyright file="VatRateTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    [Trait("Category", "Security")]
    public class VatRateDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public VatRateDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deleteRevocation = new Revocations(this.Transaction).VatRateDeleteRevocation;

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Revocation deleteRevocation;

        [Fact]
        public void OnVatRateDeriveDeletePermission()
        {
            Assert.DoesNotContain(this.deleteRevocation, new VatRates(this.Transaction).BelgiumStandard.Revocations);
        }

        [Fact]
        public void OnAddedSalesInvoiceAssignedVatRateDeriveDeletePermission()
        {
            var vatRegime = new VatRegimes(this.Transaction).BelgiumStandard;

            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            invoice.AssignedVatRegime = vatRegime;
            this.Derive();

            Assert.Contains(this.deleteRevocation, invoice.DerivedVatRate.Revocations);
        }

        [Fact]
        public void OnRemovedSalesInvoiceAssignedVatRateDeriveDeletePermission()
        {
            var vatRegime = new VatRegimes(this.Transaction).BelgiumStandard;

            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            invoice.AssignedVatRegime = vatRegime;
            this.Derive();

            var vatrate = invoice.DerivedVatRate;

            Assert.Contains(this.deleteRevocation, vatrate.Revocations);

            invoice.RemoveAssignedVatRegime();
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, vatrate.Revocations);
        }
    }
}
