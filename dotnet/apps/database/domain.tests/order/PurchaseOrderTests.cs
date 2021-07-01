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
    using Derivations.Errors;
    using Meta;
    using ContactMechanism = Domain.ContactMechanism;
    using Permission = Domain.Permission;

    public class PurchaseOrderTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenPurchaseOrderBuilder_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            var internalOrganisation = this.InternalOrganisation;
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            var order = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).Build();

            this.Transaction.Derive();

            Assert.Equal(new PurchaseOrderStates(this.Transaction).Created, order.PurchaseOrderState);
            Assert.Equal(this.Transaction.Now().Date, order.OrderDate.Date);
            Assert.Equal(this.Transaction.Now().Date, order.EntryDate.Date);
            Assert.Equal(order.PreviousTakenViaSupplier, order.TakenViaSupplier);
        }

        [Fact]
        public void GivenOrder_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier2").Build();
            var internalOrganisation = this.InternalOrganisation;
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            ContactMechanism takenViaContactMechanism = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var supplierContactMechanism = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(takenViaContactMechanism)
                .WithUseAsDefault(true)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).OrderAddress)
                .Build();
            supplier.AddPartyContactMechanism(supplierContactMechanism);

            this.Transaction.Derive();
            this.Transaction.Commit();

            var builder = new PurchaseOrderBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithTakenViaSupplier(supplier);
            builder.Build();

            Assert.False(this.Derive().HasErrors);

            builder.WithAssignedTakenViaContactMechanism(takenViaContactMechanism);
            builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenPurchaseOrder_WhenDeriving_ThenTakenViaSupplierMustBeInSupplierRelationship()
        {
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier2").Build();

            var order = new PurchaseOrderBuilder(this.Transaction)
                .WithTakenViaSupplier(supplier)
                .Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.PartyIsNotASupplier));

            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenOrder_WhenDeriving_ThenLocaleMustExist()
        {
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier2").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            ContactMechanism takenViaContactMechanism = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var supplierContactMechanism = new PartyContactMechanismBuilder(this.Transaction)
                .WithContactMechanism(takenViaContactMechanism)
                .WithUseAsDefault(true)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).OrderAddress)
                .Build();
            supplier.AddPartyContactMechanism(supplierContactMechanism);

            var order = new PurchaseOrderBuilder(this.Transaction)
                .WithTakenViaSupplier(supplier)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(order.OrderedBy.Locale, order.DerivedLocale);
        }

        [Fact]
        public void GivenPurchaseOrder_WhenGettingOrderNumberWithoutFormat_ThenOrderNumberShouldBeReturned()
        {
            var internalOrganisation = this.InternalOrganisation;
            internalOrganisation.RemovePurchaseOrderNumberPrefix();

            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            this.Transaction.Derive();

            var order1 = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).Build();

            this.Transaction.Derive();

            Assert.Equal("1", order1.OrderNumber);

            var order2 = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).Build();

            this.Transaction.Derive();

            Assert.Equal("2", order2.OrderNumber);
        }

        [Fact]
        public void GivenPurchaseOrder_WhenGettingOrderNumberWithFormat_ThenFormattedOrderNumberShouldBeReturned()
        {
            this.InternalOrganisation.InvoiceSequence = new InvoiceSequences(this.Transaction).EnforcedSequence;
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            this.Transaction.Derive();

            var internalOrganisation = this.InternalOrganisation;
            internalOrganisation.PurchaseOrderNumberPrefix = "the format is ";

            var order1 = new PurchaseOrderBuilder(this.Transaction)
                .WithTakenViaSupplier(supplier)
                .Build();

            this.Transaction.Derive();

            Assert.Equal("the format is 1", order1.OrderNumber);

            var order2 = new PurchaseOrderBuilder(this.Transaction)
                .WithTakenViaSupplier(supplier)
                .Build();

            this.Transaction.Derive();

            Assert.Equal("the format is 2", order2.OrderNumber);
        }

        [Fact]
        public void GivenBilledToWithoutOrderNumberPrefix_WhenDeriving_ThenSortableOrderNumberIsSet()
        {
            this.InternalOrganisation.RemovePurchaseOrderNumberPrefix();
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            this.Transaction.Derive();

            var order = new PurchaseOrderBuilder(this.Transaction)
                .WithTakenViaSupplier(supplier)
                .Build();
            this.Transaction.Derive();

            order.SetReadyForProcessing();
            this.Transaction.Derive();

            Assert.Equal(int.Parse(order.OrderNumber), order.SortableOrderNumber);
        }

        [Fact]
        public void GivenBilledToWithOrderNumberPrefix_WhenDeriving_ThenSortableOrderNumberIsSet()
        {
            this.InternalOrganisation.InvoiceSequence = new InvoiceSequences(this.Transaction).EnforcedSequence;
            this.InternalOrganisation.PurchaseOrderNumberPrefix = "prefix-";
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            this.Transaction.Derive();

            var order = new PurchaseOrderBuilder(this.Transaction)
                .WithTakenViaSupplier(supplier)
                .Build();
            this.Transaction.Derive();

            order.SetReadyForProcessing();
            this.Transaction.Derive();

            Assert.Equal(int.Parse(order.OrderNumber.Split('-')[1]), order.SortableOrderNumber);
        }

        [Fact]
        public void GivenBilledToWithParametrizedOrderNumberPrefix_WhenDeriving_ThenSortableOrderNumberIsSet()
        {
            this.InternalOrganisation.InvoiceSequence = new InvoiceSequences(this.Transaction).EnforcedSequence;
            this.InternalOrganisation.PurchaseOrderNumberPrefix = "prefix-{year}-";
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            this.Transaction.Derive();

            var order = new PurchaseOrderBuilder(this.Transaction)
                .WithTakenViaSupplier(supplier)
                .Build();
            this.Transaction.Derive();

            order.SetReadyForProcessing();
            this.Transaction.Derive();

            var number = int.Parse(order.OrderNumber.Split('-').Last()).ToString("000000");
            Assert.Equal(int.Parse(string.Concat(this.Transaction.Now().Date.Year.ToString(), number)), order.SortableOrderNumber);
        }

        [Fact]
        public void GivenPurchaseOrder_WhenConfirming_ThenAllValidItemsAreInConfirmedState()
        {
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier2").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .Build();

            var order = new PurchaseOrderBuilder(this.Transaction)
                .WithTakenViaSupplier(supplier)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).Exempt)
                .Build();

            var item1 = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(1).Build();
            var item2 = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(2).Build();
            var item3 = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(3).Build();
            var item4 = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(4).Build();
            order.AddPurchaseOrderItem(item1);
            order.AddPurchaseOrderItem(item2);
            order.AddPurchaseOrderItem(item3);
            order.AddPurchaseOrderItem(item4);

            this.Transaction.Derive();

            order.SetReadyForProcessing();
            this.Transaction.Derive();

            item4.Cancel();
            this.Transaction.Derive();

            Assert.Equal(3, order.ValidOrderItems.Count());
            Assert.Contains(item1, order.ValidOrderItems);
            Assert.Contains(item2, order.ValidOrderItems);
            Assert.Contains(item3, order.ValidOrderItems);
            Assert.Equal(new PurchaseOrderItemStates(this.Transaction).InProcess, item1.PurchaseOrderItemState);
            Assert.Equal(new PurchaseOrderItemStates(this.Transaction).InProcess, item2.PurchaseOrderItemState);
            Assert.Equal(new PurchaseOrderItemStates(this.Transaction).InProcess, item3.PurchaseOrderItemState);
            Assert.Equal(new PurchaseOrderItemStates(this.Transaction).Cancelled, item4.PurchaseOrderItemState);
        }

        [Fact]
        public void GivenPurchaseOrder_WhenOrdering_ThenAllValidItemsAreInInProcessState()
        {
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier2").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(supplier).Build();

            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .Build();

            var order = new PurchaseOrderBuilder(this.Transaction)
                .WithTakenViaSupplier(supplier)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).Exempt)
                .Build();

            var item1 = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(1).Build();
            var item2 = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(2).Build();
            var item3 = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(3).Build();
            order.AddPurchaseOrderItem(item1);
            order.AddPurchaseOrderItem(item2);
            order.AddPurchaseOrderItem(item3);

            this.Transaction.Derive();

            order.SetReadyForProcessing();
            this.Transaction.Derive();

            Assert.Equal(3, order.ValidOrderItems.Count());
            Assert.Contains(item1, order.ValidOrderItems);
            Assert.Contains(item2, order.ValidOrderItems);
            Assert.Contains(item3, order.ValidOrderItems);
            Assert.Equal(new PurchaseOrderItemStates(this.Transaction).InProcess, item1.PurchaseOrderItemState);
            Assert.Equal(new PurchaseOrderItemStates(this.Transaction).InProcess, item2.PurchaseOrderItemState);
            Assert.Equal(new PurchaseOrderItemStates(this.Transaction).InProcess, item3.PurchaseOrderItemState);
        }
    }

    public class PurchaseOrderCreatedRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderCreatedRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseOrderStateDeriveDerivedLocale()
        {
            var swedishLocale = new LocaleBuilder(this.Transaction)
               .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "SE"))
               .WithLanguage(new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "sv"))
               .Build();

            var order = new PurchaseOrderBuilder(this.Transaction).WithPurchaseOrderState(new PurchaseOrderStates(this.Transaction).Cancelled).Build();
            this.Derive();

            this.InternalOrganisation.Locale = swedishLocale;
            this.Derive();

            Assert.False(order.ExistDerivedLocale);

            order.PurchaseOrderState = new PurchaseOrderStates(this.Transaction).Created;
            this.Derive();

            Assert.Equal(order.DerivedLocale, swedishLocale);
        }

        [Fact]
        public void ChangedOrderedByDeriveDerivedLocaleFromOrderedByLocale()
        {
            var swedishLocale = new LocaleBuilder(this.Transaction)
               .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "SE"))
               .WithLanguage(new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "sv"))
               .Build();

            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            this.InternalOrganisation.Locale = swedishLocale;
            this.Derive();

            Assert.Equal(order.DerivedLocale, swedishLocale);
        }

        [Fact]
        public void ChangedLocaleDeriveDerivedLocaleFromLocale()
        {
            var swedishLocale = new LocaleBuilder(this.Transaction)
               .WithCountry(new Countries(this.Transaction).FindBy(this.M.Country.IsoCode, "SE"))
               .WithLanguage(new Languages(this.Transaction).FindBy(this.M.Language.IsoCode, "sv"))
               .Build();

            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.Locale = swedishLocale;
            this.Derive();

            Assert.Equal(order.DerivedLocale, swedishLocale);
        }

        [Fact]
        public void ChangedAssignedVatRegimeDeriveDerivedVatRegime()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.AssignedVatRegime = new VatRegimes(this.Transaction).ServiceB2B;
            this.Derive();

            Assert.Equal(order.DerivedVatRegime, order.AssignedVatRegime);
        }

        [Fact]
        public void ChangedAssignedIrpfRegimeDeriveDerivedIrpfRegime()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.AssignedIrpfRegime = new IrpfRegimes(this.Transaction).Assessable15;
            this.Derive();

            Assert.Equal(order.DerivedIrpfRegime, order.AssignedIrpfRegime);
        }

        [Fact]
        public void ChangedAssignedCurrencyDeriveDerivedCurrency()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var swedishKrona = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "SEK");
            order.AssignedCurrency = swedishKrona;
            this.Derive();

            Assert.Equal(order.DerivedCurrency, order.AssignedCurrency);
        }

        [Fact]
        public void ChangedOrderedByPreferredCurrencyDerivedCurrency()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var swedishKrona = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "SEK");
            order.OrderedBy.PreferredCurrency = swedishKrona;
            this.Derive();

            Assert.Equal(order.DerivedCurrency, order.OrderedBy.PreferredCurrency);
        }

        [Fact]
        public void ChangedAssignedTakenViaContactMechanismDeriveDerivedTakenViaContactMechanism()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.AssignedTakenViaContactMechanism = new PostalAddressBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(order.DerivedTakenViaContactMechanism, order.AssignedTakenViaContactMechanism);
        }

        [Fact]
        public void ChangedTakenByOrderAddressDeriveDerivedTakenViaContactMechanism()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.FirstOrDefault();
            var order = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).Build();
            this.Derive();

            supplier.OrderAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(order.DerivedTakenViaContactMechanism, supplier.OrderAddress);
        }

        [Fact]
        public void ChangedAssignedBillToContactMechanismDeriveDeriveDerivedBillToContactMechanism()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.AssignedBillToContactMechanism = new PostalAddressBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Equal(order.DerivedBillToContactMechanism, order.AssignedBillToContactMechanism);
        }

        [Fact]
        public void ChangedOrderByBillingAddressDeriveDeriveDerivedBillToContactMechanism()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var postalAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.InternalOrganisation.BillingAddress = postalAddress;
            this.Derive();

            Assert.Equal(order.DerivedBillToContactMechanism, postalAddress);
        }

        [Fact]
        public void ChangedOrderByGeneralCorrespondenceDeriveDeriveDerivedBillToContactMechanism()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var postalAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.InternalOrganisation.RemoveBillingAddress();
            this.InternalOrganisation.GeneralCorrespondence = postalAddress;
            this.Derive();

            Assert.Equal(order.DerivedBillToContactMechanism, postalAddress);
        }

        [Fact]
        public void ChangedAssignedShipToAddressDeriveDeriveDerivedShipToAddress()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var postalAddress = new PostalAddressBuilder(this.Transaction).Build();
            order.AssignedShipToAddress = postalAddress;
            this.Derive();

            Assert.Equal(order.DerivedShipToAddress, postalAddress);
        }

        [Fact]
        public void ChangedOrderedByShippingAddressDeriveDeriveDerivedShipToAddress()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var postalAddress = new PostalAddressBuilder(this.Transaction).Build();
            this.InternalOrganisation.ShippingAddress = postalAddress;
            this.Derive();

            Assert.Equal(order.DerivedShipToAddress, postalAddress);
        }

        [Fact]
        public void ChangedOrderDateDeriveVatRate()
        {
            var vatRegime = new VatRegimes(this.Transaction).SpainReduced;
            vatRegime.VatRates.ElementAt(0).ThroughDate = this.Transaction.Now().AddDays(-1).Date;
            this.Derive();

            var newVatRate = new VatRateBuilder(this.Transaction).WithFromDate(this.Transaction.Now().Date).WithRate(11).Build();
            vatRegime.AddVatRate(newVatRate);
            this.Derive();

            var order = new PurchaseOrderBuilder(this.Transaction)
                .WithOrderDate(this.Transaction.Now().AddDays(-1).Date)
                .WithAssignedVatRegime(vatRegime).Build();
            this.Derive();

            Assert.NotEqual(newVatRate, order.DerivedVatRate);

            order.OrderDate = this.Transaction.Now().AddDays(1).Date;
            this.Derive();

            Assert.Equal(newVatRate, order.DerivedVatRate);
        }
    }

    public class PurchaseOrderAwaitingApprovalLevel1RuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderAwaitingApprovalLevel1RuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseOrderStateCreateApprovalTask()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.FirstOrDefault();
            var supplierRelationship = supplier.SupplierRelationshipsWhereSupplier.First(v => v.InternalOrganisation.Equals(this.InternalOrganisation));
            supplierRelationship.NeedsApproval = true;
            supplierRelationship.ApprovalThresholdLevel1 = 0;

            var order = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithQuantityOrdered(1).WithAssignedUnitPrice(100).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            order.SetReadyForProcessing();
            this.Derive();

            Assert.True(order.ExistPurchaseOrderApprovalsLevel1WherePurchaseOrder);
        }
    }

    public class PurchaseOrderAwaitingApprovalLevel2RuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderAwaitingApprovalLevel2RuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseOrderStateCreateApprovalTask()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.FirstOrDefault();
            var supplierRelationship = supplier.SupplierRelationshipsWhereSupplier.First(v => v.InternalOrganisation.Equals(this.InternalOrganisation));
            supplierRelationship.NeedsApproval = true;
            supplierRelationship.ApprovalThresholdLevel1 = 0;
            supplierRelationship.ApprovalThresholdLevel2 = 0;

            var order = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithQuantityOrdered(1).WithAssignedUnitPrice(100).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            order.SetReadyForProcessing();
            this.Derive();

            var approvalLevel1 = order.PurchaseOrderApprovalsLevel1WherePurchaseOrder.FirstOrDefault();
            approvalLevel1.Approve();
            this.Derive();

            Assert.True(order.ExistPurchaseOrderApprovalsLevel2WherePurchaseOrder);
        }
    }

    public class PurchaseOrderRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedOrderedByThrowValidationError()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.OrderedBy = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.InternalOrganisationChanged));
        }

        [Fact]
        public void ChangedInternalOrganisationActiveSuppliersThrowValidationError()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(this.InternalOrganisation.ActiveSuppliers.FirstOrDefault()).Build();
            this.Derive();

            var supplierRelationship = this.InternalOrganisation.ActiveSuppliers.First().SupplierRelationshipsWhereSupplier.FirstOrDefault();
            supplierRelationship.ThroughDate = supplierRelationship.FromDate;

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.PartyIsNotASupplier));
        }

        [Fact]
        public void ChangedOrderDateThrowValidationErrorPartyIsNotASupplier()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(this.InternalOrganisation.ActiveSuppliers.FirstOrDefault()).Build();
            this.Derive();

            var supplierRelationship = this.InternalOrganisation.ActiveSuppliers.First().SupplierRelationshipsWhereSupplier.FirstOrDefault();
            order.OrderDate = supplierRelationship.FromDate.AddDays(-1);

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.PartyIsNotASupplier));
        }

        [Fact]
        public void ChangedTakenViaSupplierThrowValidationError()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.TakenViaSupplier = new OrganisationBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.PartyIsNotASupplier));
        }

        [Fact]
        public void ChangedTakenViaSubcontractorThrowValidationError()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.TakenViaSubcontractor = new OrganisationBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.PartyIsNotASubcontractor));
        }

        [Fact]
        public void ChangedInternalOrganisationActiveSubcontractorsThrowValidationError()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSubcontractor(this.InternalOrganisation.ActiveSubContractors.FirstOrDefault()).Build();
            this.Derive();

            var subcontractorRelationship = this.InternalOrganisation.ActiveSubContractors.First().SubContractorRelationshipsWhereSubContractor.FirstOrDefault();
            subcontractorRelationship.ThroughDate = subcontractorRelationship.FromDate;

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.PartyIsNotASubcontractor));
        }

        [Fact]
        public void ChangedOrderDateThrowValidationErrorPartyIsNotASubcontractor()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSubcontractor(this.InternalOrganisation.ActiveSubContractors.FirstOrDefault()).Build();
            this.Derive();

            var subcontractorRelationship = this.InternalOrganisation.ActiveSubContractors.First().SubContractorRelationshipsWhereSubContractor.FirstOrDefault();
            order.OrderDate = subcontractorRelationship.FromDate.AddDays(-1);

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains(ErrorMessages.PartyIsNotASubcontractor));
        }

        [Fact]
        public void ChangedOrderedByDeriveValidationErrorAtLeastOne()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.OfType<DerivationErrorAtLeastOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.PurchaseOrder.TakenViaSupplier,
                this.M.PurchaseOrder.TakenViaSubcontractor,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedTakenViaSupplierDeriveValidationErrorAtMostOne()
        {
            var order = new PurchaseOrderBuilder(this.Transaction)
                .WithTakenViaSubcontractor(this.InternalOrganisation.ActiveSubContractors.FirstOrDefault())
                .Build();
            this.Derive();

            order.TakenViaSupplier = this.InternalOrganisation.ActiveSuppliers.FirstOrDefault();

            var errors = this.Derive().Errors.OfType<DerivationErrorAtMostOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.PurchaseOrder.TakenViaSupplier,
                this.M.PurchaseOrder.TakenViaSubcontractor,
           }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedTakenViaSubcontractorDeriveValidationErrorAtMostOne()
        {
            var order = new PurchaseOrderBuilder(this.Transaction)
                .WithTakenViaSupplier(this.InternalOrganisation.ActiveSuppliers.FirstOrDefault())
                .Build();
            this.Derive();

            order.TakenViaSubcontractor = this.InternalOrganisation.ActiveSubContractors.FirstOrDefault();

            var errors = this.Derive().Errors.OfType<DerivationErrorAtMostOne>();
            Assert.Equal(new IRoleType[]
            {
                this.M.PurchaseOrder.TakenViaSupplier,
                this.M.PurchaseOrder.TakenViaSubcontractor,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedOrderedByDeriveInvoiceNumber()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            Assert.True(order.ExistOrderNumber);
        }

        [Fact]
        public void ChangedOrderedByDeriveSortableInvoiceNumber()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            Assert.True(order.ExistSortableOrderNumber);
        }

        [Fact]
        public void ChangedPurchaseOrderItemsDeriveValidOrderItems()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            Assert.Single(order.ValidOrderItems);
        }

        [Fact]
        public void ChangedPurchaseOrderItemPurchaseOrderItemStateDeriveValidOrderItems()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem1 = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem1);
            this.Derive();

            var orderItem2 = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem2);
            this.Derive();

            Assert.Equal(2, order.ValidOrderItems.Count());

            orderItem2.Cancel();
            this.Derive();

            Assert.Single(order.ValidOrderItems);
        }

        [Fact]
        public void ChangedTakenViaSupplierDeriveWorkItemDescription()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var expected = $"PurchaseOrder: {order.OrderNumber} [{order.TakenViaSupplier?.PartyName}]";
            Assert.Equal(expected, order.WorkItemDescription);
        }
    }

    public class PurchaseOrderStateRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderStateRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseOrderItemsDerivePurchaseOrderShipmentStateNa()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithIsReceivable(false)
                .Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            Assert.Equal(new PurchaseOrderShipmentStates(this.Transaction).Na, order.PurchaseOrderShipmentState);
        }

        [Fact]
        public void ChangedPurchaseOrderItemPurchaseOrderItemShipmentStateDerivePurchaseOrderShipmentStateReceived()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem1 = new PurchaseOrderItemBuilder(this.Transaction)
                .WithIsReceivable(true)
                .Build();
            order.AddPurchaseOrderItem(orderItem1);
            this.Derive();

            var orderItem2 = new PurchaseOrderItemBuilder(this.Transaction)
                .WithIsReceivable(false)
                .Build();
            order.AddPurchaseOrderItem(orderItem2);
            this.Derive();

            orderItem1.PurchaseOrderItemShipmentState = new PurchaseOrderItemShipmentStates(this.Transaction).Received;
            this.Derive();

            Assert.Equal(new PurchaseOrderShipmentStates(this.Transaction).Received, order.PurchaseOrderShipmentState);
        }

        [Fact]
        public void ChangedPurchaseOrderItemPurchaseOrderItemShipmentStateDerivePurchaseOrderShipmentStateNotReceived()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithIsReceivable(true)
                .Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            orderItem.PurchaseOrderItemShipmentState = new PurchaseOrderItemShipmentStates(this.Transaction).NotReceived;
            this.Derive();

            Assert.Equal(new PurchaseOrderShipmentStates(this.Transaction).NotReceived, order.PurchaseOrderShipmentState);
        }

        [Fact]
        public void ChangedPurchaseOrderItemPurchaseOrderItemShipmentStateDerivePurchaseOrderShipmentStatePartiallyReceived()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem1 = new PurchaseOrderItemBuilder(this.Transaction)
                .WithIsReceivable(true)
                .Build();
            order.AddPurchaseOrderItem(orderItem1);
            this.Derive();

            var orderItem2 = new PurchaseOrderItemBuilder(this.Transaction)
                .WithIsReceivable(true)
                .Build();
            order.AddPurchaseOrderItem(orderItem2);
            this.Derive();

            orderItem1.PurchaseOrderItemShipmentState = new PurchaseOrderItemShipmentStates(this.Transaction).Received;
            this.Derive();

            Assert.Equal(new PurchaseOrderShipmentStates(this.Transaction).PartiallyReceived, order.PurchaseOrderShipmentState);
        }

        [Fact]
        public void ChangedPurchaseOrderItemsDerivePurchaseOrderPaymentStateNotPaid()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem1 = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem1);
            this.Derive();

            Assert.Equal(new PurchaseOrderPaymentStates(this.Transaction).NotPaid, order.PurchaseOrderPaymentState);
        }

        [Fact]
        public void ChangedPurchaseOrderItemsDerivePurchaseOrderPaymentStatePaid()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem1 = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPurchaseOrderItemPaymentState(new PurchaseOrderItemPaymentStates(this.Transaction).Paid)
                .Build();
            order.AddPurchaseOrderItem(orderItem1);
            this.Derive();

            Assert.Equal(new PurchaseOrderPaymentStates(this.Transaction).Paid, order.PurchaseOrderPaymentState);
        }

        [Fact]
        public void ChangedPurchaseOrderItemsDerivePurchaseOrderPaymentStatePartiallyPaid()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem1 = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPurchaseOrderItemPaymentState(new PurchaseOrderItemPaymentStates(this.Transaction).Paid)
                .Build();
            order.AddPurchaseOrderItem(orderItem1);
            this.Derive();

            var orderItem2 = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPurchaseOrderItemPaymentState(new PurchaseOrderItemPaymentStates(this.Transaction).NotPaid)
                .Build();
            order.AddPurchaseOrderItem(orderItem2);
            this.Derive();

            Assert.Equal(new PurchaseOrderPaymentStates(this.Transaction).PartiallyPaid, order.PurchaseOrderPaymentState);
        }

        [Fact]
        public void ChangedPurchaseOrderStateDerivePurchaseOrderState()
        {
            var order = new PurchaseOrderBuilder(this.Transaction)
                .WithPurchaseOrderShipmentState(new PurchaseOrderShipmentStates(this.Transaction).Received)
                .Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            order.PurchaseOrderState = new PurchaseOrderStates(this.Transaction).Sent;
            this.Derive();

            Assert.Equal(new PurchaseOrderStates(this.Transaction).Completed, order.PurchaseOrderState);
        }

        [Fact]
        public void ChangedPurchaseOrderShipmentStateDerivePurchaseOrderState()
        {
            var order = new PurchaseOrderBuilder(this.Transaction)
                .WithPurchaseOrderState(new PurchaseOrderStates(this.Transaction).Sent)
                .Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithIsReceivable(true)
                .WithPurchaseOrderItemShipmentState(new PurchaseOrderItemShipmentStates(this.Transaction).Received)
                .WithQuantityOrdered(1)
                .Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            new ShipmentReceiptBuilder(this.Transaction).WithOrderItem(orderItem).WithQuantityAccepted(1).Build();
            this.Derive();

            Assert.Equal(new PurchaseOrderStates(this.Transaction).Completed, order.PurchaseOrderState);
        }

        [Fact]
        public void ChangedPurchaseOrderPaymentStateDerivePurchaseOrderState()
        {
            var order = new PurchaseOrderBuilder(this.Transaction)
                .WithPurchaseOrderState(new PurchaseOrderStates(this.Transaction).Completed)
                .Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            orderItem.PurchaseOrderItemPaymentState = new PurchaseOrderItemPaymentStates(this.Transaction).Paid;
            this.Derive();

            Assert.Equal(new PurchaseOrderStates(this.Transaction).Finished, order.PurchaseOrderState);
        }
    }

    public class PurchaseOrderPriceRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderPriceRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedValidOrderItemsCalculatePrice()
        {
            var invoice = this.InternalOrganisation.CreatePurchaseOrderWithSerializedItem();
            this.Transaction.Derive();

            Assert.True(invoice.TotalIncVat > 0);
            var totalIncVatBefore = invoice.TotalIncVat;

            invoice.PurchaseOrderItems.First().Cancel();
            this.Transaction.Derive();

            Assert.Equal(invoice.TotalIncVat, totalIncVatBefore - invoice.PurchaseOrderItems.First().TotalIncVat);
        }

        [Fact]
        public void ChangedDerivationTriggerCalculatePrice()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            var supplierOffering = new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(this.InternalOrganisation.ActiveSuppliers.ElementAt(0))
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(this.InternalOrganisation.ActiveSuppliers.ElementAt(0)).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(1).Build();
            invoice.AddPurchaseOrderItem(invoiceItem);
            this.Derive();

            Assert.Equal(supplierOffering.Price, invoice.TotalIncVat);

            supplierOffering.Price = 2;
            this.Derive();

            Assert.Equal(supplierOffering.Price, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedSalesOrderItemQuantityOrderedCalculatePrice()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            var invoice = new PurchaseOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            invoice.AddPurchaseOrderItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            invoiceItem.QuantityOrdered = 2;
            this.Derive();

            Assert.Equal(2, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedSalesOrderItemAssignedUnitPriceCalculatePrice()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            var invoice = new PurchaseOrderBuilder(this.Transaction).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            invoice.AddPurchaseOrderItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            invoiceItem.AssignedUnitPrice = 3;
            this.Derive();

            Assert.Equal(3, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedSalesOrderItemPartCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part1 = new UnifiedGoodBuilder(this.Transaction).Build();
            var part2 = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part1)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part2)
                .WithSupplier(supplier)
                .WithPrice(2)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part1).WithQuantityOrdered(1).Build();
            invoice.AddPurchaseOrderItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            invoiceItem.Part = part2;
            this.Derive();

            Assert.Equal(2, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedTakenViaSupplierCalculatePrice()
        {
            var supplier1 = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var supplier2 = this.InternalOrganisation.CreateSupplier(this.Transaction.Faker());
            var part = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier1)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier2)
                .WithPrice(2)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier1).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(1).Build();
            invoice.AddPurchaseOrderItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            invoice.TakenViaSupplier = supplier2;
            this.Derive();

            Assert.Equal(2, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedRoleSalesOrderItemDiscountAdjustmentsCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(1).Build();
            invoice.AddPurchaseOrderItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoiceItem.AddDiscountAdjustment(discount);
            this.Derive();

            Assert.Equal(0.9M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedPurchaseOrderItemDiscountAdjustmentPercentageCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(1).Build();
            invoice.AddPurchaseOrderItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoiceItem.AddDiscountAdjustment(discount);
            this.Derive();

            Assert.Equal(0.9M, invoice.TotalIncVat);

            discount.Percentage = 20M;
            this.Derive();

            Assert.Equal(0.8M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedPurchaseOrderItemDiscountAdjustmentAmountCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(1).Build();
            invoice.AddPurchaseOrderItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            invoiceItem.AddDiscountAdjustment(discount);
            this.Derive();

            Assert.Equal(0.5M, invoice.TotalIncVat);

            discount.Amount = 0.4M;
            this.Derive();

            Assert.Equal(0.6M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedPurchaseOrderItemSurchargeAdjustmentsCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(1).Build();
            invoice.AddPurchaseOrderItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoiceItem.AddSurchargeAdjustment(surcharge);
            this.Derive();

            Assert.Equal(1.1M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedPurchaseOrderItemSurchargeAdjustmentPercentageCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(1).Build();
            invoice.AddPurchaseOrderItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoiceItem.AddSurchargeAdjustment(surcharge);
            this.Derive();

            Assert.Equal(1.1M, invoice.TotalIncVat);

            surcharge.Percentage = 20M;
            this.Derive();

            Assert.Equal(1.2M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedPurchaseOrderItemSurchargeAdjustmentAmountCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(1).Build();
            invoice.AddPurchaseOrderItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            invoiceItem.AddSurchargeAdjustment(surcharge);
            this.Derive();

            Assert.Equal(1.5M, invoice.TotalIncVat);

            surcharge.Amount = 0.4M;
            this.Derive();

            Assert.Equal(1.4M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedDiscountAdjustmentsCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(1).Build();
            invoice.AddPurchaseOrderItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoice.AddOrderAdjustment(discount);
            this.Derive();

            Assert.Equal(0.9M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedDiscountAdjustmentPercentageCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(1).Build();
            invoice.AddPurchaseOrderItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoice.AddOrderAdjustment(discount);
            this.Derive();

            Assert.Equal(0.9M, invoice.TotalIncVat);

            discount.Percentage = 20M;
            this.Derive();

            Assert.Equal(0.8M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedDiscountAdjustmentAmountCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(1).Build();
            invoice.AddPurchaseOrderItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var discount = new DiscountAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            invoice.AddOrderAdjustment(discount);
            this.Derive();

            Assert.Equal(0.5M, invoice.TotalIncVat);

            discount.Amount = 0.4M;
            this.Derive();

            Assert.Equal(0.6M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedSurchargeAdjustmentsCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(1).Build();
            invoice.AddPurchaseOrderItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoice.AddOrderAdjustment(surcharge);
            this.Derive();

            Assert.Equal(1.1M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedSurchargeAdjustmentPercentageCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(1).Build();
            invoice.AddPurchaseOrderItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(10).Build();
            invoice.AddOrderAdjustment(surcharge);
            this.Derive();

            Assert.Equal(1.1M, invoice.TotalIncVat);

            surcharge.Percentage = 20M;
            this.Derive();

            Assert.Equal(1.2M, invoice.TotalIncVat);
        }

        [Fact]
        public void ChangedSurchargeAdjustmentAmountCalculatePrice()
        {
            var supplier = this.InternalOrganisation.ActiveSuppliers.ElementAt(0);
            var part = new UnifiedGoodBuilder(this.Transaction).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(supplier)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddDays(-1))
                .Build();

            var invoice = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).WithOrderDate(this.Transaction.Now()).Build();
            this.Derive();

            var invoiceItem = new PurchaseOrderItemBuilder(this.Transaction).WithPart(part).WithQuantityOrdered(1).Build();
            invoice.AddPurchaseOrderItem(invoiceItem);
            this.Derive();

            Assert.Equal(1, invoice.TotalIncVat);

            var surcharge = new SurchargeAdjustmentBuilder(this.Transaction).WithAmount(0.5M).Build();
            invoice.AddOrderAdjustment(surcharge);
            this.Derive();

            Assert.Equal(1.5M, invoice.TotalIncVat);

            surcharge.Amount = 0.4M;
            this.Derive();

            Assert.Equal(1.4M, invoice.TotalIncVat);
        }
    }

    [Trait("Category", "Security")]
    public class PurchaseOrderDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderDeniedPermissionRuleTests(Fixture fixture) : base(fixture)
        {
            this.deletePermission = new Permissions(this.Transaction).Get(this.M.PurchaseOrder, this.M.PurchaseOrder.Delete);
            this.setReadyPermission = new Permissions(this.Transaction).Get(this.M.PurchaseOrder, this.M.PurchaseOrder.SetReadyForProcessing);
            this.invoicePermission = new Permissions(this.Transaction).Get(this.M.PurchaseOrder, this.M.PurchaseOrder.Invoice);
            this.revisePermission = new Permissions(this.Transaction).Get(this.M.PurchaseOrder, this.M.PurchaseOrder.Revise);
            this.quickReceivePermission = new Permissions(this.Transaction).Get(this.M.PurchaseOrder, this.M.PurchaseOrder.QuickReceive);
            this.rejectPermission = new Permissions(this.Transaction).Get(this.M.PurchaseOrder, this.M.PurchaseOrder.Reject);
            this.cancelPermission = new Permissions(this.Transaction).Get(this.M.PurchaseOrder, this.M.PurchaseOrder.Cancel);
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
        public void OnChangedTransitionalDeniedPermissionsDeriveInvoicePermissionAllowed()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            order.PurchaseOrderState = new PurchaseOrderStates(this.Transaction).Completed;
            this.Derive();

            Assert.DoesNotContain(this.invoicePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedTransitionalDeniedPermissionsDeriveInvoicePermissionDenied()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            order.PurchaseOrderState = new PurchaseOrderStates(this.Transaction).Completed;
            this.Derive();

            Assert.DoesNotContain(this.invoicePermission, order.DeniedPermissions);

            order.PurchaseOrderState = new PurchaseOrderStates(this.Transaction).AwaitingApprovalLevel1;
            this.Derive();

            Assert.Contains(this.invoicePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedOrderItemBillingOrderItemDeriveInvoicePermission()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            order.PurchaseOrderState = new PurchaseOrderStates(this.Transaction).Completed;
            this.Derive();

            Assert.DoesNotContain(this.invoicePermission, order.DeniedPermissions);

            new OrderItemBillingBuilder(this.Transaction).WithOrderItem(orderItem).Build();
            this.Derive();

            Assert.Contains(this.invoicePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedTransitionalDeniedPermissionsDeriveRevisePermissionAllowed()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.PurchaseOrderState = new PurchaseOrderStates(this.Transaction).InProcess;
            this.Derive();

            Assert.DoesNotContain(this.revisePermission, order.DeniedPermissions);
        }

        [Fact]
        public void TransitionalDeniedPermissionsDeriveRevisePermissionDenied()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Contains(this.revisePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseInvoicePurchaseOrdersDeriveRevisePermission()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            order.PurchaseOrderState = new PurchaseOrderStates(this.Transaction).InProcess;
            this.Derive();

            Assert.DoesNotContain(this.revisePermission, order.DeniedPermissions);

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            invoice.AddPurchaseOrder(order);
            this.Derive();

            Assert.Contains(this.revisePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderShipmentStateDeriveRevisePermission()
        {
            var order = new PurchaseOrderBuilder(this.Transaction)
                .WithPurchaseOrderState(new PurchaseOrderStates(this.Transaction).Completed)
                .Build();
            this.Derive();

            Assert.DoesNotContain(this.revisePermission, order.DeniedPermissions);

            order.PurchaseOrderShipmentState = new PurchaseOrderShipmentStates(this.Transaction).Received;
            this.Derive();

            Assert.Contains(this.revisePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedTransitionalDeniedPermissionsDeriveQuickReceivePermissionAllowed()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithIsReceivable(true).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            Assert.Contains(this.quickReceivePermission, order.DeniedPermissions);

            order.PurchaseOrderState = new PurchaseOrderStates(this.Transaction).Sent;
            this.Derive();

            Assert.DoesNotContain(this.quickReceivePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedTransitionalDeniedPermissionsDeriveQuickReceivePermissionDenied()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).WithIsReceivable(true).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            order.PurchaseOrderState = new PurchaseOrderStates(this.Transaction).AwaitingApprovalLevel1;
            this.Derive();

            Assert.Contains(this.quickReceivePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedTransitionalDeniedPermissionsDeriveDeletePermissionAllowed()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deletePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedTransitionalDeniedPermissionsDeriveDeletePermissionDenied()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.PurchaseOrderState = new PurchaseOrderStates(this.Transaction).Sent;
            this.Derive();

            Assert.Contains(this.deletePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseInvoicePurchaseOrdersDeriveDeletePermission()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deletePermission, order.DeniedPermissions);

            invoice.AddPurchaseOrder(order);
            this.Derive();

            Assert.Contains(this.deletePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSerialisedItemPurchaseOrderDeriveDeletePermission()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deletePermission, order.DeniedPermissions);

            serialisedItem.PurchaseOrder = order;
            this.Derive();

            Assert.Contains(this.deletePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortPurchaseOrderItemAssignmentPurchaseOrderDeriveDeletePermission()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var assignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deletePermission, order.DeniedPermissions);

            assignment.PurchaseOrder = order;
            this.Derive();

            Assert.Contains(this.deletePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedOrderItemBillingOrderItemDeriveDeletePermission()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            var orderItemBilling = new OrderItemBillingBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deletePermission, order.DeniedPermissions);

            orderItemBilling.OrderItem = orderItem;
            this.Derive();

            Assert.Contains(this.deletePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderItemPurchaseOrderItemStateDeriveDeletePermission()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            Assert.DoesNotContain(this.deletePermission, order.DeniedPermissions);

            orderItem.PurchaseOrderItemState = new PurchaseOrderItemStates(this.Transaction).InProcess;
            this.Derive();

            Assert.Contains(this.deletePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedOrderShipmentOrderItemDeriveDeletePermission()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            var orderShipment = new OrderShipmentBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deletePermission, order.DeniedPermissions);

            orderShipment.OrderItem = orderItem;
            this.Derive();

            Assert.Contains(this.deletePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedOrderRequirementCommitmentOrderItemDeriveDeletePermission()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            var requirementCommitment = new OrderRequirementCommitmentBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deletePermission, order.DeniedPermissions);

            requirementCommitment.OrderItem = orderItem;
            this.Derive();

            Assert.Contains(this.deletePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortOrderItemFulfillmentDeriveDeletePermission()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction).Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Derive();

            var worktask = new WorkTaskBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deletePermission, order.DeniedPermissions);

            worktask.OrderItemFulfillment = orderItem;
            this.Derive();

            Assert.Contains(this.deletePermission, order.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderShipmentStateDeriveMultiplePermissions()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).Build();
            this.Derive();

            order.PurchaseOrderShipmentState = new PurchaseOrderShipmentStates(this.Transaction).Received;
            this.Derive();

            Assert.Contains(this.rejectPermission, order.DeniedPermissions);
            Assert.Contains(this.cancelPermission, order.DeniedPermissions);
            Assert.Contains(this.quickReceivePermission, order.DeniedPermissions);
            Assert.Contains(this.revisePermission, order.DeniedPermissions);
            Assert.Contains(this.setReadyPermission, order.DeniedPermissions);
        }
    }
}
