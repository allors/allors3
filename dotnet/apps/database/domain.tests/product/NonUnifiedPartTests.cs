
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

    public class NonUnifiedPartTests : DomainTest, IClassFixture<Fixture>
    {
        public NonUnifiedPartTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnPostDeriveAssertExistPart()
        {
            this.Transaction.GetSingleton().Settings.UseProductNumberCounter = true;

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Equals("NonUnifiedGood.Part is required"));
        }
    }

    public class NonUnifiedPartRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public NonUnifiedPartRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProductIdentificationsDeriveProductNumber()
        {
            var settings = this.Transaction.GetSingleton().Settings;
            settings.UsePartNumberCounter = false;

            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            Assert.False(nonUnifiedPart.ExistProductNumber);

            var goodIdentification = new ProductNumberBuilder(this.Transaction)
                .WithIdentification(settings.NextProductNumber())
                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build();

            nonUnifiedPart.AddProductIdentification(goodIdentification);
            this.Derive();

            Assert.True(nonUnifiedPart.ExistProductNumber);
        }

        [Fact]
        public void ChangedPartIdentificationDeriveName()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Contains(nonUnifiedPart.ProductIdentifications.First.Identification, nonUnifiedPart.Name);
        }

        [Fact]
        public void ChangedProductIdentificationsDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            nonUnifiedPart.AddProductIdentification(new SkuIdentificationBuilder(this.Transaction).WithIdentification("sku").WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Sku).Build());
            this.Derive();

            Assert.Contains("sku", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedLocalisedNamesDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            nonUnifiedPart.AddLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("localisedName").Build());
            this.Derive();

            Assert.Contains("localisedName", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedLocalisedTextTextDeriveSearchString()
        {
            var localisedName = new LocalisedTextBuilder(this.Transaction).WithText("localisedName").Build();
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction)
                .WithLocalisedName(localisedName)
                .Build();
            this.Derive();

            localisedName.Text = "changed";
            this.Derive();

            Assert.Contains("changed", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedPartCategoryPartsDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            new PartCategoryBuilder(this.Transaction).WithName("catname").WithPart(nonUnifiedPart).Build();
            this.Derive();

            Assert.Contains("catname", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedSupplierOfferingsPartDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            new SupplierOfferingBuilder(this.Transaction)
                .WithSupplier(this.InternalOrganisation.ActiveSuppliers.First)
                .WithPart(nonUnifiedPart)
                .Build();
            this.Derive();

            Assert.Contains(this.InternalOrganisation.ActiveSuppliers.First.PartyName, nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedSerialisedItemsDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            nonUnifiedPart.AddSerialisedItem(new SerialisedItemBuilder(this.Transaction).WithSerialNumber("serialnumber").Build());
            this.Derive();

            Assert.Contains("serialnumber", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedProductTypeDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            nonUnifiedPart.ProductType = new ProductTypeBuilder(this.Transaction).WithName("producttype").Build();
            this.Derive();

            Assert.Contains("producttype", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedBrandDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            nonUnifiedPart.Brand = new BrandBuilder(this.Transaction).WithName("brand").Build();
            this.Derive();

            Assert.Contains("brand", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedModelDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            nonUnifiedPart.Model = new ModelBuilder(this.Transaction).WithName("model").Build();
            this.Derive();

            Assert.Contains("model", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedKeywordsDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            nonUnifiedPart.Keywords = "keywords";
            this.Derive();

            Assert.Contains("keywords", nonUnifiedPart.SearchString);
        }
    }

    [Trait("Category", "Security")]
    public class NonUnifiedPartDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public NonUnifiedPartDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Transaction).Get(this.M.NonUnifiedPart, this.M.NonUnifiedPart.Delete);
        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedNonUnifiedPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            Assert.DoesNotContain(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortInventoryProducedPartDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();

            var inventoryProduced = new WorkEffortInventoryProducedBuilder(this.Transaction).Build();
            this.Derive();

            inventoryProduced.Part = nonUnifiedPart;
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortPartStandardPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            var workEffortStandard = new WorkEffortPartStandardBuilder(this.Transaction).Build();
            this.Derive();

            workEffortStandard.Part = nonUnifiedPart;
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPartBillOfMaterialPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            var bom = new ManufacturingBomBuilder(this.Transaction).Build();
            this.Derive();

            bom.Part = nonUnifiedPart;
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPartBillOfMaterialComponentPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            var bom = new ManufacturingBomBuilder(this.Transaction).Build();
            this.Derive();

            bom.ComponentPart = nonUnifiedPart;
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedNonUnifiedGoodPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            var good = new NonUnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            good.Part = nonUnifiedPart;
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseInvoiceItemPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            var invoice = new PurchaseInvoiceItemBuilder(this.Transaction).Build();
            this.Derive();

            invoice.Part = nonUnifiedPart;
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedSalesInvoiceItemPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            var invoice = new SalesInvoiceItemBuilder(this.Transaction).Build();
            this.Derive();

            invoice.Part = nonUnifiedPart;
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPurchaseOrderItemPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            var order = new PurchaseOrderItemBuilder(this.Transaction).Build();
            this.Derive();

            order.Part = nonUnifiedPart;
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedShipmentItemPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction).Build();
            this.Derive();

            shipmentItem.Part = nonUnifiedPart;
            this.Derive();

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }
    }
}
