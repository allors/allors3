// <copyright file="IrpfRateTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    [Trait("Category", "Security")]
    public class IrpfRateDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public IrpfRateDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deleteRevocation = new Revocations(this.Transaction).IrpfRateDeleteRevocation;

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Revocation deleteRevocation;

        [Fact]
        public void OnIrpfRateDeriveDeletePermission()
        {
            Assert.DoesNotContain(this.deleteRevocation, new IrpfRegimes(this.Transaction).Assessable15.IrpfRates.First().Revocations);
        }

        [Fact]
        public void OnAddedSalesInvoiceAssignedIrpfRateDeriveDeletePermission()
        {
            var irpfRegime = new IrpfRegimes(this.Transaction).Assessable15;

            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            invoice.AssignedIrpfRegime = irpfRegime;
            this.Derive();

            Assert.Contains(this.deleteRevocation, invoice.DerivedIrpfRate.Revocations);
        }

        [Fact]
        public void OnRemovedSalesInvoiceAssignedIrpfRateDeriveDeletePermission()
        {
            var irpfRegime = new IrpfRegimes(this.Transaction).Assessable15;

            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            invoice.AssignedIrpfRegime = irpfRegime;
            this.Derive();

            var irpfRate = invoice.DerivedIrpfRate;

            Assert.Contains(this.deleteRevocation, irpfRate.Revocations);

            invoice.RemoveAssignedIrpfRegime();
            this.Derive();

            Assert.DoesNotContain(this.deleteRevocation, irpfRate.Revocations);
        }
    }
}
