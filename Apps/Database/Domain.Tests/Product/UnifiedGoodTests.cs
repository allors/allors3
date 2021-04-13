// <copyright file="PartTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using TestPopulation;
    using Xunit;

    public class UnifiedGoodRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public UnifiedGoodRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProductIdentificationsDeriveSearchString()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            unifiedGood.AddProductIdentification(new SkuIdentificationBuilder(this.Transaction).WithIdentification("sku").WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Sku).Build());
            this.Transaction.Derive(false);

            Assert.Contains("sku", unifiedGood.SearchString);
        }

        [Fact]
        public void ChangedProductCategoryAllProductsDeriveSearchString()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new ProductCategoryBuilder(this.Transaction).WithName("catname").WithProduct(unifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains("catname", unifiedGood.SearchString);
        }

        [Fact]
        public void ChangedSupplierOfferingsPartDeriveSearchString()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new SupplierOfferingBuilder(this.Transaction)
                .WithSupplier(this.InternalOrganisation.ActiveSuppliers.First)
                .WithPart(unifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.InternalOrganisation.ActiveSuppliers.First.PartyName, unifiedGood.SearchString);
        }

        [Fact]
        public void ChangedSerialisedItemsDeriveSearchString()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            unifiedGood.AddSerialisedItem(new SerialisedItemBuilder(this.Transaction).WithSerialNumber("serialnumber").Build());
            this.Transaction.Derive(false);

            Assert.Contains("serialnumber", unifiedGood.SearchString);
        }

        [Fact]
        public void ChangedProductTypeDeriveSearchString()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            unifiedGood.ProductType = new ProductTypeBuilder(this.Transaction).WithName("producttype").Build();
            this.Transaction.Derive(false);

            Assert.Contains("producttype", unifiedGood.SearchString);
        }

        [Fact]
        public void ChangedBrandDeriveSearchString()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            unifiedGood.Brand = new BrandBuilder(this.Transaction).WithName("brand").Build();
            this.Transaction.Derive(false);

            Assert.Contains("brand", unifiedGood.SearchString);
        }

        [Fact]
        public void ChangedModelDeriveSearchString()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            unifiedGood.Model = new ModelBuilder(this.Transaction).WithName("model").Build();
            this.Transaction.Derive(false);

            Assert.Contains("model", unifiedGood.SearchString);
        }

        [Fact]
        public void ChangedKeywordsDeriveSearchString()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            unifiedGood.Keywords = "keywords";
            this.Transaction.Derive(false);

            Assert.Contains("keywords", unifiedGood.SearchString);
        }

        [Fact]
        public void ChangedVariantsDeriveVirtualProductPriceComponents()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Transaction)
                .WithProduct(unifiedGood)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();
            this.Transaction.Derive(false);

            var variantGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            unifiedGood.AddVariant(variantGood);
            this.Transaction.Derive(false);

            Assert.Equal(variantGood.VirtualProductPriceComponents.First, pricecomponent);
        }

        [Fact]
        public void ChangedVariantsDeriveVirtualProductPriceComponents_2()
        {
            var variantGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).WithVariant(variantGood).Build();
            this.Transaction.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Transaction)
                .WithProduct(unifiedGood)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();
            this.Transaction.Derive(false);

            unifiedGood.RemoveVariant(variantGood);
            this.Transaction.Derive(false);

            Assert.Empty(variantGood.VirtualProductPriceComponents);
        }

        [Fact]
        public void ChangedVariantsDeriveBasePrice()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Transaction)
                .WithProduct(unifiedGood)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();
            this.Transaction.Derive(false);

            var variantGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            unifiedGood.AddVariant(variantGood);
            this.Transaction.Derive(false);

            Assert.Equal(variantGood.BasePrices.First, pricecomponent);
        }

        [Fact]
        public void ChangedPriceComponentProductDeriveVirtualProductPriceComponents()
        {
            var variantGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).WithVariant(variantGood).Build();
            this.Transaction.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Transaction)
                .WithProduct(unifiedGood)
                .WithPrice(1)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .Build();
            this.Transaction.Derive(false);

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
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedDeploymentProductOfferingDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var deployment = new DeploymentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            deployment.ProductOffering = unifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedEngagementItemProductDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var engagementItem = new GoodOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            engagementItem.Product = unifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedGeneralLedgerAccountDerivedCostUnitsAllowedDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var generalLedgerAccount = new GeneralLedgerAccountBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            generalLedgerAccount.AddAssignedCostUnitsAllowed(unifiedGood);
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedGeneralLedgerAccountDefaultCostUnitDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var generalLedgerAccount = new GeneralLedgerAccountBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            generalLedgerAccount.DefaultCostUnit = unifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedQuoteItemProductDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            quoteItem.Product = unifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedShipmentItemGoodDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            shipmentItem.Good = unifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortGoodStandardUnifiedProductDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workEffortGoodStandard = new WorkEffortGoodStandardBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            workEffortGoodStandard.UnifiedProduct = unifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedMarketingPackageProductUsedInDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var marketingPackage = new MarketingPackageBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            marketingPackage.AddProductsUsedIn(unifiedGood);
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedMarketingPackageProductDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var marketingPackage = new MarketingPackageBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            marketingPackage.Product = unifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedOrganisationGlAccountProductDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var organisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            organisationGlAccount.Product = unifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProductConfigurationProductUsedInDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var productConfiguration = new ProductConfigurationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            productConfiguration.AddProductsUsedIn(unifiedGood);
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedProductConfigurationProductDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var productConfiguration = new ProductConfigurationBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            productConfiguration.Product = unifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedRequestItemProductDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var requestItem = new RequestItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            requestItem.Product = unifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesInvoiceItemProductDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesInvoiceItem = new SalesInvoiceItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesInvoiceItem.Product = unifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesOrderItemProductDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesOrderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            salesOrderItem.Product = unifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortTypeProductToProduceDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).Build();          
            this.Transaction.Derive(false);

            workEffortType.ProductToProduce = unifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortInventoryProducedPartDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workEffortInventoryProduced = new WorkEffortInventoryProducedBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            workEffortInventoryProduced.Part = unifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortPartStandardPartDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workEffortPartStandard = new WorkEffortPartStandardBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            workEffortPartStandard.Part = unifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPartBillOfMaterialPartDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var partBillOfMaterial = new EngineeringBomBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            partBillOfMaterial.Part = unifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPartBillOfMaterialComponentPartDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var partBillOfMaterial = new EngineeringBomBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            partBillOfMaterial.ComponentPart = unifiedGood;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedInventoryItemTransactionPartDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction).WithReason(new InventoryTransactionReasonBuilder(this.Transaction).Build()).WithPart(unifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSerialisedItemDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            unifiedGood.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }
    }
}
