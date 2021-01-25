
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
            this.Session.GetSingleton().Settings.UseProductNumberCounter = true;

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("NonUnifiedGood.Part is required"));
        }
    }

    public class NonUnifiedGoodDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public NonUnifiedGoodDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProductIdentificationsDeriveProductNumber()
        {
            var settings = this.Session.GetSingleton().Settings;
            settings.UseProductNumberCounter = false;

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.False(nonUnifiedGood.ExistProductNumber);

            var goodIdentification = new ProductNumberBuilder(this.Session)
                .WithIdentification(settings.NextProductNumber())
                .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Good).Build();

            nonUnifiedGood.AddProductIdentification(goodIdentification);
            this.Session.Derive(false);

            Assert.True(nonUnifiedGood.ExistProductNumber);
        }

        [Fact]
        public void ChangedLocalisedNamesDeriveName()
        {
            var defaultLocale = this.Session.GetSingleton().DefaultLocale;
            var localisedName = new LocalisedTextBuilder(this.Session).WithLocale(defaultLocale).WithText("defaultname").Build();

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            nonUnifiedGood.AddLocalisedName(localisedName);
            this.Session.Derive(false);

            Assert.Equal(nonUnifiedGood.Name, localisedName.Text);
        }

        [Fact]
        public void ChangedLocalisedTextTextDeriveName()
        {
            var defaultLocale = this.Session.GetSingleton().DefaultLocale;
            var localisedName = new LocalisedTextBuilder(this.Session).WithLocale(defaultLocale).WithText("defaultname").Build();

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).WithLocalisedName(localisedName).Build();
            this.Session.Derive(false);

            localisedName.Text = "changed";
            this.Session.Derive(false);

            Assert.Equal(nonUnifiedGood.Name, localisedName.Text);
        }

        [Fact]
        public void ChangedLocalisedDescriptionsDeriveDescription()
        {
            var defaultLocale = this.Session.GetSingleton().DefaultLocale;
            var localisedDescription = new LocalisedTextBuilder(this.Session).WithLocale(defaultLocale).WithText("defaultname").Build();

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            nonUnifiedGood.AddLocalisedDescription(localisedDescription);
            this.Session.Derive(false);

            Assert.Equal(nonUnifiedGood.Description, localisedDescription.Text);
        }

        [Fact]
        public void ChangedLocalisedTextTextDeriveDescription()
        {
            var defaultLocale = this.Session.GetSingleton().DefaultLocale;
            var localisedDescription = new LocalisedTextBuilder(this.Session).WithLocale(defaultLocale).WithText("defaultname").Build();

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).WithLocalisedDescription(localisedDescription).Build();
            this.Session.Derive(false);

            localisedDescription.Text = "changed";
            this.Session.Derive(false);

            Assert.Equal(nonUnifiedGood.Description, localisedDescription.Text);
        }

        [Fact]
        public void ChangedProductIdentificationsDeriveSearchString()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Contains(nonUnifiedGood.ProductIdentifications.First.Identification, nonUnifiedGood.SearchString);
        }

        [Fact]
        public void ChangedProductCategoryAllProductsDeriveSearchString()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            new ProductCategoryBuilder(this.Session).WithName("catname").WithProduct(nonUnifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains("catname", nonUnifiedGood.SearchString);
        }

        [Fact]
        public void ChangedKeywordsDeriveSearchString()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            nonUnifiedGood.Keywords = "keywords";
            this.Session.Derive(false);

            Assert.Contains("keywords", nonUnifiedGood.SearchString);
        }

        [Fact]
        public void ChangedVariantsDeriveVirtualProductPriceComponents()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Session)
                .WithProduct(nonUnifiedGood)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();
            this.Session.Derive(false);

            var variantGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            nonUnifiedGood.AddVariant(variantGood);
            this.Session.Derive(false);

            Assert.Equal(variantGood.VirtualProductPriceComponents.First, pricecomponent);
        }

        [Fact]
        public void ChangedVariantsDeriveVirtualProductPriceComponents_2()
        {
            var variantGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).WithVariant(variantGood).Build();
            this.Session.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Session)
                .WithProduct(nonUnifiedGood)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();
            this.Session.Derive(false);

            nonUnifiedGood.RemoveVariant(variantGood);
            this.Session.Derive(false);

            Assert.Empty(variantGood.VirtualProductPriceComponents);
        }

        [Fact]
        public void ChangedVariantsDeriveBasePrice()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var pricecomponent = new BasePriceBuilder(this.Session)
                .WithProduct(nonUnifiedGood)
                .WithPrice(1)
                .WithFromDate(this.Session.Now().AddMinutes(-1))
                .Build();
            this.Session.Derive(false);

            var variantGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            nonUnifiedGood.AddVariant(variantGood);
            this.Session.Derive(false);

            Assert.Equal(variantGood.BasePrices.First, pricecomponent);
        }
    }

    [Trait("Category", "Security")]
    public class NonUnifiedGoodDeniedPermissionDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public NonUnifiedGoodDeniedPermissionDerivationTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Session).Get(this.M.NonUnifiedGood.ObjectType, this.M.NonUnifiedGood.Delete);
        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedNonUnifiedGoodDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();

            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).WithPart(nonUnifiedPart).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithDeploymentsWhereProductOfferingDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            new DeploymentBuilder(this.Session).WithProductOffering(nonUnifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithEngagementItemsWhereProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            new GoodOrderItemBuilder(this.Session).WithProduct(nonUnifiedGood).Build();
            this.Session.Derive(false);
            
            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithGeneralLedgerAccountsWhereCostUnitsAllowedDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var generalLedgerAccounts = new GeneralLedgerAccountBuilder(this.Session).Build();
            generalLedgerAccounts.AddAssignedCostUnitsAllowed(nonUnifiedGood);
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithGeneralLedgerAccountsWhereDefaultCostUnitDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            new GeneralLedgerAccountBuilder(this.Session).WithDefaultCostUnit(nonUnifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithQuoteItemsWhereProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Session)
                .WithProduct(nonUnifiedGood)
                .WithInvoiceItemType(new InvoiceItemTypeBuilder(this.Session).Build())
                .WithUnitBasePrice(1)
                .WithAssignedUnitPrice(1)
                .Build();
            new ProductQuoteBuilder(this.Session).WithQuoteItem(quoteItem).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithShipmentItemsWhereGoodDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItem = new ShipmentItemBuilder(this.Session).WithGood(nonUnifiedGood).Build();
            new PurchaseShipmentBuilder(this.Session).WithShipmentItem(shipmentItem).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithWorkEffortGoodStandardsWhereUnifiedProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            new WorkEffortGoodStandardBuilder(this.Session).WithUnifiedProduct(nonUnifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithMarketingPackageWhereProductsUsedInDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            new MarketingPackageBuilder(this.Session).WithProductsUsedIn(nonUnifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedNonUnifiedGoodWithMarketingPackagesWhereProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            new MarketingPackageBuilder(this.Session).WithProduct(nonUnifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithOrganisationGlAccountsWhereProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            new OrganisationGlAccountBuilder(this.Session).WithProduct(nonUnifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithProductConfigurationsWhereProductsUsedInDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            new ProductConfigurationBuilder(this.Session).WithProductsUsedIn(nonUnifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithProductConfigurationsWhereProductInDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            new ProductConfigurationBuilder(this.Session).WithProduct(nonUnifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithRequestItemsWhereProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var requestItem = new RequestItemBuilder(this.Session).WithProduct(nonUnifiedGood).Build();
            var request = new RequestForQuoteBuilder(this.Session).Build();
            request.AddRequestItem(requestItem);
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithSalesInvoiceItemsWhereProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            new SalesInvoiceItemBuilder(this.Session).WithProduct(nonUnifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }
        
        [Fact]
        public void OnChangedNonUnifiedGoodWithSalesOrderItemsWhereProductDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            new SalesOrderItemBuilder(this.Session).WithProduct(nonUnifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithWorkEffortTypesWhereProductToProduceDeriveDeletePermission()
        {
            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            new WorkEffortTypeBuilder(this.Session).WithProductToProduce(nonUnifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedGood.DeniedPermissions);
        }
    }
}
