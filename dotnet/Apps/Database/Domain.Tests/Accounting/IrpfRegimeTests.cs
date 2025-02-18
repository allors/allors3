// <copyright file="IrpfRegimeTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    [Trait("Category", "Security")]
    public class IrpfRegimeDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public IrpfRegimeDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deleteRevocation = new Revocations(this.Transaction).IrpfRegimeDeleteRevocation;

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Revocation deleteRevocation;

        [Fact]
        public void OnIrpfRegimeDeriveDeletePermission()
        {
            Assert.DoesNotContain(this.deleteRevocation, new IrpfRegimes(this.Transaction).Assessable15.Revocations);
        }

        [Fact]
        public void OnAddedSalesInvoiceAssignedIrpfRegimeDeriveDeletePermission()
        {
            var irpfRegime = new IrpfRegimes(this.Transaction).Assessable15;

            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            invoice.AssignedIrpfRegime = irpfRegime;
            this.Derive();

            Assert.Contains(this.deleteRevocation, irpfRegime.Revocations);
        }

        [Fact]
        public void OnRemovedSalesInvoiceAssignedIrpfRegimeDeriveDeletePermission()
        {
            var irpfRegime = new IrpfRegimes(this.Transaction).Assessable15;

            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            invoice.AssignedIrpfRegime = irpfRegime;
            this.Derive();

            Assert.Contains(this.deleteRevocation, irpfRegime.Revocations);

            invoice.RemoveAssignedIrpfRegime();
            this.Derive();

            Assert.Contains(this.deleteRevocation, irpfRegime.Revocations); // contained in invoiceversion
        }
    }
}
