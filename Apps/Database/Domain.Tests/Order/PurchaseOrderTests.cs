// <copyright file="PurchaseOrderTests.cs" company="Allors bvba">
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
    using System.Collections.Generic;
    using Allors.Database.Derivations;

    public class PurchaseOrderTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPurchaseOrderBuilder_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            var internalOrganisation = this.InternalOrganisation;
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            var order = new PurchaseOrderBuilder(this.Session).WithTakenViaSupplier(supplier).Build();

            this.Session.Derive();

            Assert.Equal(new PurchaseOrderStates(this.Session).Created, order.PurchaseOrderState);
            Assert.Equal(this.Session.Now().Date, order.OrderDate.Date);
            Assert.Equal(this.Session.Now().Date, order.EntryDate.Date);
            Assert.Equal(order.PreviousTakenViaSupplier, order.TakenViaSupplier);
        }

        [Fact]
        public void GivenOrder_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier2").Build();
            var internalOrganisation = this.InternalOrganisation;
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            ContactMechanism takenViaContactMechanism = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var supplierContactMechanism = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(takenViaContactMechanism)
                .WithUseAsDefault(true)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).OrderAddress)
                .Build();
            supplier.AddPartyContactMechanism(supplierContactMechanism);

            this.Session.Derive();
            this.Session.Commit();

            var builder = new PurchaseOrderBuilder(this.Session);
            builder.Build();

            Assert.True(this.Session.Derive(false).HasErrors);

            this.Session.Rollback();

            builder.WithTakenViaSupplier(supplier);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);

            builder.WithAssignedTakenViaContactMechanism(takenViaContactMechanism);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenPurchaseOrder_WhenDeriving_ThenTakenViaSupplierMustBeInSupplierRelationship()
        {
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier2").Build();
            var internalOrganisation = this.InternalOrganisation;

            new PurchaseOrderBuilder(this.Session)
                .WithTakenViaSupplier(supplier)
                .Build();

            var expectedError = ErrorMessages.PartyIsNotASupplier;
            Assert.Equal(expectedError, this.Session.Derive(false).Errors[0].Message);

            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Fact]
        public void GivenOrder_WhenDeriving_ThenLocaleMustExist()
        {
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier2").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            var mechelen = new CityBuilder(this.Session).WithName("Mechelen").Build();
            ContactMechanism takenViaContactMechanism = new PostalAddressBuilder(this.Session).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var supplierContactMechanism = new PartyContactMechanismBuilder(this.Session)
                .WithContactMechanism(takenViaContactMechanism)
                .WithUseAsDefault(true)
                .WithContactPurpose(new ContactMechanismPurposes(this.Session).OrderAddress)
                .Build();
            supplier.AddPartyContactMechanism(supplierContactMechanism);

            var order = new PurchaseOrderBuilder(this.Session)
                .WithTakenViaSupplier(supplier)
                .Build();

            this.Session.Derive();

            Assert.Equal(order.OrderedBy.Locale, order.DerivedLocale);
        }

        [Fact]
        public void GivenPurchaseOrder_WhenGettingOrderNumberWithoutFormat_ThenOrderNumberShouldBeReturned()
        {
            var internalOrganisation = this.InternalOrganisation;
            internalOrganisation.RemovePurchaseOrderNumberPrefix();

            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            this.Session.Derive();

            var order1 = new PurchaseOrderBuilder(this.Session).WithTakenViaSupplier(supplier).Build();

            this.Session.Derive();

            Assert.Equal("1", order1.OrderNumber);

            var order2 = new PurchaseOrderBuilder(this.Session).WithTakenViaSupplier(supplier).Build();

            this.Session.Derive();

            Assert.Equal("2", order2.OrderNumber);
        }

        [Fact]
        public void GivenPurchaseOrder_WhenGettingOrderNumberWithFormat_ThenFormattedOrderNumberShouldBeReturned()
        {
            this.InternalOrganisation.InvoiceSequence = new InvoiceSequences(this.Session).EnforcedSequence;
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            this.Session.Derive();

            var internalOrganisation = this.InternalOrganisation;
            internalOrganisation.PurchaseOrderNumberPrefix = "the format is ";

            var order1 = new PurchaseOrderBuilder(this.Session)
                .WithTakenViaSupplier(supplier)
                .Build();

            this.Session.Derive();

            Assert.Equal("the format is 1", order1.OrderNumber);

            var order2 = new PurchaseOrderBuilder(this.Session)
                .WithTakenViaSupplier(supplier)
                .Build();

            this.Session.Derive();

            Assert.Equal("the format is 2", order2.OrderNumber);
        }

        [Fact]
        public void GivenBilledToWithoutOrderNumberPrefix_WhenDeriving_ThenSortableOrderNumberIsSet()
        {
            this.InternalOrganisation.RemovePurchaseOrderNumberPrefix();
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            this.Session.Derive();

            var order = new PurchaseOrderBuilder(this.Session)
                .WithTakenViaSupplier(supplier)
                .Build();

            order.SetReadyForProcessing();
            this.Session.Derive();

            Assert.Equal(int.Parse(order.OrderNumber), order.SortableOrderNumber);
        }

        [Fact]
        public void GivenBilledToWithOrderNumberPrefix_WhenDeriving_ThenSortableOrderNumberIsSet()
        {
            this.InternalOrganisation.InvoiceSequence = new InvoiceSequences(this.Session).EnforcedSequence;
            this.InternalOrganisation.PurchaseOrderNumberPrefix = "prefix-";
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            this.Session.Derive();

            var order = new PurchaseOrderBuilder(this.Session)
                .WithTakenViaSupplier(supplier)
                .Build();

            order.SetReadyForProcessing();
            this.Session.Derive();

            Assert.Equal(int.Parse(order.OrderNumber.Split('-')[1]), order.SortableOrderNumber);
        }

        [Fact]
        public void GivenBilledToWithParametrizedOrderNumberPrefix_WhenDeriving_ThenSortableOrderNumberIsSet()
        {
            this.InternalOrganisation.InvoiceSequence = new InvoiceSequences(this.Session).EnforcedSequence;
            this.InternalOrganisation.PurchaseOrderNumberPrefix = "prefix-{year}-";
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            this.Session.Derive();

            var order = new PurchaseOrderBuilder(this.Session)
                .WithTakenViaSupplier(supplier)
                .Build();

            order.SetReadyForProcessing();
            this.Session.Derive();

            var number = int.Parse(order.OrderNumber.Split('-').Last()).ToString("000000");
            Assert.Equal(int.Parse(string.Concat(this.Session.Now().Date.Year.ToString(), number)), order.SortableOrderNumber);
        }

        [Fact]
        public void GivenPurchaseOrder_WhenConfirming_ThenAllValidItemsAreInConfirmedState()
        {
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier2").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            var part = new NonUnifiedPartBuilder(this.Session)
                .WithProductIdentification(new PartNumberBuilder(this.Session)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Part).Build())
                .Build();

            var order = new PurchaseOrderBuilder(this.Session)
                .WithTakenViaSupplier(supplier)
                .WithAssignedVatRegime(new VatRegimes(this.Session).Exempt)
                .Build();

            var item1 = new PurchaseOrderItemBuilder(this.Session).WithPart(part).WithQuantityOrdered(1).Build();
            var item2 = new PurchaseOrderItemBuilder(this.Session).WithPart(part).WithQuantityOrdered(2).Build();
            var item3 = new PurchaseOrderItemBuilder(this.Session).WithPart(part).WithQuantityOrdered(3).Build();
            var item4 = new PurchaseOrderItemBuilder(this.Session).WithPart(part).WithQuantityOrdered(4).Build();
            order.AddPurchaseOrderItem(item1);
            order.AddPurchaseOrderItem(item2);
            order.AddPurchaseOrderItem(item3);
            order.AddPurchaseOrderItem(item4);

            this.Session.Derive();

            order.SetReadyForProcessing();
            this.Session.Derive();

            item4.Cancel();
            this.Session.Derive();

            Assert.Equal(3, order.ValidOrderItems.Count);
            Assert.Contains(item1, order.ValidOrderItems);
            Assert.Contains(item2, order.ValidOrderItems);
            Assert.Contains(item3, order.ValidOrderItems);
            Assert.Equal(new PurchaseOrderItemStates(this.Session).InProcess, item1.PurchaseOrderItemState);
            Assert.Equal(new PurchaseOrderItemStates(this.Session).InProcess, item2.PurchaseOrderItemState);
            Assert.Equal(new PurchaseOrderItemStates(this.Session).InProcess, item3.PurchaseOrderItemState);
            Assert.Equal(new PurchaseOrderItemStates(this.Session).Cancelled, item4.PurchaseOrderItemState);
        }

        [Fact]
        public void GivenPurchaseOrder_WhenOrdering_ThenAllValidItemsAreInInProcessState()
        {
            var supplier = new OrganisationBuilder(this.Session).WithName("supplier2").Build();
            new SupplierRelationshipBuilder(this.Session).WithSupplier(supplier).Build();

            var part = new NonUnifiedPartBuilder(this.Session)
                .WithProductIdentification(new PartNumberBuilder(this.Session)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Part).Build())
                .Build();

            var order = new PurchaseOrderBuilder(this.Session)
                .WithTakenViaSupplier(supplier)
                .WithAssignedVatRegime(new VatRegimes(this.Session).Exempt)
                .Build();

            var item1 = new PurchaseOrderItemBuilder(this.Session).WithPart(part).WithQuantityOrdered(1).Build();
            var item2 = new PurchaseOrderItemBuilder(this.Session).WithPart(part).WithQuantityOrdered(2).Build();
            var item3 = new PurchaseOrderItemBuilder(this.Session).WithPart(part).WithQuantityOrdered(3).Build();
            order.AddPurchaseOrderItem(item1);
            order.AddPurchaseOrderItem(item2);
            order.AddPurchaseOrderItem(item3);

            this.Session.Derive();

            order.SetReadyForProcessing();
            this.Session.Derive();

            Assert.Equal(3, order.ValidOrderItems.Count);
            Assert.Contains(item1, order.ValidOrderItems);
            Assert.Contains(item2, order.ValidOrderItems);
            Assert.Contains(item3, order.ValidOrderItems);
            Assert.Equal(new PurchaseOrderItemStates(this.Session).InProcess, item1.PurchaseOrderItemState);
            Assert.Equal(new PurchaseOrderItemStates(this.Session).InProcess, item2.PurchaseOrderItemState);
            Assert.Equal(new PurchaseOrderItemStates(this.Session).InProcess, item3.PurchaseOrderItemState);
        }
    }

    public class PurchaseOrderCreatedDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderCreatedDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedOrderedByDeriveDerivedLocaleFromOrderedByLocale()
        {
            var swedishLocale = new LocaleBuilder(this.Session)
               .WithCountry(new Countries(this.Session).FindBy(this.M.Country.IsoCode, "SE"))
               .WithLanguage(new Languages(this.Session).FindBy(this.M.Language.IsoCode, "sv"))
               .Build();

            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            this.InternalOrganisation.Locale = swedishLocale;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedLocale, swedishLocale);
        }

        [Fact]
        public void ChangedLocaleDeriveDerivedLocaleFromLocale()
        {
            var swedishLocale = new LocaleBuilder(this.Session)
               .WithCountry(new Countries(this.Session).FindBy(this.M.Country.IsoCode, "SE"))
               .WithLanguage(new Languages(this.Session).FindBy(this.M.Language.IsoCode, "sv"))
               .Build();

            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.Locale = swedishLocale;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedLocale, swedishLocale);
        }

        [Fact]
        public void ChangedAssignedVatRegimeDeriveDerivedVatRegime()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.AssignedVatRegime = new VatRegimes(this.Session).ServiceB2B;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedVatRegime, order.AssignedVatRegime);
        }

        [Fact]
        public void ChangedTakenViaSupplierDeriveDerivedVatRegime()
        {
            var supplier1 = this.InternalOrganisation.ActiveSuppliers.First;
            supplier1.VatRegime = new VatRegimes(this.Session).Assessable10;

            var supplier2 = this.InternalOrganisation.CreateSupplier(this.Session.Faker());
            supplier2.VatRegime = new VatRegimes(this.Session).Assessable21;

            var order = new PurchaseOrderBuilder(this.Session).WithTakenViaSupplier(supplier1).Build();
            this.Session.Derive(false);

            order.TakenViaSupplier = supplier2;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedVatRegime, supplier2.VatRegime);
        }

        [Fact]
        public void ChangedTakenViaSupplierVatRegimeDeriveDerivedVatRegime()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.First;

            var order = new PurchaseOrderBuilder(this.Session).WithTakenViaSupplier(supplier).Build();
            this.Session.Derive(false);

            supplier.VatRegime = new VatRegimes(this.Session).Assessable10;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedVatRegime, supplier.VatRegime);
        }

        [Fact]
        public void ChangedAssignedIrpfRegimeDeriveDerivedIrpfRegime()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.AssignedIrpfRegime = new IrpfRegimes(this.Session).Assessable15;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedIrpfRegime, order.AssignedIrpfRegime);
        }

        [Fact]
        public void ChangedTakenViaSupplierDeriveDerivedIrpfRegime()
        {
            var supplier1 = this.InternalOrganisation.ActiveSuppliers.First;
            supplier1.IrpfRegime = new IrpfRegimes(this.Session).Assessable15;

            var supplier2 = this.InternalOrganisation.CreateSupplier(this.Session.Faker());
            supplier2.IrpfRegime = new IrpfRegimes(this.Session).Assessable19;

            var order = new PurchaseOrderBuilder(this.Session).WithTakenViaSupplier(supplier1).Build();
            this.Session.Derive(false);

            order.TakenViaSupplier = supplier2;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedIrpfRegime, supplier2.IrpfRegime);
        }

        [Fact]
        public void ChangedTakenViaSupplierIrpfRegimeDeriveDerivedIrpfRegime()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.First;

            var order = new PurchaseOrderBuilder(this.Session).WithTakenViaSupplier(supplier).Build();
            this.Session.Derive(false);

            supplier.IrpfRegime = new IrpfRegimes(this.Session).Assessable15;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedIrpfRegime, supplier.IrpfRegime);
        }

        [Fact]
        public void ChangedAssignedCurrencyDeriveDerivedCurrency()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var swedishKrona = new Currencies(this.Session).FindBy(M.Currency.IsoCode, "SEK");
            order.AssignedCurrency = swedishKrona;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedCurrency, order.AssignedCurrency);
        }

        [Fact]
        public void ChangedOrderedByPreferredCurrencyDerivedCurrency()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var swedishKrona = new Currencies(this.Session).FindBy(M.Currency.IsoCode, "SEK");
            order.OrderedBy.PreferredCurrency = swedishKrona;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedCurrency, order.OrderedBy.PreferredCurrency);
        }

        [Fact]
        public void ChangedAssignedTakenViaContactMechanismDeriveDerivedTakenViaContactMechanism()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.AssignedTakenViaContactMechanism = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(order.DerivedTakenViaContactMechanism, order.AssignedTakenViaContactMechanism);
        }

        [Fact]
        public void ChangedTakenByOrderAddressDeriveDerivedTakenViaContactMechanism()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.First;
            var order = new PurchaseOrderBuilder(this.Session).WithTakenViaSupplier(supplier).Build();
            this.Session.Derive(false);

            supplier.OrderAddress = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(order.DerivedTakenViaContactMechanism, supplier.OrderAddress);
        }

        [Fact]
        public void ChangedAssignedBillToContactMechanismDeriveDeriveDerivedBillToContactMechanism()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.AssignedBillToContactMechanism = new PostalAddressBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Equal(order.DerivedBillToContactMechanism, order.AssignedBillToContactMechanism);
        }

        [Fact]
        public void ChangedOrderByBillingAddressDeriveDeriveDerivedBillToContactMechanism()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var postalAddress = new PostalAddressBuilder(this.Session).Build();
            this.InternalOrganisation.BillingAddress = postalAddress;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedBillToContactMechanism, postalAddress);
        }

        [Fact]
        public void ChangedOrderByGeneralCorrespondenceDeriveDeriveDerivedBillToContactMechanism()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var postalAddress = new PostalAddressBuilder(this.Session).Build();
            this.InternalOrganisation.RemoveBillingAddress();
            this.InternalOrganisation.GeneralCorrespondence = postalAddress;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedBillToContactMechanism, postalAddress);
        }

        [Fact]
        public void ChangedAssignedShipToAddressDeriveDeriveDerivedShipToAddress()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var postalAddress = new PostalAddressBuilder(this.Session).Build();
            order.AssignedShipToAddress = postalAddress;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedShipToAddress, postalAddress);
        }

        [Fact]
        public void ChangedOrderedByShippingAddressDeriveDeriveDerivedShipToAddress()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var postalAddress = new PostalAddressBuilder(this.Session).Build();
            this.InternalOrganisation.ShippingAddress = postalAddress;
            this.Session.Derive(false);

            Assert.Equal(order.DerivedShipToAddress, postalAddress);
        }
    }

    public class PurchaseOrderAwaitingApprovalLevel1DerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderAwaitingApprovalLevel1DerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseOrderStateCreateApprovalTask()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.First;
            var supplierRelationship = supplier.SupplierRelationshipsWhereSupplier.First(v => v.InternalOrganisation.Equals(this.InternalOrganisation));
            supplierRelationship.NeedsApproval = true;
            supplierRelationship.ApprovalThresholdLevel1 = 0;

            var order = new PurchaseOrderBuilder(this.Session).WithTakenViaSupplier(supplier).Build();
            this.Session.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Session).WithQuantityOrdered(1).WithAssignedUnitPrice(100).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Session.Derive(false);

            order.SetReadyForProcessing();
            this.Session.Derive(false);

            Assert.True(order.ExistPurchaseOrderApprovalsLevel1WherePurchaseOrder);
        }
    }

    public class PurchaseOrderAwaitingApprovalLevel2DerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderAwaitingApprovalLevel2DerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseOrderStateCreateApprovalTask()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.First;
            var supplierRelationship = supplier.SupplierRelationshipsWhereSupplier.First(v => v.InternalOrganisation.Equals(this.InternalOrganisation));
            supplierRelationship.NeedsApproval = true;
            supplierRelationship.ApprovalThresholdLevel1 = 0;
            supplierRelationship.ApprovalThresholdLevel2 = 0;

            var order = new PurchaseOrderBuilder(this.Session).WithTakenViaSupplier(supplier).Build();
            this.Session.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Session).WithQuantityOrdered(1).WithAssignedUnitPrice(100).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Session.Derive(false);

            order.SetReadyForProcessing();
            this.Session.Derive(false);

            var approvalLevel1 = order.PurchaseOrderApprovalsLevel1WherePurchaseOrder.First;
            approvalLevel1.Approve();
            this.Session.Derive(false);

            Assert.True(order.ExistPurchaseOrderApprovalsLevel2WherePurchaseOrder);
        }
    }

    public class PurchaseOrderDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedOrderedByThrowValidationError()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.OrderedBy = new OrganisationBuilder(this.Session).WithIsInternalOrganisation(true).Build();

            var expectedMessage = $"{order} { this.M.PurchaseOrder.OrderedBy} { ErrorMessages.InternalOrganisationChanged}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedInternalOrganisationActiveSuppliersThrowValidationError()
        {
            var order = new PurchaseOrderBuilder(this.Session).WithTakenViaSupplier(this.InternalOrganisation.ActiveSuppliers.First).Build();
            this.Session.Derive(false);

            var supplierRelationship = this.InternalOrganisation.ActiveSuppliers.First.SupplierRelationshipsWhereSupplier.First;
            supplierRelationship.ThroughDate = supplierRelationship.FromDate;

            var expectedMessage = $"{order} { this.M.PurchaseOrder.TakenViaSupplier} { ErrorMessages.PartyIsNotASupplier}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedOrderDateThrowValidationErrorPartyIsNotASupplier()
        {
            var order = new PurchaseOrderBuilder(this.Session).WithTakenViaSupplier(this.InternalOrganisation.ActiveSuppliers.First).Build();
            this.Session.Derive(false);

            var supplierRelationship = this.InternalOrganisation.ActiveSuppliers.First.SupplierRelationshipsWhereSupplier.First;
            order.OrderDate = supplierRelationship.FromDate.AddDays(-1);

            var expectedMessage = $"{order} { this.M.PurchaseOrder.TakenViaSupplier} { ErrorMessages.PartyIsNotASupplier}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedTakenViaSupplierThrowValidationError()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.TakenViaSupplier = new OrganisationBuilder(this.Session).Build();

            var expectedMessage = $"{order} { this.M.PurchaseOrder.TakenViaSupplier} { ErrorMessages.PartyIsNotASupplier}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedTakenViaSubcontractorThrowValidationError()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            order.TakenViaSubcontractor = new OrganisationBuilder(this.Session).Build();

            var expectedMessage = $"{order} { this.M.PurchaseOrder.TakenViaSubcontractor} { ErrorMessages.PartyIsNotASubcontractor}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedInternalOrganisationActiveSubcontractorsThrowValidationError()
        {
            var order = new PurchaseOrderBuilder(this.Session).WithTakenViaSubcontractor(this.InternalOrganisation.ActiveSubContractors.First).Build();
            this.Session.Derive(false);

            var subcontractorRelationship = this.InternalOrganisation.ActiveSubContractors.First.SubContractorRelationshipsWhereSubContractor.First;
            subcontractorRelationship.ThroughDate = subcontractorRelationship.FromDate;

            var expectedMessage = $"{order} { this.M.PurchaseOrder.TakenViaSubcontractor} { ErrorMessages.PartyIsNotASubcontractor}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedOrderDateThrowValidationErrorPartyIsNotASubcontractor()
        {
            var order = new PurchaseOrderBuilder(this.Session).WithTakenViaSubcontractor(this.InternalOrganisation.ActiveSubContractors.First).Build();
            this.Session.Derive(false);

            var subcontractorRelationship = this.InternalOrganisation.ActiveSubContractors.First.SubContractorRelationshipsWhereSubContractor.First;
            order.OrderDate = subcontractorRelationship.FromDate.AddDays(-1);

            var expectedMessage = $"{order} { this.M.PurchaseOrder.TakenViaSubcontractor} { ErrorMessages.PartyIsNotASubcontractor}";
            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedOrderedByDeriveValidationErrorAtLeastOne()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("AssertAtLeastOne: PurchaseOrder.TakenViaSupplier\nPurchaseOrder.TakenViaSubcontractor"));
        }

        [Fact]
        public void ChangedTakenViaSupplierDeriveValidationErrorAtMostOne()
        {
            var order = new PurchaseOrderBuilder(this.Session)
                .WithTakenViaSubcontractor(this.InternalOrganisation.ActiveSubContractors.First)
                .Build();
            this.Session.Derive(false);

            order.TakenViaSupplier = this.InternalOrganisation.ActiveSuppliers.First;

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("AssertExistsAtMostOne: PurchaseOrder.TakenViaSupplier\nPurchaseOrder.TakenViaSubcontractor"));
        }

        [Fact]
        public void ChangedTakenViaSubcontractorDeriveValidationErrorAtMostOne()
        {
            var order = new PurchaseOrderBuilder(this.Session)
                .WithTakenViaSupplier(this.InternalOrganisation.ActiveSuppliers.First)
                .Build();
            this.Session.Derive(false);

            order.TakenViaSubcontractor = this.InternalOrganisation.ActiveSubContractors.First;

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("AssertExistsAtMostOne: PurchaseOrder.TakenViaSupplier\nPurchaseOrder.TakenViaSubcontractor"));
        }

        [Fact]
        public void ChangedOrderedByDeriveInvoiceNumber()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.True(order.ExistOrderNumber);
        }

        [Fact]
        public void ChangedOrderedByDeriveSortableInvoiceNumber()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.True(order.ExistSortableOrderNumber);
        }

        [Fact]
        public void ChangedPurchaseOrderItemsDeriveValidOrderItems()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Session).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.Single(order.ValidOrderItems);
        }

        [Fact]
        public void ChangedPurchaseOrderItemPurchaseOrderItemStateDeriveValidOrderItems()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem1 = new PurchaseOrderItemBuilder(this.Session).Build();
            order.AddPurchaseOrderItem(orderItem1);
            this.Session.Derive(false);

            var orderItem2 = new PurchaseOrderItemBuilder(this.Session).Build();
            order.AddPurchaseOrderItem(orderItem2);
            this.Session.Derive(false);

            Assert.Equal(2, order.ValidOrderItems.Count);

            orderItem2.Cancel();
            this.Session.Derive(false);

            Assert.Single(order.ValidOrderItems);
        }

        [Fact]
        public void ChangedTakenViaSupplierDeriveWorkItemDescription()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var expected = $"PurchaseOrder: {order.OrderNumber} [{order.TakenViaSupplier?.PartyName}]";
            Assert.Equal(expected, order.WorkItemDescription);
        }
    }

    [Trait("Category", "Security")]
    public class PurchaseOrderDeniedPermissionDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderDeniedPermissionDerivationTests(Fixture fixture) : base(fixture)
        {
            this.deletePermission = new Permissions(this.Session).Get(this.M.PurchaseOrder.ObjectType, this.M.PurchaseOrder.Delete);
            this.setReadyPermission = new Permissions(this.Session).Get(this.M.PurchaseOrder.ObjectType, this.M.PurchaseOrder.SetReadyForProcessing);
            this.invoicePermission = new Permissions(this.Session).Get(this.M.PurchaseOrder.ObjectType, this.M.PurchaseOrder.Invoice);
            this.revisePermission = new Permissions(this.Session).Get(this.M.PurchaseOrder.ObjectType, this.M.PurchaseOrder.Revise);
            this.quickReceivePermission = new Permissions(this.Session).Get(this.M.PurchaseOrder.ObjectType, this.M.PurchaseOrder.QuickReceive);
            this.rejectPermission = new Permissions(this.Session).Get(this.M.PurchaseOrder.ObjectType, this.M.PurchaseOrder.Reject);
            this.cancelPermission = new Permissions(this.Session).Get(this.M.PurchaseOrder.ObjectType, this.M.PurchaseOrder.Cancel);
        }

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;
        private readonly Permission setReadyPermission;
        private readonly Permission invoicePermission;
        private readonly Permission revisePermission;
        private readonly Permission quickReceivePermission;
        private readonly Permission rejectPermission;
        private readonly Permission cancelPermission;

        [Fact]
        public void OnChangedPurchaseOrderStateIsCompletedDeriveInvoicePermission()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Session).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Session.Derive(false);

            order.Approve();
            this.Session.Derive(false);

            order.Send();
            this.Session.Derive(false);

            Assert.True(order.PurchaseOrderState.IsCompleted);
            Assert.DoesNotContain(this.invoicePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateInProcessDeriveInvoicePermission()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Session).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Session.Derive(false);

            order.SetReadyForProcessing();
            this.Session.Derive(false);

            Assert.True(order.PurchaseOrderState.IsInProcess);
            Assert.Contains(this.invoicePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateIsCompletedWithoutValidItemsDeriveInvoicePermission()
        {
            var order = new PurchaseOrderBuilder(this.Session)
                .WithPurchaseOrderState(new PurchaseOrderStates(this.Session).Completed)
                .Build();
            this.Session.Derive(false);

            Assert.True(order.PurchaseOrderState.IsCompleted);
            Assert.Contains(this.invoicePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderItemBillingDeriveInvoicePermission()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Session).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Session.Derive(false);

            order.Approve();
            this.Session.Derive(false);

            order.Send();
            this.Session.Derive(false);

            new OrderItemBillingBuilder(this.Session).WithOrderItem(orderItem).Build();
            this.Session.Derive(false);

            Assert.Contains(this.invoicePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateIsInProcessDeriveRevisePermission()
        {
            var order = new PurchaseOrderBuilder(this.Session)
                .WithPurchaseOrderState(new PurchaseOrderStates(this.Session).InProcess)
                .Build();
            this.Session.Derive(false);

            Assert.True(order.PurchaseOrderState.IsInProcess);
            Assert.DoesNotContain(this.revisePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateIsSentDeriveRevisePermission()
        {
            var order = new PurchaseOrderBuilder(this.Session)
                .WithPurchaseOrderState(new PurchaseOrderStates(this.Session).Sent)
                .Build();
            this.Session.Derive(false);

            Assert.True(order.PurchaseOrderState.IsSent);
            Assert.DoesNotContain(this.revisePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateIsCompletedDeriveRevisePermission()
        {
            var order = new PurchaseOrderBuilder(this.Session)
                .WithPurchaseOrderState(new PurchaseOrderStates(this.Session).Completed)
                .Build();
            this.Session.Derive(false);

            Assert.True(order.PurchaseOrderState.IsCompleted);
            Assert.DoesNotContain(this.revisePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateCreatedDeriveRevisePermission()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Contains(this.revisePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseInvoicePurchaseOrdersDeriveRevisePermission()
        {
            var order = new PurchaseOrderBuilder(this.Session)
                .WithPurchaseOrderState(new PurchaseOrderStates(this.Session).Completed)
                .Build();
            this.Session.Derive(false);

            new PurchaseInvoiceBuilder(this.Session).WithPurchaseOrder(order).Build();
            this.Session.Derive(false);

            Assert.Contains(this.revisePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderShipmentStateReceivedDeriveRevisePermission()
        {
            var order = new PurchaseOrderBuilder(this.Session)
                .WithPurchaseOrderState(new PurchaseOrderStates(this.Session).Completed)
                .Build();
            this.Session.Derive(false);

            order.PurchaseOrderShipmentState = new PurchaseOrderShipmentStates(this.Session).Received;
            this.Session.Derive(false);

            Assert.Contains(this.revisePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderShipmentStateNotReceivedDeriveRevisePermission()
        {
            var order = new PurchaseOrderBuilder(this.Session)
                .WithPurchaseOrderState(new PurchaseOrderStates(this.Session).Completed)
                .Build();
            this.Session.Derive(false);

            order.PurchaseOrderShipmentState = new PurchaseOrderShipmentStates(this.Session).NotReceived;
            this.Session.Derive(false);

            Assert.DoesNotContain(this.revisePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateCreatedDeriveQuickReceivePermission()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Session).WithIsReceivable(true).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.True(orderItem.IsReceivable);
            Assert.Contains(this.quickReceivePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateSentDeriveQuickReceivePermission()
        {
            var order = new PurchaseOrderBuilder(this.Session)
                .WithPurchaseOrderState(new PurchaseOrderStates(this.Session).Sent)
                .Build();
            this.Session.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Session).WithIsReceivable(true).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Session.Derive(false);

            Assert.True(orderItem.IsReceivable);
            Assert.True(order.PurchaseOrderState.IsSent);
            Assert.Contains(this.quickReceivePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateCreatedDeriveDeletePermission()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.True(order.PurchaseOrderState.IsCreated);
            Assert.DoesNotContain(this.deletePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateSentDeriveDeletePermission()
        {
            var order = new PurchaseOrderBuilder(this.Session)
                .WithPurchaseOrderState(new PurchaseOrderStates(this.Session).Sent)
                .Build();
            this.Session.Derive(false);

            Assert.True(order.PurchaseOrderState.IsSent);
            Assert.Contains(this.deletePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseInvoicePurchaseOrdersDeriveDeletePermission()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            new PurchaseInvoiceBuilder(this.Session).WithPurchaseOrder(order).Build();
            this.Session.Derive(false);

            Assert.True(order.PurchaseOrderState.IsCreated);
            Assert.Contains(this.deletePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSerialisedItemPurchaseOrderDeriveDeletePermission()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            new SerialisedItemBuilder(this.Session).WithPurchaseOrder(order).Build();
            this.Session.Derive(false);

            Assert.True(order.PurchaseOrderState.IsCreated);
            Assert.Contains(this.deletePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortPurchaseOrderItemAssignmentPurchaseOrderDeriveDeletePermission()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Session).WithPurchaseOrder(order).Build();
            this.Session.Derive(false);

            Assert.True(order.PurchaseOrderState.IsCreated);
            Assert.Contains(this.deletePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderItemBillingDeriveDeletePermission()
        {
            var order = new PurchaseOrderBuilder(this.Session).Build();
            this.Session.Derive(false);

            var orderItem = new PurchaseOrderItemBuilder(this.Session).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Session.Derive(false);

            new OrderItemBillingBuilder(this.Session).WithOrderItem(orderItem).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderStateCreatedPurchaseOrderShipmentStateIsNaDeriveMultiplePermissions()
        {
            var order = new PurchaseOrderBuilder(this.Session)
                .WithPurchaseOrderShipmentState(new PurchaseOrderShipmentStates(this.Session).NotReceived)
                .Build();
            this.Session.Derive(false);

            Assert.True(order.PurchaseOrderShipmentState.IsNotReceived);

            Assert.Contains(this.rejectPermission, order.DeniedPermissions);
            Assert.Contains(this.cancelPermission, order.DeniedPermissions);
            Assert.Contains(this.quickReceivePermission, order.DeniedPermissions);
            Assert.Contains(this.revisePermission, order.DeniedPermissions);
            Assert.Contains(this.setReadyPermission, order.DeniedPermissions);
        }
    }
}
