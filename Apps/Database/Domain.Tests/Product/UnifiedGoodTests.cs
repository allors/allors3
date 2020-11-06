// <copyright file="PartTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Domain
{
    using System.Linq;
    using Allors.Domain.TestPopulation;
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

        [Fact]
        public void OnChangedNonUnifiedGoodWithSalesOrderItemDeriveDeletePermission()
        {
            var salesOrder = this.InternalOrganisation.CreateB2BSalesOrder(this.Session.Faker());
            this.Session.Derive(false);

            var item = salesOrder.SalesOrderItems.Where(v => v.Product.GetType().Name == typeof(UnifiedGood).Name).Select(v => v.Product).First();

            Assert.Contains(this.deletePermission, item.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithWorkEffortTypeDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var workEffortType = new WorkEffortTypeBuilder(this.Session).WithProductToProduce(unifiedGood).Build();          
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithWorkEffortInventoryProducedDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var workEffortInventoryProduced = new WorkEffortInventoryProducedBuilder(this.Session).WithPart(unifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithWorkEffortPartStandardDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var workEffortPartStandard = new WorkEffortPartStandardBuilder(this.Session).WithPart(unifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithPartBillOfMaterialPartDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var partBillOfMaterial = new EngineeringBomBuilder(this.Session).WithPart(unifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithPartBillOfMaterialComponentPartDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var partBillOfMaterial = new EngineeringBomBuilder(this.Session).WithComponentPart(unifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithInventoryItemTransactionDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Session).WithReason(new InventoryTransactionReasonBuilder(this.Session).Build()).WithPart(unifiedGood).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodWithSerialisedItemDeriveDeletePermission()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Session).Build();
            this.Session.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Session).Build();
            unifiedGood.AddSerialisedItem(serialisedItem);
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, unifiedGood.DeniedPermissions);
        }


    }
}
