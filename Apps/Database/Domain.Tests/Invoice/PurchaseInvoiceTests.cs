// <copyright file="PurchaseInvoiceTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using TestPopulation;
    using Resources;
    using Xunit;

    public class PurchaseInvoiceTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPurchaseInvoice_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new PurchaseInvoiceBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Session).PurchaseInvoice);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithBilledFrom(this.InternalOrganisation.ActiveSuppliers.First);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenPurchaseInvoice_WhenDeriving_ThenBilledFromPartyMustBeInSupplierRelationship()
        {
            var supplier2 = new OrganisationBuilder(this.Session).WithName("supplier2").Build();

            new PurchaseInvoiceBuilder(this.Session)
                .WithInvoiceNumber("1")
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Session).PurchaseInvoice)
                .WithBilledFrom(supplier2)
                .Build();

            Assert.Equal(ErrorMessages.PartyIsNotASupplier, this.Session.Derive(false).Errors[0].Message);

            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier2).Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenPurchaseInvoice_WhenGettingInvoiceNumberWithoutFormat_ThenInvoiceNumberShouldBeReturned()
        {
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            this.Session.Derive();

            var invoice1 = new PurchaseInvoiceBuilder(this.Session)
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Session).PurchaseInvoice)
                .WithBilledFrom(supplier)
                .Build();

            this.Session.Derive();

            Assert.Equal("incoming invoiceno: 1", invoice1.InvoiceNumber);

            var invoice2 = new PurchaseInvoiceBuilder(this.Session)
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Session).PurchaseInvoice)
                .WithBilledFrom(supplier)
                .Build();

            this.Session.Derive();

            Assert.Equal("incoming invoiceno: 2", invoice2.InvoiceNumber);
        }

        [Fact]
        public void GivenBilledToWithoutInvoiceNumberPrefix_WhenDeriving_ThenSortableInvoiceNumberIsSet()
        {
            this.InternalOrganisation.RemovePurchaseInvoiceNumberPrefix();
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            this.Session.Derive();

            var invoice = new PurchaseInvoiceBuilder(this.Session)
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Session).PurchaseInvoice)
                .WithBilledFrom(supplier)
                .Build();

            invoice.Confirm();
            this.Session.Derive();

            Assert.Equal(int.Parse(invoice.InvoiceNumber), invoice.SortableInvoiceNumber);
        }

        [Fact]
        public void GivenBilledToWithInvoiceNumberPrefix_WhenDeriving_ThenSortableInvoiceNumberIsSet()
        {
            this.InternalOrganisation.PurchaseInvoiceNumberPrefix = "prefix-";
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            this.Session.Derive();

            var invoice = new PurchaseInvoiceBuilder(this.Session)
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Session).PurchaseInvoice)
                .WithBilledFrom(supplier)
                .Build();

            invoice.Confirm();
            this.Session.Derive();

            Assert.Equal(int.Parse(invoice.InvoiceNumber.Split('-')[1]), invoice.SortableInvoiceNumber);
        }

        [Fact]
        public void GivenBilledToWithParametrizedInvoiceNumberPrefix_WhenDeriving_ThenSortableInvoiceNumberIsSet()
        {
            this.InternalOrganisation.PurchaseInvoiceNumberPrefix = "prefix-{year}-";
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            this.Session.Derive();

            var invoice = new PurchaseInvoiceBuilder(this.Session)
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Session).PurchaseInvoice)
                .WithBilledFrom(supplier)
                .Build();

            invoice.Confirm();
            this.Session.Derive();

            Assert.Equal(int.Parse(string.Concat(this.Session.Now().Date.Year.ToString(), invoice.InvoiceNumber.Split('-').Last())), invoice.SortableInvoiceNumber);
        }
    }

    public class PurchaseInvoiceBuildDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceBuildDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DerivePurchaseInvoiceState()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.True(invoice.ExistPurchaseInvoiceState);
        }

        [Fact]
        public void DeriveInvoiceDate()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.True(invoice.ExistInvoiceDate);
        }

        [Fact]
        public void DeriveEntryDate()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.True(invoice.ExistEntryDate);
        }
    }

    public class PurchaseInvoiceCreatedDerivation : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceCreatedDerivation(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedAssignedVatRegimeDeriveDerivedVatRegime()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Session).Build();
            this.Session.Derive(false);

            invoice.AssignedVatRegime = new VatRegimes(this.Session).ServiceB2B;
            this.Session.Derive(false);

            Assert.Equal(invoice.DerivedVatRegime, invoice.AssignedVatRegime);
        }

        [Fact]
        public void ChangedBilledFromDeriveDerivedVatRegime()
        {
            var supplier1 = this.InternalOrganisation.ActiveSuppliers.First;
            supplier1.VatRegime = new VatRegimes(this.Session).Assessable10;

            var supplier2 = this.InternalOrganisation.CreateSupplier(this.Session.Faker());
            supplier2.VatRegime = new VatRegimes(this.Session).Assessable21;

            var invoice = new PurchaseInvoiceBuilder(this.Session).WithBilledFrom(supplier1).Build();
            this.Session.Derive(false);

            invoice.BilledFrom = supplier2;
            this.Session.Derive(false);

            Assert.Equal(invoice.DerivedVatRegime, supplier2.VatRegime);
        }

        [Fact]
        public void ChangedBilledFromVatRegimeDeriveDerivedVatRegime()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.First;

            var invoice = new PurchaseInvoiceBuilder(this.Session).WithBilledFrom(supplier).Build();
            this.Session.Derive(false);

            supplier.VatRegime = new VatRegimes(this.Session).Assessable10;
            this.Session.Derive(false);

            Assert.Equal(invoice.DerivedVatRegime, supplier.VatRegime);
        }

        [Fact]
        public void ChangedAssignedIrpfRegimeDeriveDerivedIrpfRegime()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Session).Build();
            this.Session.Derive(false);

            invoice.AssignedIrpfRegime = new IrpfRegimes(this.Session).Assessable15;
            this.Session.Derive(false);

            Assert.Equal(invoice.DerivedIrpfRegime, invoice.AssignedIrpfRegime);
        }

        [Fact]
        public void ChangedBilledFromDeriveDerivedIrpfRegime()
        {
            var supplier1 = this.InternalOrganisation.ActiveSuppliers.First;
            supplier1.IrpfRegime = new IrpfRegimes(this.Session).Assessable15;

            var supplier2 = this.InternalOrganisation.CreateSupplier(this.Session.Faker());
            supplier2.IrpfRegime = new IrpfRegimes(this.Session).Assessable19;

            var invoice = new PurchaseInvoiceBuilder(this.Session).WithBilledFrom(supplier1).Build();
            this.Session.Derive(false);

            invoice.BilledFrom = supplier2;
            this.Session.Derive(false);

            Assert.Equal(invoice.DerivedIrpfRegime, supplier2.IrpfRegime);
        }

        [Fact]
        public void ChangedBilledFromIrpfRegimeDeriveDerivedIrpfRegime()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.First;

            var invoice = new PurchaseInvoiceBuilder(this.Session).WithBilledFrom(supplier).Build();
            this.Session.Derive(false);

            supplier.IrpfRegime = new IrpfRegimes(this.Session).Assessable15;
            this.Session.Derive(false);

            Assert.Equal(invoice.DerivedIrpfRegime, supplier.IrpfRegime);
        }

        [Fact]
        public void ChangedAssignedCurrencyDeriveDerivedCurrency()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Session).Build();
            this.Session.Derive(false);

            var swedishKrona = new Currencies(this.Session).FindBy(M.Currency.IsoCode, "SEK");
            invoice.AssignedCurrency = swedishKrona;
            this.Session.Derive(false);

            Assert.Equal(invoice.DerivedCurrency, invoice.AssignedCurrency);
        }

        [Fact]
        public void ChangedBillToCustomerPreferredCurrencyDerivedCurrency()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Session).Build();
            this.Session.Derive(false);

            var swedishKrona = new Currencies(this.Session).FindBy(M.Currency.IsoCode, "SEK");
            invoice.BilledTo.PreferredCurrency = swedishKrona;
            this.Session.Derive(false);

            Assert.Equal(invoice.DerivedCurrency, invoice.BilledTo.PreferredCurrency);
        }
    }

    [Trait("Category", "Security")]
    public class PurchaseInvoiceSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceSecurityTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void GivenPurchaseInvoice_WhenObjectStateIsCreated_ThenCheckTransitions()
        {
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            this.Session.Derive();
            this.Session.Commit();

            User user = this.Administrator;
            this.Session.SetUser(user);

            var invoice = new PurchaseInvoiceBuilder(this.Session)
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Session).PurchaseInvoice)
                .WithBilledFrom(supplier)
                .Build();

            this.Session.Derive();

            var acl = new DatabaseAccessControlLists(this.Session.GetUser())[invoice];

            Assert.Equal(new PurchaseInvoiceStates(this.Session).Created, invoice.PurchaseInvoiceState);
            Assert.False(acl.CanExecute(this.M.PurchaseInvoice.Approve));
            Assert.False(acl.CanExecute(this.M.PurchaseInvoice.Reject));
            Assert.False(acl.CanExecute(this.M.PurchaseInvoice.Reopen));
            Assert.False(acl.CanExecute(this.M.PurchaseInvoice.SetPaid));
            Assert.False(acl.CanExecute(this.M.PurchaseInvoice.Revise));
            Assert.False(acl.CanExecute(this.M.PurchaseInvoice.FinishRevising));
            Assert.False(acl.CanExecute(this.M.PurchaseInvoice.CreateSalesInvoice));
        }
    }

    [Trait("Category", "Security")]
    public class PurchaseInvoiceDeniedPermissionDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceDeniedPermissionDerivationTests(Fixture fixture) : base(fixture)
        {
            this.deletePermission = new Permissions(this.Session).Get(this.M.PurchaseInvoice.ObjectType, this.M.PurchaseInvoice.Delete);
            this.createSalesInvoicePermission = new Permissions(this.Session).Get(this.M.PurchaseInvoice.ObjectType, this.M.PurchaseInvoice.CreateSalesInvoice);
        }

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;
        private readonly Permission createSalesInvoicePermission;


        [Fact]
        public void OnChangedPurchaseInvoiceStateCreatedDeriveDeletePermission()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Session).Build();

            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, purchaseInvoice.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseInvoiceStateApprovedDeriveDeletePermission()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Session).Build();

            this.Session.Derive(false);

            purchaseInvoice.Approve();

            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, purchaseInvoice.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseInvoiceStateCreatedWithSalesInvoiceDeriveDeletePermission()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Session).Build();

            var salesInvoice = new SalesInvoiceBuilder(this.Session).WithPurchaseInvoice(purchaseInvoice).Build();

            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, purchaseInvoice.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseInvoiceStateCreatedDeriveCreateSalesInvoicePermission()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Session).Build();

            this.Session.Derive(false);

            Assert.Contains(this.createSalesInvoicePermission, purchaseInvoice.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseInvoiceStateNotPaidWithInternalOrganisationDeriveCreateSalesInvoicePermission()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Session).WithBilledFrom(this.InternalOrganisation).Build();
            this.Session.Derive(false);

            purchaseInvoice.Approve();
            this.Session.Derive(false);

            purchaseInvoice.SetPaid();
            this.Session.Derive(false);

            Assert.DoesNotContain(this.createSalesInvoicePermission, purchaseInvoice.DeniedPermissions);
        }
    }
}
