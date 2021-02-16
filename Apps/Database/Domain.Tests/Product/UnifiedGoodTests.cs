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

    public class UnifiedGoodDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public UnifiedGoodDerivationTests(Fixture fixture) : base(fixture) { }

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
    public class UnifiedGoodDeniedPermissionDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public UnifiedGoodDeniedPermissionDerivationTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Transaction).Get(this.M.UnifiedGood.ObjectType, this.M.UnifiedGood.Delete);

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
        public void OnChangedNonUnifiedGoodWithDeploymentDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var deployments = new DeploymentBuilder(this.Transaction).WithProductOffering(unifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithEngagementItemDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var engagementItems = new GoodOrderItemBuilder(this.Transaction).WithProduct(unifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithGeneralLedgerAccountDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var generalLedgerAccounts = new GeneralLedgerAccountBuilder(this.Transaction).WithAssignedCostUnitsAllowed(unifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithQuoteItemDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var quote = new ProposalBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var quoteItems = new QuoteItemBuilder(this.Transaction).WithProduct(unifiedGood).Build();
            quote.AddQuoteItem(quoteItems);
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithShipmentItemDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var shipment = new TransferBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var shipmentItems = new ShipmentItemBuilder(this.Transaction).WithGood(unifiedGood).Build();
            shipment.AddShipmentItem(shipmentItems);
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithWorkEffortGoodStandardDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workEffortGoodStandards = new WorkEffortGoodStandardBuilder(this.Transaction).WithUnifiedProduct(unifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithMarketingPackageProductUsedInDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var marketingPackage = new MarketingPackageBuilder(this.Transaction).WithProductsUsedIn(unifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithMarketingPackageProductDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var marketingPackage = new MarketingPackageBuilder(this.Transaction).WithProduct(unifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithOrganisationGlAccountDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var organisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction).WithProduct(unifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithProductConfigurationProductUsedInDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var productConfiguration = new ProductConfigurationBuilder(this.Transaction).WithProductsUsedIn(unifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithProductConfigurationProductDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var productConfiguration = new ProductConfigurationBuilder(this.Transaction).WithProduct(unifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithrequestItemDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var request = new RequestForQuoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var requestItem = new RequestItemBuilder(this.Transaction).WithProduct(unifiedGood).Build();
            request.AddRequestItem(requestItem);
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithSalesInvoiceItemDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var salesInvoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(unifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithSalesOrderItemDeriveDeletePermission()
        {
            var salesOrder = this.InternalOrganisation.CreateB2BSalesOrder(this.Transaction.Faker());
            this.Transaction.Derive(false);

            var item = salesOrder.SalesOrderItems.Where(v => v.Product.GetType().Name == typeof(UnifiedGood).Name).Select(v => v.Product).First();

            Assert.Contains(this.deletePermission, item.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithWorkEffortTypeDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).WithProductToProduce(unifiedGood).Build();          
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithWorkEffortInventoryProducedDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workEffortInventoryProduced = new WorkEffortInventoryProducedBuilder(this.Transaction).WithPart(unifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithWorkEffortPartStandardDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workEffortPartStandard = new WorkEffortPartStandardBuilder(this.Transaction).WithPart(unifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithPartBillOfMaterialPartDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var partBillOfMaterial = new EngineeringBomBuilder(this.Transaction).WithPart(unifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithPartBillOfMaterialComponentPartDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var partBillOfMaterial = new EngineeringBomBuilder(this.Transaction).WithComponentPart(unifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithInventoryItemTransactionDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction).WithReason(new InventoryTransactionReasonBuilder(this.Transaction).Build()).WithPart(unifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithSerialisedItemDeriveDeletePermission()
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
