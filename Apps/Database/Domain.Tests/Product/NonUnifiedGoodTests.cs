
// <copyright file="OrderTermTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
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

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("NonUnifiedGood.Part is required"));
        }
    }

    public class NonUnifiedGoodDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public NonUnifiedGoodDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProductIdentificationsDeriveSearchString()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Contains(nonUnifiedGood.ProductIdentifications.First.Identification, nonUnifiedGood.SearchString);
        }

        [Fact]
        public void ChangedProductCategoryAllProductsDeriveSearchString()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new ProductCategoryBuilder(this.Transaction).WithName("catname").WithProduct(nonUnifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains("catname", nonUnifiedGood.SearchString);
        }

        [Fact]
        public void ChangedKeywordsDeriveSearchString()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            nonUnifiedGood.Keywords = "keywords";
            this.Transaction.Derive(false);

            Assert.Contains("keywords", nonUnifiedGood.SearchString);
        }

        [Fact]
        public void ChangedVariantsDeriveVirtualProductPriceComponents()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Transaction)
                .WithProduct(nonUnifiedGood)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();
            this.Transaction.Derive(false);

            var variantGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            nonUnifiedGood.AddVariant(variantGood);
            this.Transaction.Derive(false);

            Assert.Equal(variantGood.VirtualProductPriceComponents.First, pricecomponent);
        }

        [Fact]
        public void ChangedVariantsDeriveVirtualProductPriceComponents_2()
        {
            var variantGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).WithVariant(variantGood).Build();
            this.Transaction.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Transaction)
                .WithProduct(nonUnifiedGood)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();
            this.Transaction.Derive(false);

            nonUnifiedGood.RemoveVariant(variantGood);
            this.Transaction.Derive(false);

            Assert.Empty(variantGood.VirtualProductPriceComponents);
        }

        [Fact]
        public void ChangedVariantsDeriveBasePrice()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Transaction)
                .WithProduct(nonUnifiedGood)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();
            this.Transaction.Derive(false);

            var variantGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            nonUnifiedGood.AddVariant(variantGood);
            this.Transaction.Derive(false);

            Assert.Equal(variantGood.BasePrices.First, pricecomponent);
        }

        [Fact]
        public void ChangedPriceComponentProductDeriveVirtualProductPriceComponents()
        {
            var variantGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).WithVariant(variantGood).Build();
            this.Transaction.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Transaction)
                .WithProduct(nonUnifiedGood)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(variantGood.VirtualProductPriceComponents.First, pricecomponent);
        }
    }

    [Trait("Category", "Security")]
    public class NonUnifiedGoodDeniedPermissionDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public NonUnifiedGoodDeniedPermissionDerivationTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Transaction).Get(this.M.NonUnifiedGood, this.M.NonUnifiedGood.Delete);
        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedNonUnifiedGoodDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodPartDeriveDeletePermissionDenied()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            nonUnifiedGood.Part = nonUnifiedPart;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodPartDeriveDeletePermissionAllowed()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).WithPart(nonUnifiedPart).Build();
            this.Transaction.Derive(false);

            nonUnifiedGood.RemovePart();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedDeploymentProductOfferingDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var deployment = new DeploymentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            deployment.ProductOffering = nonUnifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedEngagementItemProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var goodOrder = new GoodOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            goodOrder.Product = nonUnifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedGeneralLedgerAccountCostUnitsAllowedDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var generalLedgerAccounts = new GeneralLedgerAccountBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            generalLedgerAccounts.AddAssignedCostUnitsAllowed(nonUnifiedGood);
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedGeneralLedgerAccountDefaultCostUnitDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var generalLedgerAccounts = new GeneralLedgerAccountBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            generalLedgerAccounts.DefaultCostUnit = nonUnifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedQuoteItemProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            quoteItem.Product = nonUnifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedShipmentItemGoodDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            shipmentItem.Good = nonUnifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortGoodStandardUnifiedProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workEffortStandard = new WorkEffortGoodStandardBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            workEffortStandard.UnifiedProduct = nonUnifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedMarketingPackageProductsUsedInDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var marketingPackage = new MarketingPackageBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            marketingPackage.AddProductsUsedIn(nonUnifiedGood);
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedMarketingPackageProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var marketingPackage = new MarketingPackageBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            marketingPackage.Product = nonUnifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedOrganisationGlAccountProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var account = new OrganisationGlAccountBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            account.Product = nonUnifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProductConfigurationProductsUsedInDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var configuration = new ProductConfigurationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            configuration.AddProductsUsedIn(nonUnifiedGood);
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProductConfigurationProductInDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var configuration = new ProductConfigurationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            configuration.Product = nonUnifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestItemProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var requestItem = new RequestItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            requestItem.Product = nonUnifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesInvoiceItemProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            invoiceItem.Product = nonUnifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedSalesOrderItemProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            orderItem.Product = nonUnifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortTypeProductToProduceDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            workEffortType.ProductToProduce = nonUnifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }
    }
}
