// <copyright file="PartTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class UnifiedGoodRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public UnifiedGoodRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProductIdentificationsDeriveSearchString()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            unifiedGood.AddProductIdentification(new SkuIdentificationBuilder(this.Transaction).WithIdentification("sku").WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Sku).Build());
            this.Derive();

            Assert.Contains("sku", unifiedGood.SearchString);
        }

        [Fact]
        public void ChangedProductCategoryAllProductsDeriveSearchString()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            new ProductCategoryBuilder(this.Transaction).WithName("catname").WithProduct(unifiedGood).Build();
            this.Derive();

            Assert.Contains("catname", unifiedGood.SearchString);
        }

        [Fact]
        public void ChangedSupplierOfferingsPartDeriveSearchString()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            new SupplierOfferingBuilder(this.Transaction)
                .WithSupplier(this.InternalOrganisation.ActiveSuppliers.First)
                .WithPart(unifiedGood).Build();
            this.Derive();

            Assert.Contains(this.InternalOrganisation.ActiveSuppliers.First.PartyName, unifiedGood.SearchString);
        }

        [Fact]
        public void ChangedSerialisedItemsDeriveSearchString()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            unifiedGood.AddSerialisedItem(new SerialisedItemBuilder(this.Transaction).WithSerialNumber("serialnumber").Build());
            this.Derive();

            Assert.Contains("serialnumber", unifiedGood.SearchString);
        }

        [Fact]
        public void ChangedProductTypeDeriveSearchString()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            unifiedGood.ProductType = new ProductTypeBuilder(this.Transaction).WithName("producttype").Build();
            this.Derive();

            Assert.Contains("producttype", unifiedGood.SearchString);
        }

        [Fact]
        public void ChangedBrandDeriveSearchString()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            unifiedGood.Brand = new BrandBuilder(this.Transaction).WithName("brand").Build();
            this.Derive();

            Assert.Contains("brand", unifiedGood.SearchString);
        }

        [Fact]
        public void ChangedModelDeriveSearchString()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            unifiedGood.Model = new ModelBuilder(this.Transaction).WithName("model").Build();
            this.Derive();

            Assert.Contains("model", unifiedGood.SearchString);
        }

        [Fact]
        public void ChangedKeywordsDeriveSearchString()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            unifiedGood.Keywords = "keywords";
            this.Derive();

            Assert.Contains("keywords", unifiedGood.SearchString);
        }

        [Fact]
        public void ChangedVariantsDeriveVirtualProductPriceComponents()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var pricecomponent = new BasePriceBuilder(this.Transaction)
                .WithProduct(unifiedGood)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();
            this.Derive();

            var variantGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            unifiedGood.AddVariant(variantGood);
            this.Derive();

            Assert.Equal(variantGood.VirtualProductPriceComponents.First, pricecomponent);
        }

        [Fact]
        public void ChangedVariantsDeriveVirtualProductPriceComponents_2()
        {
            var variantGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).WithVariant(variantGood).Build();
            this.Derive();

            var pricecomponent = new BasePriceBuilder(this.Transaction)
                .WithProduct(unifiedGood)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();
            this.Derive();

            unifiedGood.RemoveVariant(variantGood);
            this.Derive();

            Assert.Empty(variantGood.VirtualProductPriceComponents);
        }

        [Fact]
        public void ChangedVariantsDeriveBasePrice()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var pricecomponent = new BasePriceBuilder(this.Transaction)
                .WithProduct(unifiedGood)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();
            this.Derive();

            var variantGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            unifiedGood.AddVariant(variantGood);
            this.Derive();

            Assert.Equal(variantGood.BasePrices.First, pricecomponent);
        }

        [Fact]
        public void ChangedPriceComponentProductDeriveVirtualProductPriceComponents()
        {
            var variantGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).WithVariant(variantGood).Build();
            this.Derive();

            var pricecomponent = new BasePriceBuilder(this.Transaction)
                .WithProduct(unifiedGood)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();
            this.Derive();

            Assert.Equal(variantGood.VirtualProductPriceComponents.First, pricecomponent);
        }
    }

    [Trait("Category", "Security")]
    public class UnifiedGoodDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public UnifiedGoodDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Transaction).Get(this.M.UnifiedGood, this.M.UnifiedGood.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedNonUnifiedGoodDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedDeploymentProductOfferingDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var deployment = new DeploymentBuilder(this.Transaction).Build();
            this.Derive();

            deployment.ProductOffering = unifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedEngagementItemProductDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var engagementItem = new GoodOrderItemBuilder(this.Transaction).Build();
            this.Derive();

            engagementItem.Product = unifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedGeneralLedgerAccountDerivedCostUnitsAllowedDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var generalLedgerAccount = new GeneralLedgerAccountBuilder(this.Transaction).Build();
            this.Derive();

            generalLedgerAccount.AddAssignedCostUnitsAllowed(unifiedGood);
            this.Derive();

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedGeneralLedgerAccountDefaultCostUnitDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var generalLedgerAccount = new GeneralLedgerAccountBuilder(this.Transaction).Build();
            this.Derive();

            generalLedgerAccount.DefaultCostUnit = unifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedQuoteItemProductDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var quoteItem = new QuoteItemBuilder(this.Transaction).Build();
            this.Derive();

            quoteItem.Product = unifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedShipmentItemGoodDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            this.Derive();

            shipmentItem.Good = unifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortGoodStandardUnifiedProductDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var workEffortGoodStandard = new WorkEffortGoodStandardBuilder(this.Transaction).Build();
            this.Derive();

            workEffortGoodStandard.UnifiedProduct = unifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedMarketingPackageProductUsedInDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var marketingPackage = new MarketingPackageBuilder(this.Transaction).Build();
            this.Derive();

            marketingPackage.AddProductsUsedIn(unifiedGood);
            this.Derive();

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedMarketingPackageProductDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var marketingPackage = new MarketingPackageBuilder(this.Transaction).Build();
            this.Derive();

            marketingPackage.Product = unifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedOrganisationGlAccountProductDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var organisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction).Build();
            this.Derive();

            organisationGlAccount.Product = unifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProductConfigurationProductUsedInDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var productConfiguration = new ProductConfigurationBuilder(this.Transaction).Build();
            this.Derive();

            productConfiguration.AddProductsUsedIn(unifiedGood);
            this.Derive();

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProductConfigurationProductDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var productConfiguration = new ProductConfigurationBuilder(this.Transaction).Build();
            this.Derive();

            productConfiguration.Product = unifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestItemProductDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var requestItem = new RequestItemBuilder(this.Transaction).Build();
            this.Derive();

            requestItem.Product = unifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesInvoiceItemProductDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var salesInvoiceItem = new SalesInvoiceItemBuilder(this.Transaction).Build();
            this.Derive();

            salesInvoiceItem.Product = unifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesOrderItemProductDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var salesOrderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            this.Derive();

            salesOrderItem.Product = unifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortTypeProductToProduceDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).Build();          
            this.Derive();

            workEffortType.ProductToProduce = unifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortInventoryProducedPartDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var workEffortInventoryProduced = new WorkEffortInventoryProducedBuilder(this.Transaction).Build();
            this.Derive();

            workEffortInventoryProduced.Part = unifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortPartStandardPartDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var workEffortPartStandard = new WorkEffortPartStandardBuilder(this.Transaction).Build();
            this.Derive();

            workEffortPartStandard.Part = unifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPartBillOfMaterialPartDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var partBillOfMaterial = new EngineeringBomBuilder(this.Transaction).Build();
            this.Derive();

            partBillOfMaterial.Part = unifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPartBillOfMaterialComponentPartDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var partBillOfMaterial = new EngineeringBomBuilder(this.Transaction).Build();
            this.Derive();

            partBillOfMaterial.ComponentPart = unifiedGood;
            this.Derive();

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedInventoryItemTransactionPartDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction).WithReason(new InventoryTransactionReasonBuilder(this.Transaction).Build()).WithPart(unifiedGood).Build();
            this.Derive();

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSerialisedItemDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            unifiedGood.AddSerialisedItem(serialisedItem);
            this.Derive();

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }
    }
}
