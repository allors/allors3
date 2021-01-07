// <copyright file="OrderTermTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using TestPopulation;
    using Xunit;

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
            generalLedgerAccounts.AddCostUnitsAllowed(nonUnifiedGood);
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
