
// <copyright file="OrderTermTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Xunit;

    public class NonUnifiedGoodTests : DomainTest, IClassFixture<Fixture>
    {
        public NonUnifiedGoodTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnPostDeriveAssertExistPart()
        {
            this.Transaction.GetSingleton().Settings.UseProductNumberCounter = true;

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Equals("NonUnifiedGood.Part is required"));
        }
    }

    public class NonUnifiedGoodRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public NonUnifiedGoodRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProductIdentificationsDeriveSearchString()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Contains(nonUnifiedGood.ProductIdentifications.First().Identification, nonUnifiedGood.SearchString);
        }

        [Fact]
        public void ChangedProductCategoryAllProductsDeriveSearchString()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            new ProductCategoryBuilder(this.Transaction).WithName("catname").WithProduct(nonUnifiedGood).Build();
            this.Derive();

            Assert.Contains("catname", nonUnifiedGood.SearchString);
        }

        [Fact]
        public void ChangedKeywordsDeriveSearchString()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            nonUnifiedGood.Keywords = "keywords";
            this.Derive();

            Assert.Contains("keywords", nonUnifiedGood.SearchString);
        }

        [Fact]
        public void ChangedVariantsDeriveVirtualProductPriceComponents()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var pricecomponent = new BasePriceBuilder(this.Transaction)
                .WithProduct(nonUnifiedGood)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();
            this.Derive();

            var variantGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            nonUnifiedGood.AddVariant(variantGood);
            this.Derive();

            Assert.Equal(variantGood.VirtualProductPriceComponents.FirstOrDefault(), pricecomponent);
        }

        [Fact]
        public void ChangedVariantsDeriveVirtualProductPriceComponents_2()
        {
            var variantGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).WithVariant(variantGood).Build();
            this.Derive();

            var pricecomponent = new BasePriceBuilder(this.Transaction)
                .WithProduct(nonUnifiedGood)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();
            this.Derive();

            nonUnifiedGood.RemoveVariant(variantGood);
            this.Derive();

            Assert.Empty(variantGood.VirtualProductPriceComponents);
        }

        [Fact]
        public void ChangedVariantsDeriveBasePrice()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var pricecomponent = new BasePriceBuilder(this.Transaction)
                .WithProduct(nonUnifiedGood)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();
            this.Derive();

            var variantGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            nonUnifiedGood.AddVariant(variantGood);
            this.Derive();

            Assert.Equal(variantGood.BasePrices.FirstOrDefault(), pricecomponent);
        }

        [Fact]
        public void ChangedPriceComponentProductDeriveVirtualProductPriceComponents()
        {
            var variantGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).WithVariant(variantGood).Build();
            this.Derive();

            var pricecomponent = new BasePriceBuilder(this.Transaction)
                .WithProduct(nonUnifiedGood)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();
            this.Derive();

            Assert.Equal(variantGood.VirtualProductPriceComponents.FirstOrDefault(), pricecomponent);
        }
    }

    [Trait("Category", "Security")]
    public class NonUnifiedGoodDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public NonUnifiedGoodDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Transaction).Get(this.M.NonUnifiedGood, this.M.NonUnifiedGood.Delete);
        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedNonUnifiedGoodDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();

            this.Derive();

            Assert.DoesNotContain(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodPartDeriveDeletePermissionDenied()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            nonUnifiedGood.Part = nonUnifiedPart;
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodPartDeriveDeletePermissionAllowed()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).WithPart(nonUnifiedPart).Build();
            this.Derive();

            nonUnifiedGood.RemovePart();
            this.Derive();

            Assert.DoesNotContain(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedDeploymentProductOfferingDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var deployment = new DeploymentBuilder(this.Transaction).Build();
            this.Derive();

            deployment.ProductOffering = nonUnifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedEngagementItemProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var goodOrder = new GoodOrderItemBuilder(this.Transaction).Build();
            this.Derive();

            goodOrder.Product = nonUnifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedGeneralLedgerAccountCostUnitsAllowedDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var generalLedgerAccounts = new GeneralLedgerAccountBuilder(this.Transaction).Build();
            this.Derive();

            generalLedgerAccounts.AddAssignedCostUnitsAllowed(nonUnifiedGood);
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedGeneralLedgerAccountDefaultCostUnitDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var generalLedgerAccounts = new GeneralLedgerAccountBuilder(this.Transaction).Build();
            this.Derive();

            generalLedgerAccounts.DefaultCostUnit = nonUnifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedQuoteItemProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).Build();
            this.Derive();

            quoteItem.Product = nonUnifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedShipmentItemGoodDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            this.Derive();

            shipmentItem.Good = nonUnifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortGoodStandardUnifiedProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var workEffortStandard = new WorkEffortGoodStandardBuilder(this.Transaction).Build();
            this.Derive();

            workEffortStandard.UnifiedProduct = nonUnifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedMarketingPackageProductsUsedInDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var marketingPackage = new MarketingPackageBuilder(this.Transaction).Build();
            this.Derive();

            marketingPackage.AddProductsUsedIn(nonUnifiedGood);
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedMarketingPackageProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var marketingPackage = new MarketingPackageBuilder(this.Transaction).Build();
            this.Derive();

            marketingPackage.Product = nonUnifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedOrganisationGlAccountProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var account = new OrganisationGlAccountBuilder(this.Transaction).Build();
            this.Derive();

            account.Product = nonUnifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProductConfigurationProductsUsedInDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var configuration = new ProductConfigurationBuilder(this.Transaction).Build();
            this.Derive();

            configuration.AddProductsUsedIn(nonUnifiedGood);
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProductConfigurationProductInDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var configuration = new ProductConfigurationBuilder(this.Transaction).Build();
            this.Derive();

            configuration.Product = nonUnifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestItemProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var requestItem = new RequestItemBuilder(this.Transaction).Build();
            this.Derive();

            requestItem.Product = nonUnifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesInvoiceItemProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).Build();
            this.Derive();

            invoiceItem.Product = nonUnifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedSalesOrderItemProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            this.Derive();

            orderItem.Product = nonUnifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortTypeProductToProduceDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).Build();
            this.Derive();

            workEffortType.ProductToProduce = nonUnifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }
    }
}
