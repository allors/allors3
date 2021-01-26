
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
            this.Session.GetSingleton().Settings.UseProductNumberCounter = true;

            var nonUnifiedGood = new NonUnifiedGoodBuilder(this.Session).Build();

            var errors = new List<IDerivationError>(this.Session.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("NonUnifiedGood.Part is required"));
        }
    }

    public class NonUnifiedPartDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public NonUnifiedPartDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProductIdentificationsDeriveProductNumber()
        {
            var settings = this.Session.GetSingleton().Settings;
            settings.UsePartNumberCounter = false;

            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.False(nonUnifiedPart.ExistProductNumber);

            var goodIdentification = new ProductNumberBuilder(this.Session)
                .WithIdentification(settings.NextProductNumber())
                .WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Part).Build();

            nonUnifiedPart.AddProductIdentification(goodIdentification);
            this.Session.Derive(false);

            Assert.True(nonUnifiedPart.ExistProductNumber);
        }

        [Fact]
        public void ChangedPartIdentificationDeriveName()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.Contains(nonUnifiedPart.ProductIdentifications.First.Identification, nonUnifiedPart.Name);
        }

        [Fact]
        public void ChangedProductIdentificationsDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            nonUnifiedPart.AddProductIdentification(new SkuIdentificationBuilder(this.Session).WithIdentification("sku").WithProductIdentificationType(new ProductIdentificationTypes(this.Session).Sku).Build());
            this.Session.Derive(false);

            Assert.Contains("sku", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedLocalisedNamesDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            nonUnifiedPart.AddLocalisedName(new LocalisedTextBuilder(this.Session).WithText("localisedName").Build());
            this.Session.Derive(false);

            Assert.Contains("localisedName", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedLocalisedTextTextDeriveSearchString()
        {
            var localisedName = new LocalisedTextBuilder(this.Session).WithText("localisedName").Build();
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session)
                .WithLocalisedName(localisedName)
                .Build();
            this.Session.Derive(false);

            localisedName.Text = "changed";
            this.Session.Derive(false);

            Assert.Contains("changed", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedPartCategoryPartsDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            new PartCategoryBuilder(this.Session).WithName("catname").WithPart(nonUnifiedPart).Build();
            this.Session.Derive(false);

            Assert.Contains("catname", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedSupplierOfferingsPartDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            new SupplierOfferingBuilder(this.Session)
                .WithSupplier(this.InternalOrganisation.ActiveSuppliers.First)
                .WithPart(nonUnifiedPart)
                .Build();
            this.Session.Derive(false);

            Assert.Contains(this.InternalOrganisation.ActiveSuppliers.First.PartyName, nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedSerialisedItemsDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            nonUnifiedPart.AddSerialisedItem(new SerialisedItemBuilder(this.Session).WithSerialNumber("serialnumber").Build());
            this.Session.Derive(false);

            Assert.Contains("serialnumber", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedProductTypeDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            nonUnifiedPart.ProductType = new ProductTypeBuilder(this.Session).WithName("producttype").Build();
            this.Session.Derive(false);

            Assert.Contains("producttype", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedBrandDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            nonUnifiedPart.Brand = new BrandBuilder(this.Session).WithName("brand").Build();
            this.Session.Derive(false);

            Assert.Contains("brand", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedModelDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            nonUnifiedPart.Model = new ModelBuilder(this.Session).WithName("model").Build();
            this.Session.Derive(false);

            Assert.Contains("model", nonUnifiedPart.SearchString);
        }

        [Fact]
        public void ChangedKeywordsDeriveSearchString()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            nonUnifiedPart.Keywords = "keywords";
            this.Session.Derive(false);

            Assert.Contains("keywords", nonUnifiedPart.SearchString);
        }
    }

    [Trait("Category", "Security")]
    public class NonUnifiedPartDeniedPermissionDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public NonUnifiedPartDeniedPermissionDerivationTests(Fixture fixture) : base(fixture) => this.deletePermission = new Permissions(this.Session).Get(this.M.NonUnifiedPart.ObjectType, this.M.NonUnifiedPart.Delete);
        public override Config Config => new Config { SetupSecurity = true };

        private readonly Permission deletePermission;

        [Fact]
        public void OnChangedNonUnifiedPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            Assert.DoesNotContain(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortInventoryProducedPartDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            new WorkEffortInventoryProducedBuilder(this.Session).WithPart(nonUnifiedPart).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedWorkEffortPartStandardPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            new WorkEffortPartStandardBuilder(this.Session).WithPart(nonUnifiedPart).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPartBillOfMaterialPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            new ManufacturingBomBuilder(this.Session).WithPart(nonUnifiedPart).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedPartBillOfMaterialComponentPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            new ManufacturingBomBuilder(this.Session).WithComponentPart(nonUnifiedPart).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }

        [Fact]
        public void OnChangedInventoryItemTransactionPartDeriveDeletePermission()
        {
            var nonUnifiedPart = new NonUnifiedPartBuilder(this.Session).Build();
            this.Session.Derive(false);

            new InventoryItemTransactionBuilder(this.Session).WithPart(nonUnifiedPart).Build();
            this.Session.Derive(false);

            Assert.Contains(this.deletePermission, nonUnifiedPart.DeniedPermissions);
        }
    }
}
