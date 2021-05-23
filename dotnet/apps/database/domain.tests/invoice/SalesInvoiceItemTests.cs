// <copyright file="SalesInvoiceItemTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Collections.Generic;
    using TestPopulation;
    using Database.Derivations;
    using Resources;
    using Xunit;

    public class SalesInvoiceItemTests : DomainTest, IClassFixture<Fixture>
    {
        private Part finishedGood;
        private NonUnifiedGood good;
        private Colour feature1;
        private Colour feature2;
        private Singleton internalOrganisation;
        private Organisation billToCustomer;
        private Organisation shipToCustomer;
        private Organisation supplier;
        private SupplierOffering goodPurchasePrice;
        private City mechelen;
        private City kiev;
        private PostalAddress billToContactMechanismMechelen;
        private PostalAddress shipToContactMechanismKiev;
        private BasePrice currentBasePriceGeoBoundary;
        private BasePrice currentGood1BasePrice;
        private BasePrice currentFeature1BasePrice;
        private BasePrice currentGood1Feature1BasePrice;
        private SalesInvoice invoice;
        private VatRegime vatRegime;

        public SalesInvoiceItemTests(Fixture fixture) : base(fixture)
        {
            var euro = new Currencies(this.Transaction).FindBy(this.M.Currency.IsoCode, "EUR");

            this.supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").WithLocale(new Locales(this.Transaction).EnglishGreatBritain).Build();

            this.internalOrganisation = this.Transaction.GetSingleton();

            this.vatRegime = new VatRegimes(this.Transaction).BelgiumStandard;

            this.mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            this.kiev = new CityBuilder(this.Transaction).WithName("Kiev").Build();

            this.billToContactMechanismMechelen = new PostalAddressBuilder(this.Transaction).WithAddress1("Mechelen").WithPostalAddressBoundary(this.mechelen).Build();
            this.shipToContactMechanismKiev = new PostalAddressBuilder(this.Transaction).WithAddress1("Kiev").WithPostalAddressBoundary(this.kiev).Build();
            this.billToCustomer = new OrganisationBuilder(this.Transaction).WithName("billToCustomer").WithPreferredCurrency(euro).Build();

            this.shipToCustomer = new OrganisationBuilder(this.Transaction).WithName("shipToCustomer").WithPreferredCurrency(euro).Build();

            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(this.billToCustomer).Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(this.shipToCustomer).Build();

            this.Transaction.Derive();

            this.good = new NonUnifiedGoods(this.Transaction).FindBy(this.M.Good.Name, "good1");
            this.finishedGood = this.good.Part;

            this.feature1 = new ColourBuilder(this.Transaction)
                .WithVatRegime(this.vatRegime)
                .WithName("white")
                .Build();

            this.feature2 = new ColourBuilder(this.Transaction)
                .WithName("black")
                .Build();

            this.goodPurchasePrice = new SupplierOfferingBuilder(this.Transaction)
                .WithPart(this.finishedGood)
                .WithSupplier(this.supplier)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPrice(7)
                .WithCurrency(euro)
                .Build();

            this.currentBasePriceGeoBoundary = new BasePriceBuilder(this.Transaction)
                .WithDescription("current BasePriceGeoBoundary")
                .WithGeographicBoundary(this.mechelen)
                .WithProduct(this.good)
                .WithPrice(8)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();

            // historic basePrice for good
            new BasePriceBuilder(this.Transaction).WithDescription("previous good baseprice")
                .WithProduct(this.good)
                .WithPrice(8)
                .WithFromDate(this.Transaction.Now().AddYears(-1))
                .WithThroughDate(this.Transaction.Now().AddDays(-1))
                .Build();

            this.currentGood1BasePrice = new BasePriceBuilder(this.Transaction)
                .WithDescription("current good baseprice")
                .WithProduct(this.good)
                .WithPrice(10)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            // future basePrice for good
            new BasePriceBuilder(this.Transaction).WithDescription("future good baseprice")
                .WithProduct(this.good)
                .WithPrice(11)
                .WithFromDate(this.Transaction.Now().AddYears(1))
                .Build();

            // historic basePrice for feature1
            new BasePriceBuilder(this.Transaction).WithDescription("previous feature1 price")
                .WithProductFeature(this.feature1)
                .WithPrice(0.5M)
                .WithFromDate(this.Transaction.Now().AddYears(-1))
                .WithThroughDate(this.Transaction.Now().AddDays(-1))
                .Build();

            // future basePrice for feature1
            new BasePriceBuilder(this.Transaction).WithDescription("future feature1 price")
                .WithProductFeature(this.feature1)
                .WithPrice(2.5M)
                .WithFromDate(this.Transaction.Now().AddYears(1))
                .Build();

            this.currentFeature1BasePrice = new BasePriceBuilder(this.Transaction)
                .WithDescription("current feature1 price")
                .WithProductFeature(this.feature1)
                .WithPrice(2)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            // historic basePrice for feature2
            new BasePriceBuilder(this.Transaction).WithDescription("previous feature2 price")
                .WithProductFeature(this.feature2)
                .WithPrice(2)
                .WithFromDate(this.Transaction.Now().AddYears(-1))
                .WithThroughDate(this.Transaction.Now().AddDays(-1))
                .Build();

            // future basePrice for feature2
            new BasePriceBuilder(this.Transaction)
                .WithDescription("future feature2 price")
                .WithProductFeature(this.feature2)
                .WithPrice(4)
                .WithFromDate(this.Transaction.Now().AddYears(1))
                .Build();

            new BasePriceBuilder(this.Transaction)
                .WithDescription("current feature2 price")
                .WithProductFeature(this.feature2)
                .WithPrice(3)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            // historic basePrice for good with feature1
            new BasePriceBuilder(this.Transaction).WithDescription("previous good/feature1 baseprice")
                .WithProduct(this.good)
                .WithProductFeature(this.feature1)
                .WithPrice(4)
                .WithFromDate(this.Transaction.Now().AddYears(-1))
                .WithThroughDate(this.Transaction.Now().AddDays(-1))
                .Build();

            // future basePrice for good with feature1
            new BasePriceBuilder(this.Transaction)
                .WithDescription("future good/feature1 baseprice")
                .WithProduct(this.good)
                .WithProductFeature(this.feature1)
                .WithPrice(6)
                .WithFromDate(this.Transaction.Now().AddYears(1))
                .Build();

            this.currentGood1Feature1BasePrice = new BasePriceBuilder(this.Transaction)
                .WithDescription("current good/feature1 baseprice")
                .WithProduct(this.good)
                .WithProductFeature(this.feature1)
                .WithPrice(5)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .WithAssignedBillToContactMechanism(this.billToContactMechanismMechelen)
                .WithBillToCustomer(this.billToCustomer)
                .WithAssignedShipToAddress(this.shipToContactMechanismKiev)
                .WithShipToCustomer(this.shipToCustomer)
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();
        }

        [Fact]
        public void GivenInvoiceItem_WhenBuild_ThenPostBuildRelationsMustExist()
        {
            this.InstantiateObjects(this.Transaction);

            var item = new SalesInvoiceItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithProduct(this.good).WithQuantity(1).Build();

            this.Transaction.Derive();

            Assert.Equal(new SalesInvoiceItemStates(this.Transaction).ReadyForPosting, item.SalesInvoiceItemState);
            Assert.Equal(item.SalesInvoiceItemState, item.LastSalesInvoiceItemState);
            Assert.Equal(0, item.AmountPaid);
        }

        [Fact]
        public void GivenInvoiceItemWithoutVatRegime_WhenDeriving_ThenDerivedVatRegimeIsFromInvoice()
        {
            this.InstantiateObjects(this.Transaction);

            var productItem = new InvoiceItemTypes(this.Transaction).ProductItem;

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction)
                .WithBillToCustomer(this.billToCustomer)
                .WithAssignedBillToContactMechanism(this.billToContactMechanismMechelen)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .Build();

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithQuantity(1).WithInvoiceItemType(productItem).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);

            this.Transaction.Derive();

            Assert.Equal(salesInvoice.AssignedVatRegime, invoiceItem.DerivedVatRegime);
        }

        [Fact]
        public void GivenInvoiceItemWithoutVatRegime_WhenDeriving_ThenItemDerivedVatRateIsFromInvoiceVatRegime()
        {
            this.InstantiateObjects(this.Transaction);

            var vatRate0 = new VatRates(this.Transaction).FindBy(this.M.VatRate.Rate, 0);

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction)
                .WithBillToCustomer(this.billToCustomer)
                .WithAssignedBillToContactMechanism(this.billToContactMechanismMechelen)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .Build();

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(1).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);

            this.Transaction.Derive();

            Assert.Equal(salesInvoice.DerivedVatRegime, invoiceItem.DerivedVatRegime);
            Assert.Equal(vatRate0, invoiceItem.VatRate);
        }

        [Fact]
        public void GivenInvoiceItemWithoutIrpfRegime_WhenDeriving_ThenDerivedIrpfRegimeIsFromInvoice()
        {
            this.InstantiateObjects(this.Transaction);

            var productItem = new InvoiceItemTypes(this.Transaction).ProductItem;

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction)
                .WithBillToCustomer(this.billToCustomer)
                .WithAssignedBillToContactMechanism(this.billToContactMechanismMechelen)
                .WithAssignedIrpfRegime(new IrpfRegimes(this.Transaction).Assessable19)
                .Build();

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithQuantity(1).WithInvoiceItemType(productItem).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);

            this.Transaction.Derive();

            Assert.Equal(salesInvoice.DerivedIrpfRegime, invoiceItem.DerivedIrpfRegime);
        }

        [Fact]
        public void GivenInvoiceItemWithoutIrpfRegime_WhenDeriving_ThenItemDerivedIrpfRateIsFromInvoiceIrpfRegime()
        {
            this.InstantiateObjects(this.Transaction);

            var irpfRate19 = new IrpfRates(this.Transaction).FindBy(this.M.IrpfRate.Rate, 19);

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction)
                .WithBillToCustomer(this.billToCustomer)
                .WithAssignedBillToContactMechanism(this.billToContactMechanismMechelen)
                .WithAssignedIrpfRegime(new IrpfRegimes(this.Transaction).Assessable19)
                .Build();

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(1).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);

            this.Transaction.Derive();

            Assert.Equal(salesInvoice.DerivedIrpfRegime, invoiceItem.DerivedIrpfRegime);
            Assert.Equal(irpfRate19, invoiceItem.IrpfRate);
        }

        [Fact]
        public void GivenPurchasePriceInDifferenUnitOfMeasureComparedToProduct_WhenDerivingMarkupAndProfitMargin_ThenUnitOfMeasureConversionIsPerformed()
        {
            var pair = new UnitsOfMeasure(this.Transaction).Pair;
            var piece = new UnitsOfMeasure(this.Transaction).Piece;

            var fromPairToPiece = new UnitOfMeasureConversionBuilder(this.Transaction)
                .WithToUnitOfMeasure(piece)
                .WithConversionFactor(2).Build();

            var fromPieceToPair = new UnitOfMeasureConversionBuilder(this.Transaction)
                .WithToUnitOfMeasure(pair)
                .WithConversionFactor(0.5M).Build();

            pair.AddUnitOfMeasureConversion(fromPairToPiece);
            pair.AddUnitOfMeasureConversion(fromPieceToPair);

            this.goodPurchasePrice.UnitOfMeasure = pair;
            this.good.UnitOfMeasure = piece;

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            const decimal quantity = 3;
            var item1 = new SalesInvoiceItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithQuantity(quantity)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .Build();

            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitPrice);
            Assert.Equal(Math.Round(item1.UnitPrice * 21 / 100, 2), item1.UnitVat);

            Assert.Equal(this.currentGood1BasePrice.Price * quantity, item1.TotalBasePrice);
            Assert.Equal(0, item1.TotalDiscount);
            Assert.Equal(0, item1.TotalSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price * quantity, item1.TotalExVat);
            Assert.Equal(Math.Round(item1.UnitPrice * 21 / 100, 2) * quantity, item1.TotalVat);

            var purchasePrice = this.goodPurchasePrice.Price * 0.5M;

            Assert.Equal(this.currentGood1BasePrice.Price * quantity, this.invoice.TotalBasePrice);
            Assert.Equal(0, this.invoice.TotalDiscount);
            Assert.Equal(0, this.invoice.TotalSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price * quantity, this.invoice.TotalExVat);
            Assert.Equal(Math.Round(item1.UnitPrice * 21 / 100, 2) * quantity, this.invoice.TotalVat);
        }

        [Fact]
        public void GivenInvoiceItemForGood1WithActualPrice_WhenDerivingPrices_ThenUseActualPrice()
        {
            this.InstantiateObjects(this.Transaction);

            var item1 = new SalesInvoiceItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithQuantity(3)
                .WithAssignedUnitPrice(15)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithAssignedIrpfRegime(new IrpfRegimes(this.Transaction).Assessable19)
                .Build();

            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(10, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(15, item1.UnitPrice);
            Assert.Equal(3.15m, item1.UnitVat);
            Assert.Equal(30, item1.TotalBasePrice);
            Assert.Equal(0, item1.TotalDiscount);
            Assert.Equal(0, item1.TotalSurcharge);
            Assert.Equal(45, item1.TotalExVat);
            Assert.Equal(9.45m, item1.TotalVat);
            Assert.Equal(54.45m, item1.TotalIncVat);
            Assert.Equal(8.55m, item1.TotalIrpf);
            Assert.Equal(45.90m, item1.GrandTotal);

            Assert.Equal(30, this.invoice.TotalBasePrice);
            Assert.Equal(0, this.invoice.TotalDiscount);
            Assert.Equal(0, this.invoice.TotalSurcharge);
            Assert.Equal(45, this.invoice.TotalExVat);
            Assert.Equal(9.45m, this.invoice.TotalVat);
            Assert.Equal(54.45m, this.invoice.TotalIncVat);
            Assert.Equal(45, this.invoice.TotalListPrice);
            Assert.Equal(8.55m, this.invoice.TotalIrpf);
            Assert.Equal(45.90m, this.invoice.GrandTotal);
        }

        [Fact]
        public void GivenInvoiceItemForGood1_WhenDerivingPrices_ThenUsePriceComponentsForGood1()
        {
            this.InstantiateObjects(this.Transaction);

            var irpfRegime = new IrpfRegimes(this.Transaction).Assessable19;

            const decimal quantity = 3;
            var item1 = new SalesInvoiceItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithQuantity(quantity)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithAssignedIrpfRegime(irpfRegime)
                .Build();

            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitPrice);
            Assert.Equal(Math.Round(item1.UnitPrice * 21 / 100, 2), item1.UnitVat);
            Assert.Equal(Math.Round(item1.UnitPrice * 19 / 100, 2), item1.UnitIrpf);

            Assert.Equal(this.currentGood1BasePrice.Price * quantity, item1.TotalBasePrice);
            Assert.Equal(0, item1.TotalDiscount);
            Assert.Equal(0, item1.TotalSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price * quantity, item1.TotalExVat);
            Assert.Equal(Math.Round(item1.UnitPrice * 19 / 100, 2) * quantity, item1.TotalIrpf);

            Assert.Equal(this.currentGood1BasePrice.Price * quantity, this.invoice.TotalBasePrice);
            Assert.Equal(0, this.invoice.TotalDiscount);
            Assert.Equal(0, this.invoice.TotalSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price * quantity, this.invoice.TotalExVat);
            Assert.Equal(Math.Round(item1.UnitPrice * 21 / 100, 2) * quantity, this.invoice.TotalVat);
            Assert.Equal(Math.Round(item1.UnitPrice * 19 / 100, 2) * quantity, this.invoice.TotalIrpf);
        }

        [Fact]
        public void GivenInvoiceItemForFeature1_WhenDerivingPrices_ThenUsePriceComponentsForFeature1()
        {
            this.InstantiateObjects(this.Transaction);

            const decimal quantity = 3;
            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProductFeature(this.feature1).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductFeatureItem).WithQuantity(quantity).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(this.currentFeature1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentFeature1BasePrice.Price, item1.UnitPrice);
            Assert.Equal(this.currentFeature1BasePrice.Price * quantity, item1.TotalBasePrice);
            Assert.Equal(0, item1.TotalDiscount);
            Assert.Equal(0, item1.TotalSurcharge);
            Assert.Equal(this.currentFeature1BasePrice.Price * quantity, item1.TotalExVat);

            Assert.Equal(this.currentFeature1BasePrice.Price * quantity, this.invoice.TotalBasePrice);
            Assert.Equal(0, this.invoice.TotalDiscount);
            Assert.Equal(0, this.invoice.TotalSurcharge);
            Assert.Equal(this.currentFeature1BasePrice.Price * quantity, this.invoice.TotalExVat);
        }

        [Fact]
        public void GivenProductWithMultipleBasePrices_WhenDeriving_ThenLowestUnitPriceMustBeCalculated()
        {
            this.InstantiateObjects(this.Transaction);

            this.invoice.AssignedShipToAddress = this.billToContactMechanismMechelen;

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(3).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(this.currentBasePriceGeoBoundary.Price, item1.UnitPrice);

            var invoice2 = new SalesInvoiceBuilder(this.Transaction)
                .WithBillToCustomer(this.billToCustomer)
                .WithAssignedBillToContactMechanism(this.billToContactMechanismMechelen)
                .Build();

            item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(3).Build();
            invoice2.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithDiscount_WhenDeriving_ThenUseGeneralDiscountNotBoundToProduct()
        {
            const decimal quantity = 3;
            const decimal amount = 1;
            new DiscountComponentBuilder(this.Transaction)
                .WithDescription("discount good for geo boundary")
                .WithGeographicBoundary(this.kiev)
                .WithPrice(amount)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(amount, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount, item1.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithDiscountAmountForGeoBoundary_WhenDeriving_ThenUseDiscountComponentsForGeoBoundary()
        {
            const decimal quantity = 3;
            const decimal amount = 1;
            new DiscountComponentBuilder(this.Transaction)
                .WithDescription("discount good for geo boundary")
                .WithGeographicBoundary(this.kiev)
                .WithProduct(this.good)
                .WithPrice(amount)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(amount, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount, item1.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithDiscountPercentageForGeoBoundary_WhenDeriving_ThenUseDiscountComponentsForGeoBoundary()
        {
            const decimal quantity = 3;
            const decimal percentage = 5;

            new DiscountComponentBuilder(this.Transaction)
                .WithDescription("discount good for geo boundary")
                .WithGeographicBoundary(this.kiev)
                .WithProduct(this.good)
                .WithPercentage(percentage)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            var price = this.currentGood1BasePrice.Price ?? 0;
            var amount = Math.Round(price * percentage / 100, 2);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(amount, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount, item1.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithSurchargeAmountForGeoBoundary_WhenDeriving_ThenUseSurchargeComponentsForGeoBoundary()
        {
            const decimal quantity = 3;
            const decimal amount = 1;

            new SurchargeComponentBuilder(this.Transaction)
                .WithDescription("surcharge good for geo boundary")
                .WithGeographicBoundary(this.kiev)
                .WithProduct(this.good)
                .WithPrice(amount)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(amount, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount, item1.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithSurchargePercentageForGeoBoundary_WhenDeriving_ThenUseSurchargeComponentsForGeoBoundary()
        {
            const decimal quantity = 3;
            const decimal percentage = 5;

            new SurchargeComponentBuilder(this.Transaction)
                .WithDescription("surcharge good for geo boundary")
                .WithGeographicBoundary(this.kiev)
                .WithProduct(this.good)
                .WithPercentage(percentage)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            var price = this.currentGood1BasePrice.Price ?? 0;
            var amount = Math.Round(price * percentage / 100, 2);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(amount, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount, item1.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithDiscountAmountForPartyClassification_WhenDeriving_ThenUseDiscountComponentsForPartyClassification()
        {
            const decimal quantity = 3;
            const decimal amount = 1;

            var classification = new IndustryClassificationBuilder(this.Transaction).WithName("gold customer").Build();
            new DiscountComponentBuilder(this.Transaction)
                .WithDescription("discount good for party classification")
                .WithPartyClassification(classification)
                .WithProduct(this.good)
                .WithPrice(amount)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            (this.billToCustomer).AddPartyClassification(classification);

            this.invoice.ShipToCustomer = this.shipToCustomer;

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(amount, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount, item1.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithDiscountPercentageForPartyClassification_WhenDeriving_ThenUseDiscountComponentsForPartyClassification()
        {
            const decimal quantity = 3;
            const decimal percentage = 5;

            var classification = new IndustryClassificationBuilder(this.Transaction).WithName("gold customer").Build();
            new DiscountComponentBuilder(this.Transaction)
                .WithDescription("discount good for party classification")
                .WithPartyClassification(classification)
                .WithProduct(this.good)
                .WithPercentage(percentage)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            (this.billToCustomer).AddPartyClassification(classification);

            this.invoice.ShipToCustomer = this.shipToCustomer;

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            var price = this.currentGood1BasePrice.Price ?? 0;
            var amount = Math.Round(price * percentage / 100, 2);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(amount, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount, item1.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithSurchargeAmountForPartyClassification_WhenDeriving_ThenUseSurchargeComponentsForPartyClassification()
        {
            const decimal quantity = 3;
            const decimal amount = 1;

            var classification = new IndustryClassificationBuilder(this.Transaction).WithName("gold customer").Build();
            new SurchargeComponentBuilder(this.Transaction)
                .WithDescription("discount good for party classification")
                .WithPartyClassification(classification)
                .WithProduct(this.good)
                .WithPrice(amount)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            (this.billToCustomer).AddPartyClassification(classification);

            this.invoice.ShipToCustomer = this.shipToCustomer;

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(amount, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount, item1.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithSurchargePercentageForPartyClassification_WhenDeriving_ThenUseSurchargeComponentsForPartyClassification()
        {
            const decimal quantity = 3;
            const decimal percentage = 5;

            var classification = new IndustryClassificationBuilder(this.Transaction).WithName("gold customer").Build();
            new SurchargeComponentBuilder(this.Transaction)
                .WithDescription("discount good for party classification")
                .WithPartyClassification(classification)
                .WithProduct(this.good)
                .WithPercentage(percentage)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            (this.billToCustomer).AddPartyClassification(classification);

            this.invoice.ShipToCustomer = this.shipToCustomer;

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            var price = this.currentGood1BasePrice.Price ?? 0;
            var amount = Math.Round(price * percentage / 100, 2);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(amount, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount, item1.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithDiscountAmountForProductCatergory_WhenDeriving_ThenUseDiscountComponentsForProductCatergory()
        {
            const decimal quantity = 3;
            const decimal amount = 1;

            var category = new ProductCategoryBuilder(this.Transaction)
                .WithName("gizmo")
                .Build();

            new DiscountComponentBuilder(this.Transaction)
                .WithDescription("discount good for product category")
                .WithProductCategory(category)
                .WithProduct(this.good)
                .WithPrice(amount)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            category.AddProduct(this.good);

            this.Transaction.Derive();

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(amount, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount, item1.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithDiscountPercentageForProductCatergory_WhenDeriving_ThenUseDiscountComponentsForProductCatergory()
        {
            const decimal quantity = 3;
            const decimal percentage = 5;

            var category = new ProductCategoryBuilder(this.Transaction)
                .WithName("gizmo")
                .Build();

            new DiscountComponentBuilder(this.Transaction)
                .WithDescription("discount good for product category")
                .WithProductCategory(category)
                .WithProduct(this.good)
                .WithPercentage(percentage)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            category.AddProduct(this.good);

            this.Transaction.Derive();

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            var price = this.currentGood1BasePrice.Price ?? 0;
            var amount = Math.Round(price * percentage / 100, 2);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(amount, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount, item1.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithSurchargeAmountForProductCatergory_WhenDeriving_ThenUseSurchargeComponentsForProductCatergory()
        {
            const decimal quantity = 3;
            const decimal amount = 1;

            var category = new ProductCategoryBuilder(this.Transaction)
                .WithName("gizmo")
                .Build();

            new SurchargeComponentBuilder(this.Transaction)
                .WithDescription("discount good for product category")
                .WithProductCategory(category)
                .WithProduct(this.good)
                .WithPrice(amount)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            category.AddProduct(this.good);

            this.Transaction.Derive();

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(amount, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount, item1.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithSurchargePercentageForProductCatergory_WhenDeriving_ThenUseSurchargeComponentsForProductCatergory()
        {
            const decimal quantity = 3;
            const decimal percentage = 5;

            var category = new ProductCategoryBuilder(this.Transaction)
                .WithName("gizmo")
                .Build();

            new SurchargeComponentBuilder(this.Transaction)
                .WithDescription("discount good for product category")
                .WithProductCategory(category)
                .WithProduct(this.good)
                .WithPercentage(percentage)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            category.AddProduct(this.good);

            this.Transaction.Derive();

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            var price = this.currentGood1BasePrice.Price ?? 0;
            var amount = Math.Round(price * percentage / 100, 2);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(amount, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount, item1.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithDiscountAmountForOrderQuantityBreak_WhenDeriving_ThenUseDiscountComponentsForOrderQuantityBreak()
        {
            const decimal quantity1 = 3;
            const decimal quantity2 = 50;
            const decimal quantity3 = 50;
            const decimal amount1 = 1;
            const decimal amount2 = 3;

            var break1 = new OrderQuantityBreakBuilder(this.Transaction).WithFromAmount(50).WithThroughAmount(99).Build();
            var break2 = new OrderQuantityBreakBuilder(this.Transaction).WithFromAmount(100).Build();

            new DiscountComponentBuilder(this.Transaction)
                .WithDescription("discount good for quantity break 1")
                .WithOrderQuantityBreak(break1)
                .WithProduct(this.good)
                .WithPrice(amount1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            new DiscountComponentBuilder(this.Transaction)
                .WithDescription("discount good for quantity break 2")
                .WithOrderQuantityBreak(break2)
                .WithProduct(this.good)
                .WithPrice(amount2)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity1).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitPrice);

            var item2 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity2).Build();
            this.invoice.AddSalesInvoiceItem(item2);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(amount1, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount1, item1.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item2.UnitBasePrice);
            Assert.Equal(amount1, item2.UnitDiscount);
            Assert.Equal(0, item2.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount1, item2.UnitPrice);

            var item3 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity3).Build();
            this.invoice.AddSalesInvoiceItem(item3);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(amount2, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount2, item1.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item2.UnitBasePrice);
            Assert.Equal(amount2, item2.UnitDiscount);
            Assert.Equal(0, item2.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount2, item2.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item3.UnitBasePrice);
            Assert.Equal(amount2, item3.UnitDiscount);
            Assert.Equal(0, item3.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount2, item3.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithDiscountPercentageForOrderQuantityBreak_WhenDeriving_ThenUseDiscountComponentsForOrderQuantityBreak()
        {
            const decimal quantity1 = 3;
            const decimal quantity2 = 50;
            const decimal quantity3 = 50;
            const decimal percentage1 = 5;
            const decimal percentage2 = 10;

            var break1 = new OrderQuantityBreakBuilder(this.Transaction).WithFromAmount(50).WithThroughAmount(99).Build();
            var break2 = new OrderQuantityBreakBuilder(this.Transaction).WithFromAmount(100).Build();

            new DiscountComponentBuilder(this.Transaction)
                .WithDescription("discount good for quantity break 1")
                .WithOrderQuantityBreak(break1)
                .WithProduct(this.good)
                .WithPercentage(percentage1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            new DiscountComponentBuilder(this.Transaction)
                .WithDescription("discount good for quantity break 2")
                .WithOrderQuantityBreak(break2)
                .WithProduct(this.good)
                .WithPercentage(percentage2)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity1).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitPrice);

            var item2 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity2).Build();
            this.invoice.AddSalesInvoiceItem(item2);

            this.Transaction.Derive();

            var price = this.currentGood1BasePrice.Price ?? 0;
            var amount1 = Math.Round(price * percentage1 / 100, 2);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(amount1, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount1, item1.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item2.UnitBasePrice);
            Assert.Equal(amount1, item2.UnitDiscount);
            Assert.Equal(0, item2.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount1, item2.UnitPrice);

            var item3 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity3).Build();
            this.invoice.AddSalesInvoiceItem(item3);

            this.Transaction.Derive();

            var price2 = this.currentGood1BasePrice.Price ?? 0;
            var amount2 = Math.Round(price2 * percentage2 / 100, 2);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(amount2, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount2, item1.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item2.UnitBasePrice);
            Assert.Equal(amount2, item2.UnitDiscount);
            Assert.Equal(0, item2.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount2, item2.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item3.UnitBasePrice);
            Assert.Equal(amount2, item3.UnitDiscount);
            Assert.Equal(0, item3.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount2, item3.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithSurchargeAmountForOrderQuantityBreak_WhenDeriving_ThenUseSurchargeComponentsForOrderQuantityBreak()
        {
            const decimal quantity1 = 3;
            const decimal quantity2 = 50;
            const decimal quantity3 = 50;
            const decimal amount1 = 1;
            const decimal amount2 = 3;

            var break1 = new OrderQuantityBreakBuilder(this.Transaction).WithFromAmount(50).WithThroughAmount(99).Build();
            var break2 = new OrderQuantityBreakBuilder(this.Transaction).WithFromAmount(100).Build();

            new SurchargeComponentBuilder(this.Transaction)
                .WithDescription("surcharge good for quantity break 1")
                .WithOrderQuantityBreak(break1)
                .WithProduct(this.good)
                .WithPrice(amount1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            new SurchargeComponentBuilder(this.Transaction)
                .WithDescription("surcharge good for quantity break 2")
                .WithOrderQuantityBreak(break2)
                .WithProduct(this.good)
                .WithPrice(amount2)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity1).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitPrice);

            var item2 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity2).Build();
            this.invoice.AddSalesInvoiceItem(item2);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(amount1, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount1, item1.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item2.UnitBasePrice);
            Assert.Equal(0, item2.UnitDiscount);
            Assert.Equal(amount1, item2.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount1, item2.UnitPrice);

            var item3 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity3).Build();
            this.invoice.AddSalesInvoiceItem(item3);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(amount2, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount2, item1.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item2.UnitBasePrice);
            Assert.Equal(0, item2.UnitDiscount);
            Assert.Equal(amount2, item2.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount2, item2.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item3.UnitBasePrice);
            Assert.Equal(0, item3.UnitDiscount);
            Assert.Equal(amount2, item3.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount2, item3.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithSurchargePercentageForOrderQuantityBreak_WhenDeriving_ThenUseSurchargeComponentsForOrderQuantityBreak()
        {
            const decimal quantity1 = 3;
            const decimal quantity2 = 50;
            const decimal quantity3 = 50;
            const decimal percentage1 = 5;
            const decimal percentage2 = 10;

            var break1 = new OrderQuantityBreakBuilder(this.Transaction).WithFromAmount(50).WithThroughAmount(99).Build();
            var break2 = new OrderQuantityBreakBuilder(this.Transaction).WithFromAmount(100).Build();

            new SurchargeComponentBuilder(this.Transaction)
                .WithDescription("surcharge good for quantity break 1")
                .WithOrderQuantityBreak(break1)
                .WithProduct(this.good)
                .WithPercentage(percentage1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            new SurchargeComponentBuilder(this.Transaction)
                .WithDescription("surcharge good for quantity break 2")
                .WithOrderQuantityBreak(break2)
                .WithProduct(this.good)
                .WithPercentage(percentage2)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity1).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitPrice);

            var item2 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity2).Build();
            this.invoice.AddSalesInvoiceItem(item2);

            this.Transaction.Derive();

            var price = this.currentGood1BasePrice.Price ?? 0;
            var amount1 = Math.Round(price * percentage1 / 100, 2);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(amount1, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount1, item1.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item2.UnitBasePrice);
            Assert.Equal(0, item2.UnitDiscount);
            Assert.Equal(amount1, item2.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount1, item2.UnitPrice);

            var item3 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity3).Build();
            this.invoice.AddSalesInvoiceItem(item3);

            this.Transaction.Derive();

            var price2 = this.currentGood1BasePrice.Price ?? 0;
            var amount2 = Math.Round(price2 * percentage2 / 100, 2);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(amount2, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount2, item1.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item2.UnitBasePrice);
            Assert.Equal(0, item2.UnitDiscount);
            Assert.Equal(amount2, item2.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount2, item2.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item3.UnitBasePrice);
            Assert.Equal(0, item3.UnitDiscount);
            Assert.Equal(amount2, item3.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount2, item3.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithDiscountAmountForOrderValue_WhenDeriving_ThenUseDiscountComponentsForOrderValue()
        {
            const decimal quantity1 = 3;
            const decimal quantity2 = 3;
            const decimal quantity3 = 10;
            const decimal amount1 = 1;
            const decimal amount2 = 3;

            var value1 = new OrderValueBuilder(this.Transaction).WithFromAmount(50).WithThroughAmount(99).Build();
            var value2 = new OrderValueBuilder(this.Transaction).WithFromAmount(100).Build();

            new DiscountComponentBuilder(this.Transaction)
                .WithDescription("discount good for order value 1")
                .WithOrderValue(value1)
                .WithProduct(this.good)
                .WithPrice(amount1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            new DiscountComponentBuilder(this.Transaction)
                .WithDescription("discount good for order value 1")
                .WithOrderValue(value2)
                .WithProduct(this.good)
                .WithPrice(amount2)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity1).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitPrice);

            var item2 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity2).Build();
            this.invoice.AddSalesInvoiceItem(item2);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(amount1, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount1, item1.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item2.UnitBasePrice);
            Assert.Equal(amount1, item2.UnitDiscount);
            Assert.Equal(0, item2.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount1, item2.UnitPrice);

            var item3 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity3).Build();
            this.invoice.AddSalesInvoiceItem(item3);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(amount2, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount2, item1.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item2.UnitBasePrice);
            Assert.Equal(amount2, item2.UnitDiscount);
            Assert.Equal(0, item2.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount2, item2.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item3.UnitBasePrice);
            Assert.Equal(amount2, item3.UnitDiscount);
            Assert.Equal(0, item3.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount2, item3.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithDiscountPercentageForOrderValue_WhenDeriving_ThenUseDiscountComponentsForOrderValue()
        {
            const decimal quantity1 = 3;
            const decimal quantity2 = 3;
            const decimal quantity3 = 10;
            const decimal percentage1 = 5;
            const decimal percentage2 = 10;

            var value1 = new OrderValueBuilder(this.Transaction).WithFromAmount(50).WithThroughAmount(99).Build();
            var value2 = new OrderValueBuilder(this.Transaction).WithFromAmount(100).Build();

            new DiscountComponentBuilder(this.Transaction)
                .WithDescription("discount good for order value 1")
                .WithOrderValue(value1)
                .WithProduct(this.good)
                .WithPercentage(percentage1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            new DiscountComponentBuilder(this.Transaction)
                .WithDescription("discount good for order value 1")
                .WithOrderValue(value2)
                .WithProduct(this.good)
                .WithPercentage(percentage2)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity1).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitPrice);

            var item2 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity2).Build();
            this.invoice.AddSalesInvoiceItem(item2);

            this.Transaction.Derive();

            var price = this.currentGood1BasePrice.Price ?? 0;
            var amount1 = Math.Round(price * percentage1 / 100, 2);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(amount1, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount1, item1.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item2.UnitBasePrice);
            Assert.Equal(amount1, item2.UnitDiscount);
            Assert.Equal(0, item2.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount1, item2.UnitPrice);

            var item3 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity3).Build();
            this.invoice.AddSalesInvoiceItem(item3);

            this.Transaction.Derive();

            var price2 = this.currentGood1BasePrice.Price ?? 0;
            var amount2 = Math.Round(price2 * percentage2 / 100, 2);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(amount2, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount2, item1.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item2.UnitBasePrice);
            Assert.Equal(amount2, item2.UnitDiscount);
            Assert.Equal(0, item2.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount2, item2.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item3.UnitBasePrice);
            Assert.Equal(amount2, item3.UnitDiscount);
            Assert.Equal(0, item3.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount2, item3.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithSurchargeAmountForOrderValue_WhenDeriving_ThenUseSurchargeComponentsForOrderValue()
        {
            const decimal quantity1 = 3;
            const decimal quantity2 = 3;
            const decimal quantity3 = 10;
            const decimal amount1 = 1;
            const decimal amount2 = 3;

            var value1 = new OrderValueBuilder(this.Transaction).WithFromAmount(50).WithThroughAmount(99).Build();
            var value2 = new OrderValueBuilder(this.Transaction).WithFromAmount(100).Build();

            new SurchargeComponentBuilder(this.Transaction)
                .WithDescription("surcharge good for order value 1")
                .WithOrderValue(value1)
                .WithProduct(this.good)
                .WithPrice(amount1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            new SurchargeComponentBuilder(this.Transaction)
                .WithDescription("surcharge good for order value 1")
                .WithOrderValue(value2)
                .WithProduct(this.good)
                .WithPrice(amount2)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity1).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitPrice);

            var item2 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity2).Build();
            this.invoice.AddSalesInvoiceItem(item2);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(amount1, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount1, item1.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item2.UnitBasePrice);
            Assert.Equal(0, item2.UnitDiscount);
            Assert.Equal(amount1, item2.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount1, item2.UnitPrice);

            var item3 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity3).Build();
            this.invoice.AddSalesInvoiceItem(item3);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(amount2, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount2, item1.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item2.UnitBasePrice);
            Assert.Equal(0, item2.UnitDiscount);
            Assert.Equal(amount2, item2.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount2, item2.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item3.UnitBasePrice);
            Assert.Equal(0, item3.UnitDiscount);
            Assert.Equal(amount2, item3.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount2, item3.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithSurchargePercentageForOrderValue_WhenDeriving_ThenUseSurchargeComponentsForOrderValue()
        {
            const decimal quantity1 = 3;
            const decimal quantity2 = 3;
            const decimal quantity3 = 10;
            const decimal percentage1 = 5;
            const decimal percentage2 = 10;

            var value1 = new OrderValueBuilder(this.Transaction).WithFromAmount(50).WithThroughAmount(99).Build();
            var value2 = new OrderValueBuilder(this.Transaction).WithFromAmount(100).Build();

            new SurchargeComponentBuilder(this.Transaction)
            .WithDescription("surcharge good for order value 1")
            .WithOrderValue(value1)
            .WithProduct(this.good)
            .WithPercentage(percentage1)
            .WithFromDate(this.Transaction.Now().AddMinutes(-1))
            .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
            .Build();

            new SurchargeComponentBuilder(this.Transaction)
            .WithDescription("surcharge good for order value 1")
            .WithOrderValue(value2)
            .WithProduct(this.good)
            .WithPercentage(percentage2)
            .WithFromDate(this.Transaction.Now().AddMinutes(-1))
            .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
            .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity1).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitPrice);

            var item2 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity2).Build();
            this.invoice.AddSalesInvoiceItem(item2);

            this.Transaction.Derive();

            var price = this.currentGood1BasePrice.Price ?? 0;
            var amount1 = Math.Round(price * percentage1 / 100, 2);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(amount1, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount1, item1.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item2.UnitBasePrice);
            Assert.Equal(0, item2.UnitDiscount);
            Assert.Equal(amount1, item2.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount1, item2.UnitPrice);

            var item3 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity3).Build();
            this.invoice.AddSalesInvoiceItem(item3);

            this.Transaction.Derive();

            var price2 = this.currentGood1BasePrice.Price ?? 0;
            var amount2 = Math.Round(price2 * percentage2 / 100, 2);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(amount2, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount2, item1.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item2.UnitBasePrice);
            Assert.Equal(0, item2.UnitDiscount);
            Assert.Equal(amount2, item2.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount2, item2.UnitPrice);

            Assert.Equal(this.currentGood1BasePrice.Price, item3.UnitBasePrice);
            Assert.Equal(0, item3.UnitDiscount);
            Assert.Equal(amount2, item3.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount2, item3.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithDiscountAmountForSalesType_WhenDeriving_ThenUseDiscountComponentsForSalesType()
        {
            const decimal quantity = 3;
            const decimal amount = 1;

            new DiscountComponentBuilder(this.Transaction)
                .WithDescription("discount good for sales type")
                .WithSalesChannel(new SalesChannels(this.Transaction).EmailChannel)
                .WithProduct(this.good)
                .WithPrice(amount)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            this.invoice.SalesChannel = new SalesChannels(this.Transaction).EmailChannel;

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(amount, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - amount, item1.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithDiscountPercentageAndItemDiscountPercentage_WhenDeriving_ThenExtraDiscountIsCalculated()
        {
            const decimal quantity = 3;
            const decimal percentage = 5;
            const decimal adjustmentPerc = 10;

            var discountAdjustment = new DiscountAdjustmentBuilder(this.Transaction).WithPercentage(adjustmentPerc).Build();

            new DiscountComponentBuilder(this.Transaction)
                .WithDescription("discount good for sales type")
                .WithSalesChannel(new SalesChannels(this.Transaction).EmailChannel)
                .WithProduct(this.good)
                .WithPercentage(percentage)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            this.invoice.SalesChannel = new SalesChannels(this.Transaction).EmailChannel;

            var item1 = new SalesInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithProduct(this.good)
                .WithQuantity(quantity)
                .WithDiscountAdjustment(discountAdjustment)
                .Build();

            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            var price = this.currentGood1BasePrice.Price ?? 0;
            var discount = Math.Round(price * percentage / 100, 2);
            var discountedprice = price - discount;
            var adjustmentPercentage = discountAdjustment.Percentage ?? 0;
            discount += Math.Round(discountedprice * adjustmentPercentage / 100, 2);

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(discount, item1.UnitDiscount);
            Assert.Equal(0, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price - discount, item1.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithSurchargeAmountForSalesType_WhenDeriving_ThenUseSurchargeComponentsForSalesType()
        {
            const decimal quantity = 3;
            const decimal amount = 1;

            new SurchargeComponentBuilder(this.Transaction)
                .WithDescription("discount good for sales type")
                .WithSalesChannel(new SalesChannels(this.Transaction).EmailChannel)
                .WithProduct(this.good)
                .WithPrice(amount)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            this.invoice.SalesChannel = new SalesChannels(this.Transaction).EmailChannel;

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(amount, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount, item1.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithSurchargePercentageForSalesType_WhenDeriving_ThenUseSurchargeComponentsForSalesType()
        {
            const decimal quantity = 3;
            const decimal percentage = 5;

            new SurchargeComponentBuilder(this.Transaction)
                .WithDescription("discount good for sales type")
                .WithSalesChannel(new SalesChannels(this.Transaction).EmailChannel)
                .WithProduct(this.good)
                .WithPercentage(percentage)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            this.invoice.SalesChannel = new SalesChannels(this.Transaction).EmailChannel;

            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity).Build();
            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            var price = this.currentGood1BasePrice.Price ?? 0;
            var amount = Math.Round(price * percentage / 100, 2);
            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(amount, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + amount, item1.UnitPrice);
        }

        [Fact]
        public void GivenInvoiceItemWithSurchargePercentageAndItemSurchargePercentage_WhenDeriving_ThenExtraSurchargeIsCalculated()
        {
            const decimal quantity = 3;
            const decimal percentage = 5;
            const decimal adjustmentPerc = 10;

            var surchargeAdjustment = new SurchargeAdjustmentBuilder(this.Transaction).WithPercentage(adjustmentPerc).Build();

            new SurchargeComponentBuilder(this.Transaction)
                .WithDescription("discount good for sales type")
                .WithSalesChannel(new SalesChannels(this.Transaction).EmailChannel)
                .WithProduct(this.good)
                .WithPercentage(percentage)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithThroughDate(this.Transaction.Now().AddYears(1).AddDays(-1))
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            this.invoice.SalesChannel = new SalesChannels(this.Transaction).EmailChannel;

            var item1 = new SalesInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithProduct(this.good)
                .WithQuantity(quantity)
                .WithSurchargeAdjustment(surchargeAdjustment)
                .Build();

            this.invoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            var price = this.currentGood1BasePrice.Price ?? 0;
            var surcharge = Math.Round(price * percentage / 100, 2);
            var surchargedprice = price + surcharge;
            var adjustmentPercentage = surchargeAdjustment.Percentage ?? 0;
            surcharge += Math.Round(surchargedprice * adjustmentPercentage / 100, 2);

            Assert.Equal(this.currentGood1BasePrice.Price, item1.UnitBasePrice);
            Assert.Equal(0, item1.UnitDiscount);
            Assert.Equal(surcharge, item1.UnitSurcharge);
            Assert.Equal(this.currentGood1BasePrice.Price + surcharge, item1.UnitPrice);
        }

        [Fact]
        public void GivenBillToCustomerWithDifferentCurrency_WhenDerivingPrices_ThenCalculatePricesInPreferredCurrency()
        {
            var euro = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "EUR");
            var poundSterling = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "GBP");

            new ExchangeRateBuilder(this.Transaction)
                .WithValidFrom(this.Transaction.Now())
                .WithFromCurrency(euro)
                .WithToCurrency(poundSterling)
                .WithRate(0.8553M)
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.InstantiateObjects(this.Transaction);

            Assert.Equal(euro, this.invoice.DerivedCurrency);

            this.billToCustomer.PreferredCurrency = poundSterling;

            var newInvoice = new SalesInvoiceBuilder(this.Transaction)
                .WithBillToCustomer(this.billToCustomer)
                .WithAssignedBillToContactMechanism(this.billToContactMechanismMechelen)
                .Build();

            this.Transaction.Derive();

            const decimal quantity = 3;
            var item1 = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(this.good).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem).WithQuantity(quantity).Build();
            newInvoice.AddSalesInvoiceItem(item1);

            this.Transaction.Derive();

            Assert.Equal(poundSterling, newInvoice.DerivedCurrency);

            Assert.Equal(0, item1.TotalDiscount);
            Assert.Equal(0, item1.TotalSurcharge);
        }

        [Fact]
        public void GiveninvoiceItem_WhenPartialPaymentIsReceived_ThenInvoiceItemStateIsSetToPartiallyPaid()
        {
            this.InstantiateObjects(this.Transaction);

            var item1 = new SalesInvoiceItemBuilder(this.Transaction)
                .WithProduct(this.good)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithQuantity(1)
                .WithAssignedUnitPrice(100M)
                .Build();

            this.invoice.AddSalesInvoiceItem(item1);
            this.Transaction.Derive();

            this.invoice.Send();
            this.Transaction.Derive();

            new ReceiptBuilder(this.Transaction)
                .WithAmount(50)
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(item1).WithAmountApplied(50).Build())
                .WithEffectiveDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new SalesInvoiceItemStates(this.Transaction).PartiallyPaid, item1.SalesInvoiceItemState);
        }

        [Fact]
        public void GiveninvoiceItem_WhenFullPaymentIsReceived_ThenInvoiceItemStateIsSetToPaid()
        {
            this.InstantiateObjects(this.Transaction);

            var item1 = new SalesInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithProduct(this.good)
                .WithQuantity(1)
                .WithAssignedUnitPrice(100M)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .Build();

            this.invoice.AddSalesInvoiceItem(item1);
            this.Transaction.Derive();

            this.invoice.Send();
            this.Transaction.Derive();

            new ReceiptBuilder(this.Transaction)
                .WithAmount(121)
                .WithPaymentApplication(new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(item1).WithAmountApplied(121).Build())
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new SalesInvoiceItemStates(this.Transaction).Paid, item1.SalesInvoiceItemState);
        }

        private void InstantiateObjects(ITransaction transaction)
        {
            this.good = (NonUnifiedGood)transaction.Instantiate(this.good);
            this.finishedGood = (Part)transaction.Instantiate(this.finishedGood);
            this.feature1 = (Colour)transaction.Instantiate(this.feature1);
            this.feature2 = (Colour)transaction.Instantiate(this.feature2);
            this.internalOrganisation = (Singleton)transaction.Instantiate(this.internalOrganisation);
            this.billToCustomer = (Organisation)transaction.Instantiate(this.billToCustomer);
            this.shipToCustomer = (Organisation)transaction.Instantiate(this.shipToCustomer);
            this.supplier = (Organisation)transaction.Instantiate(this.supplier);
            this.mechelen = (City)transaction.Instantiate(this.mechelen);
            this.kiev = (City)transaction.Instantiate(this.kiev);
            this.billToContactMechanismMechelen = (PostalAddress)transaction.Instantiate(this.billToContactMechanismMechelen);
            this.shipToContactMechanismKiev = (PostalAddress)transaction.Instantiate(this.shipToContactMechanismKiev);
            this.goodPurchasePrice = (SupplierOffering)transaction.Instantiate(this.goodPurchasePrice);
            this.currentBasePriceGeoBoundary = (BasePrice)transaction.Instantiate(this.currentBasePriceGeoBoundary);
            this.currentFeature1BasePrice = (BasePrice)transaction.Instantiate(this.currentFeature1BasePrice);
            this.currentGood1BasePrice = (BasePrice)transaction.Instantiate(this.currentGood1BasePrice);
            this.currentGood1Feature1BasePrice = (BasePrice)transaction.Instantiate(this.currentGood1Feature1BasePrice);
            this.invoice = (SalesInvoice)transaction.Instantiate(this.invoice);
            this.vatRegime = (VatRegime)transaction.Instantiate(this.vatRegime);
        }
    }

    public class SalesInvoiceItemOnBuildTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesInvoiceItemOnBuildTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DeriveSalesInvoiceItemState()
        {
            var invoiceitem = new SalesInvoiceItemBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            Assert.Equal(new SalesInvoiceItemStates(this.Transaction).ReadyForPosting, invoiceitem.SalesInvoiceItemState);
        }

        [Fact]
        public void DeriveInvoiceItemType()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).WithNonSerialisedDefaults(this.InternalOrganisation).Build();
            var invoiceitem = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(product).Build();

            this.Transaction.Derive(false);

            Assert.Equal(new InvoiceItemTypes(this.Transaction).ProductItem, invoiceitem.InvoiceItemType);
        }

        [Fact]
        public void DeriveDerivationTrigger()
        {
            var invoiceitem = new SalesInvoiceItemBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            Assert.True(invoiceitem.ExistDerivationTrigger);
        }
    }

    public class SalesInvoiceItemRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesInvoiceItemRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnChangedSalesInvoiceSalesInvoiceItemsDeriveVatRegime()
        {
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithAssignedVatRegime(new VatRegimes(this.Transaction).Exempt).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(invoiceItem.DerivedVatRegime, salesInvoice.DerivedVatRegime);
        }

        [Fact]
        public void ValidateAtmostOneProductAndPart()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();
            var part = new NonUnifiedPartBuilder(this.Transaction).Build();
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithPart(part).WithProduct(product).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("AssertExistsAtMostOne"));
        }

        [Fact]
        public void ValidateAtmostOneProductAndProductFeature()
        {
            var product = new NonUnifiedGoodBuilder(this.Transaction).Build();
            var colour = new ColourBuilder(this.Transaction).Build();
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithProductFeature(colour).WithProduct(product).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.StartsWith("AssertExistsAtMostOne"));
        }

        [Fact]
        public void ValidateAtmostOneSerialisedItemAndProductFeature()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var colour = new ColourBuilder(this.Transaction).Build();
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithProductFeature(colour).WithSerialisedItem(serialisedItem).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.StartsWith("AssertExistsAtMostOne")));
        }

        [Fact]
        public void ValidateAtmostOneSerialisedItemAndPart()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var part = new NonUnifiedPartBuilder(this.Transaction).Build();
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithPart(part).WithSerialisedItem(serialisedItem).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.StartsWith("AssertExistsAtMostOne")));
        }

        [Fact]
        public void ValidateNextSerialisedItemAvailability()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.StartsWith("AssertExists: ")));
        }

        [Fact]
        public void ValidateSerialisedPartQuantityValid()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithSerialisedDefaults(this.InternalOrganisation, this.Transaction.Faker()).Build();
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(1).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);

            var expectedMessage = $"{invoiceItem}, {this.M.SalesInvoiceItem.Quantity},{ ErrorMessages.InvalidQuantity}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.DoesNotContain(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ValidateSerialisedPartQuantityInvalid()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithSerialisedDefaults(this.InternalOrganisation, this.Transaction.Faker()).Build();
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(2).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);

            var expectedMessage = $"{invoiceItem}, {this.M.SalesInvoiceItem.Quantity},{ ErrorMessages.InvalidQuantity}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals(expectedMessage)));
        }

        [Fact]
        public void ValidateSerialisedPartQuantityInvalidAgain()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithSerialisedDefaults(this.InternalOrganisation, this.Transaction.Faker()).Build();
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(0).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);

            var expectedMessage = $"{invoiceItem}, {this.M.SalesInvoiceItem.Quantity},{ ErrorMessages.InvalidQuantity}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals(expectedMessage)));
        }

        [Fact]
        public void ValidateNonSerialisedPartQuantityInvalid()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithNonSerialisedDefaults(this.InternalOrganisation).Build();
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithPart(part).WithQuantity(0).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);

            var expectedMessage = $"{invoiceItem}, {this.M.SalesInvoiceItem.Quantity},{ ErrorMessages.InvalidQuantity}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals(expectedMessage)));
        }

        [Fact]
        public void ValidateServiceInvoiceItemQuantityInvalid()
        {
            var service = new InvoiceItemTypes(this.Transaction).Service;
            var part = new NonUnifiedPartBuilder(this.Transaction).WithNonSerialisedDefaults(this.InternalOrganisation).Build();
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithInvoiceItemType(service).WithQuantity(service.MaxQuantity + 1).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);

            var expectedMessage = $"{invoiceItem}, {this.M.SalesInvoiceItem.Quantity},{ ErrorMessages.InvalidQuantity}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Single(errors.FindAll(e => e.Message.Equals(expectedMessage)));
        }

        [Fact]
        public void ValidateServiceInvoiceItemQuantityvalid()
        {
            var service = new InvoiceItemTypes(this.Transaction).Service;
            var part = new NonUnifiedPartBuilder(this.Transaction).WithNonSerialisedDefaults(this.InternalOrganisation).Build();
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithInvoiceItemType(service).WithQuantity(service.MaxQuantity).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);

            var expectedMessage = $"{invoiceItem}, {this.M.SalesInvoiceItem.Quantity},{ ErrorMessages.InvalidQuantity}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.DoesNotContain(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedAssignedVatRegimeDeriveVatRegime()
        {
            var assignedVatRegime = new VatRegimes(this.Transaction).SpainReduced;
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithAssignedVatRegime(new VatRegimes(this.Transaction).Exempt).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoiceItem.AssignedVatRegime = assignedVatRegime;
            this.Transaction.Derive(false);

            Assert.Equal(assignedVatRegime, invoiceItem.DerivedVatRegime);
        }

        [Fact]
        public void ChangedSalesInvoiceVatRegimeDeriveVatRegime()
        {
            var vatRegime = new VatRegimes(this.Transaction).SpainReduced;
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            salesInvoice.AssignedVatRegime = vatRegime;
            this.Transaction.Derive(false);

            Assert.Equal(vatRegime, invoiceItem.DerivedVatRegime);
        }

        [Fact]
        public void ChangedSalesInvoiceVatRegimeDeriveVatRate()
        {
            var vatRegime = new VatRegimes(this.Transaction).SpainReduced;
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            salesInvoice.AssignedVatRegime = vatRegime;
            this.Transaction.Derive(false);

            Assert.Equal(vatRegime.VatRates[0], invoiceItem.VatRate);
        }

        [Fact]
        public void ChangedSalesInvoiceInvoiceDateDeriveVatRate()
        {
            var vatRegime = new VatRegimes(this.Transaction).SpainReduced;
            vatRegime.VatRates[0].ThroughDate = this.Transaction.Now().AddDays(-1).Date;
            this.Transaction.Derive(false);

            var newVatRate = new VatRateBuilder(this.Transaction).WithFromDate(this.Transaction.Now().Date).WithRate(11).Build();
            vatRegime.AddVatRate(newVatRate);
            this.Transaction.Derive(false);

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceDate(this.Transaction.Now().AddDays(-1).Date)
                .WithAssignedVatRegime(vatRegime).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.NotEqual(newVatRate, invoiceItem.VatRate);

            salesInvoice.InvoiceDate = this.Transaction.Now().AddDays(1).Date;
            this.Transaction.Derive(false);

            Assert.Equal(newVatRate, invoiceItem.VatRate);
        }

        [Fact]
        public void ChangedAssignedIrpfRegimeDeriveIrpfRegime()
        {
            var assignedIrpfRegime = new IrpfRegimes(this.Transaction).Assessable15;
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithAssignedIrpfRegime(new IrpfRegimes(this.Transaction).Exempt).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoiceItem.AssignedIrpfRegime = assignedIrpfRegime;
            this.Transaction.Derive(false);

            Assert.Equal(assignedIrpfRegime, invoiceItem.DerivedIrpfRegime);
        }

        [Fact]
        public void ChangedSalesInvoiceIrpfRegimeDeriveIrpfRegime()
        {
            var irpfRegime = new IrpfRegimes(this.Transaction).Assessable15;
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            salesInvoice.AssignedIrpfRegime = irpfRegime;
            this.Transaction.Derive(false);

            Assert.Equal(irpfRegime, invoiceItem.DerivedIrpfRegime);
        }

        [Fact]
        public void ChangedSalesInvoiceIrpfRegimeDeriveIrpfRate()
        {
            var irpfRegime = new IrpfRegimes(this.Transaction).Assessable15;
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            salesInvoice.AssignedIrpfRegime = irpfRegime;
            this.Transaction.Derive(false);

            Assert.Equal(irpfRegime.IrpfRates[0], invoiceItem.IrpfRate);
        }

        [Fact]
        public void ChangedPaymentApplicationAmountAppliedDeriveAmountPaid()
        {
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            new PaymentApplicationBuilder(this.Transaction).WithInvoiceItem(invoiceItem).WithAmountApplied(1).Build();
            this.Transaction.Derive(false);

            Assert.Equal(1, invoiceItem.AmountPaid);
        }

        [Fact]
        public void ChangedSalesInvoiceSalesInvoiceStateDeriveSalesInvoiceItemStateCancelled()
        {
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            salesInvoice.CancelInvoice();
            this.Transaction.Derive(false);

            Assert.Equal(new SalesInvoiceItemStates(this.Transaction).CancelledByInvoice, invoiceItem.SalesInvoiceItemState);
        }

        [Fact]
        public void ChangedSalesInvoiceSalesInvoiceStateDeriveSalesInvoiceItemStateWrittenOff()
        {
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            salesInvoice.WriteOff();
            this.Transaction.Derive(false);

            Assert.Equal(new SalesInvoiceItemStates(this.Transaction).WrittenOff, invoiceItem.SalesInvoiceItemState);
        }

        [Fact]
        public void ChangedSalesInvoiceSalesInvoiceStateDeriveSalesInvoiceItemStateReadyForPosting()
        {
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).Build();
            salesInvoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            salesInvoice.CancelInvoice();
            this.Transaction.Derive(false);

            salesInvoice.Reopen();
            this.Transaction.Derive(false);

            Assert.Equal(new SalesInvoiceItemStates(this.Transaction).ReadyForPosting, invoiceItem.SalesInvoiceItemState);
        }
    }

    public class SalesInvoiceItemSubTotalItemRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesInvoiceItemSubTotalItemRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedItemInvoiceItemTypeThrowvalidationError()
        {
            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithQuantity(0).Build();
            this.Transaction.Derive(false);

            invoiceItem.InvoiceItemType = new InvoiceItemTypes(this.Transaction).PartItem;

            var expectedMessage = $"{invoiceItem}, {this.M.SalesInvoiceItem.Quantity},{ ErrorMessages.InvalidQuantity}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedQuantityThrowvalidationError()
        {
            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem).WithQuantity(1).Build();
            this.Transaction.Derive(false);

            invoiceItem.Quantity = 0;

            var expectedMessage = $"{invoiceItem}, {this.M.SalesInvoiceItem.Quantity},{ ErrorMessages.InvalidQuantity}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }
    }

    [Trait("Category", "Security")]
    public class SalesInvoiceItemDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesInvoiceItemDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Transaction).Get(this.M.SalesInvoiceItem, this.M.SalesInvoiceItem.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedSalesInvoiceItemStateReadyForPostingDeriveDeletePermission()
        {
            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, invoiceItem.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesInvoiceItemStateCancelledDeriveDeletePermission()
        {
            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            invoiceItem.CancelFromInvoice();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, invoiceItem.DeniedPermissions);
        }
    }
}
