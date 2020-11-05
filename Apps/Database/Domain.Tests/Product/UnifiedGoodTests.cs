// <copyright file="PartTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Domain
{
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Xunit;

    [Trait("Category", "Security")]
    public class UnifiedGoodSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public UnifiedGoodSecurityTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Session).Get(this.M.UnifiedGood.ObjectType, this.M.UnifiedGood.Delete);

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedNonUnifiedGoodDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithDeploymentDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var deployments = new DeploymentBuilder(this.Session).WithProductOffering(unifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithEngagementItemDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var engagementItems = new GoodOrderItemBuilder(this.Session).WithProduct(unifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithGeneralLedgerAccountDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var generalLedgerAccounts = new GeneralLedgerAccountBuilder(this.Session).WithCostUnitsAllowed(unifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithQuoteItemDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var quote = new ProposalBuilder(this.Session).Build();
            this.Session.Derive(false);

            var quoteItems = new QuoteItemBuilder(this.Session).WithProduct(unifiedGood).Build();
            quote.AddQuoteItem(quoteItems);
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithShipmentItemDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipment = new TransferBuilder(this.Session).Build();
            this.Session.Derive(false);

            var shipmentItems = new ShipmentItemBuilder(this.Session).WithGood(unifiedGood).Build();
            shipment.AddShipmentItem(shipmentItems);
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithWorkEffortGoodStandardDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var workEffortGoodStandards = new WorkEffortGoodStandardBuilder(this.Session).WithUnifiedProduct(unifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithMarketingPackageProductUsedInDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var marketingPackage = new MarketingPackageBuilder(this.Session).WithProductsUsedIn(unifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithMarketingPackageProductDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var marketingPackage = new MarketingPackageBuilder(this.Session).WithProduct(unifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithOrganisationGlAccountDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var organisationGlAccount = new OrganisationGlAccountBuilder(this.Session).WithProduct(unifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithProductConfigurationProductUsedInDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var productConfiguration = new ProductConfigurationBuilder(this.Session).WithProductsUsedIn(unifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithProductConfigurationProductDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var productConfiguration = new ProductConfigurationBuilder(this.Session).WithProduct(unifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithrequestItemDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var request = new RequestForQuoteBuilder(this.Session).Build();
            this.Session.Derive(false);

            var requestItem = new RequestItemBuilder(this.Session).WithProduct(unifiedGood).Build();
            request.AddRequestItem(requestItem);
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithSalesInvoiceItemDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var salesInvoiceItem = new SalesInvoiceItemBuilder(this.Session).WithProduct(unifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        //[Fact]
        //public void OnChangedNonUnifiedGoodWithSalesOrderItemDeriveDeletePermission()
        //{
        //    var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
        //    this.Session.Derive(false);

        //    var salesOrder = new SalesOrderBuilder(this.Session).Build();

        //    var salesOrderItem = new SalesOrderItemBuilder(this.Session).WithProduct(unifiedGood).Build();
        //    salesOrder.AddSalesOrderItem(salesOrderItem);
        //    this.Session.Derive(false);

        //    Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        //}

    }
}
