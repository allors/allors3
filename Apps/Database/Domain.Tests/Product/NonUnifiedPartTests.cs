
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

    public class NonUnifiedPartTests : DomainTest, IClassFixture<Fixture>
    {
        public NonUnifiedPartTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnPostDeriveAssertExistPart()
        {
            this.Transaction.GetSingleton().Settings.UseProductNumberCounter = true;

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Transaction).Build();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("NonUnifiedGood.Part is required"));
        }
    }

    public class NonUnifiedPartDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public NonUnifiedPartDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProductIdentificationsDeriveProductNumber()
        {
            var settings = this.Transaction.GetSingleton().Settings;
            settings.UsePartNumberCounter = false;

            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.False(nonUnifiedPart.ExistProductNumber);

            var goodIdentification = new ProductNumberBuilder(this.Transaction)
                .WithIdentification(settings.NextProductNumber())
                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build();

            nonUnifiedPart.AddProductIdentification(goodIdentification);
            this.Transaction.Derive(false);

            Assert.True(nonUnifiedPart.ExistProductNumber);
        }

        [Fact]
        public void ChangedPartIdentificationDeriveName()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Contains(nonUnifiedPart.ProductIdentifications.First.Identification, nonUnifiedPart.Name);
        }

        [Fact]
        public void ChangedProductIdentificationsDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            nonUnifiedPart.AddProductIdentification(new SkuIdentificationBuilder(this.Transaction).WithIdentification("sku").WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Sku).Build());
            this.Transaction.Derive(false);

            Assert.Contains("sku", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedLocalisedNamesDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            nonUnifiedPart.AddLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("localisedName").Build());
            this.Transaction.Derive(false);

            Assert.Contains("localisedName", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedLocalisedTextTextDeriveSearchString()
        {
            var localisedName = new LocalisedTextBuilder(this.Transaction).WithText("localisedName").Build();
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction)
                .WithLocalisedName(localisedName)
                .Build();
            this.Transaction.Derive(false);

            localisedName.Text = "changed";
            this.Transaction.Derive(false);

            Assert.Contains("changed", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedPartCategoryPartsDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new PartCategoryBuilder(this.Transaction).WithName("catname").WithPart(nonUnifiedPart).Build();
            this.Transaction.Derive(false);

            Assert.Contains("catname", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedSupplierOfferingsPartDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new SupplierOfferingBuilder(this.Transaction)
                .WithSupplier(this.InternalOrganisation.ActiveSuppliers.First)
                .WithPart(nonUnifiedPart)
                .Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.InternalOrganisation.ActiveSuppliers.First.PartyName, nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedSerialisedItemsDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            nonUnifiedPart.AddSerialisedItem(new SerialisedItemBuilder(this.Transaction).WithSerialNumber("serialnumber").Build());
            this.Transaction.Derive(false);

            Assert.Contains("serialnumber", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedProductTypeDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            nonUnifiedPart.ProductType = new ProductTypeBuilder(this.Transaction).WithName("producttype").Build();
            this.Transaction.Derive(false);

            Assert.Contains("producttype", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedBrandDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            nonUnifiedPart.Brand = new BrandBuilder(this.Transaction).WithName("brand").Build();
            this.Transaction.Derive(false);

            Assert.Contains("brand", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedModelDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            nonUnifiedPart.Model = new ModelBuilder(this.Transaction).WithName("model").Build();
            this.Transaction.Derive(false);

            Assert.Contains("model", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedKeywordsDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            nonUnifiedPart.Keywords = "keywords";
            this.Transaction.Derive(false);

            Assert.Contains("keywords", nonUnifiedPart.SearchString);
        }
    }

    [Trait("Category", "Security")]
    public class NonUnifiedPartDeniedPermissionDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public NonUnifiedPartDeniedPermissionDerivationTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Transaction).Get(this.M.NonUnifiedPart.ObjectType, this.M.NonUnifiedPart.Delete);
        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedNonUnifiedPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortInventoryProducedPartDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();

            var inventoryProduced = new WorkEffortInventoryProducedBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            inventoryProduced.Part = nonUnifiedPart;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortPartStandardPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workEffortStandard = new WorkEffortPartStandardBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            workEffortStandard.Part = nonUnifiedPart;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPartBillOfMaterialPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var bom = new ManufacturingBomBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            bom.Part = nonUnifiedPart;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPartBillOfMaterialComponentPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var bom = new ManufacturingBomBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            bom.ComponentPart = nonUnifiedPart;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedInventoryItemTransactionPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var transaction = new InventoryItemTransactionBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            transaction.Part = nonUnifiedPart;
            this.Transaction.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }
    }
}
