
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
    using TestPopulation;
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
        public NonUnifiedGoodDeniedPermissionDerivationTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Transaction).Get(this.M.NonUnifiedGood.ObjectType, this.M.NonUnifiedGood.Delete);
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
        public void OnChangedNonUnifiedGoodWithPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).WithPart(nonUnifiedPart).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithDeploymentsWhereProductOfferingDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new DeploymentBuilder(this.Transaction).WithProductOffering(nonUnifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithEngagementItemsWhereProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new GoodOrderItemBuilder(this.Transaction).WithProduct(nonUnifiedGood).Build();
            this.Transaction.Derive(false);
            
            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithGeneralLedgerAccountsWhereCostUnitsAllowedDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var generalLedgerAccounts = new GeneralLedgerAccountBuilder(this.Transaction).Build();
            generalLedgerAccounts.AddAssignedCostUnitsAllowed(nonUnifiedGood);
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithGeneralLedgerAccountsWhereDefaultCostUnitDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new GeneralLedgerAccountBuilder(this.Transaction).WithDefaultCostUnit(nonUnifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithQuoteItemsWhereProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction)
                .WithProduct(nonUnifiedGood)
                .WithInvoiceItemType(new InvoiceItemTypeBuilder(this.Transaction).Build())
                .WithUnitBasePrice(1)
                .WithAssignedUnitPrice(1)
                .Build();
            new ProductQuoteBuilder(this.Transaction).WithQuoteItem(quoteItem).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithShipmentItemsWhereGoodDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).WithGood(nonUnifiedGood).Build();
            new PurchaseShipmentBuilder(this.Transaction).WithShipmentItem(shipmentItem).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithWorkEffortGoodStandardsWhereUnifiedProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new WorkEffortGoodStandardBuilder(this.Transaction).WithUnifiedProduct(nonUnifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithMarketingPackageWhereProductsUsedInDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new MarketingPackageBuilder(this.Transaction).WithProductsUsedIn(nonUnifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedNonUnifiedGoodWithMarketingPackagesWhereProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new MarketingPackageBuilder(this.Transaction).WithProduct(nonUnifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithOrganisationGlAccountsWhereProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new OrganisationGlAccountBuilder(this.Transaction).WithProduct(nonUnifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithProductConfigurationsWhereProductsUsedInDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new ProductConfigurationBuilder(this.Transaction).WithProductsUsedIn(nonUnifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithProductConfigurationsWhereProductInDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new ProductConfigurationBuilder(this.Transaction).WithProduct(nonUnifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithRequestItemsWhereProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var requestItem = new RequestItemBuilder(this.Transaction).WithProduct(nonUnifiedGood).Build();
            var request = new RequestForQuoteBuilder(this.Transaction).Build();
            request.AddRequestItem(requestItem);
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithSalesInvoiceItemsWhereProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new SalesInvoiceItemBuilder(this.Transaction).WithProduct(nonUnifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedNonUnifiedGoodWithSalesOrderItemsWhereProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new SalesOrderItemBuilder(this.Transaction).WithProduct(nonUnifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithWorkEffortTypesWhereProductToProduceDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new WorkEffortTypeBuilder(this.Transaction).WithProductToProduce(nonUnifiedGood).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }
    }
}
