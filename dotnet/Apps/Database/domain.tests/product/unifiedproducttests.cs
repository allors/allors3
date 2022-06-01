// <copyright file="UnifiedProductProductIdentificationNamesRule.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class UnifiedProductProductIdentificationNamesRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public UnifiedProductProductIdentificationNamesRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProductIdentificationsDeriveProductIdentificationNames()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            unifiedGood.AddProductIdentification(new SkuIdentificationBuilder(this.Transaction).WithIdentification("sku").WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Sku).Build());
            this.Derive();

            Assert.Contains("sku", unifiedGood.ProductIdentificationNames);
        }

        [Fact]
        public void ChangedProductIdentificationIdentificationDeriveProductIdentificationNames()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var productIdentification = new SkuIdentificationBuilder(this.Transaction).WithIdentification("sku").WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Sku).Build();
            unifiedGood.AddProductIdentification(productIdentification);
            this.Derive();

            productIdentification.Identification = "changed";
            this.Derive();

            Assert.Contains("changed", unifiedGood.ProductIdentificationNames);
        }
    }

    public class UnifiedProductUnitOfMeasureAbbreviationRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public UnifiedProductUnitOfMeasureAbbreviationRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedUnitOfMeasureDeriveUnitOfMeasureAbbreviation()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var uom = new UnitsOfMeasure(this.Transaction).SquareMeter;
            unifiedGood.UnitOfMeasure = uom;
            this.Derive();

            Assert.Equal(uom.Abbreviation, unifiedGood.UnitOfMeasureAbbreviation);
        }

        [Fact]
        public void ChangedUnitOfMeasureAbbreviationDeriveUnitOfMeasureAbbreviation()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var uom = new UnitsOfMeasure(this.Transaction).SquareMeter;
            unifiedGood.UnitOfMeasure = uom;
            this.Derive();

            uom.Abbreviation = "changed";
            this.Derive();

            Assert.Equal("changed", unifiedGood.UnitOfMeasureAbbreviation);
        }
    }

    public class UnifiedProductScopeNameRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public UnifiedProductScopeNameRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedScopeDeriveScopeName()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var scope = new Scopes(this.Transaction).Private;
            unifiedGood.Scope= scope;
            this.Derive();

            Assert.Equal(scope.Name, unifiedGood.ScopeName);
        }

        [Fact]
        public void ChangedScopeNameDeriveScopeName()
        {
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Derive();

            var scope = new Scopes(this.Transaction).Private;
            unifiedGood.Scope = scope;
            this.Derive();

            scope.Name = "changed";
            this.Derive();

            Assert.Equal("changed", unifiedGood.ScopeName);
        }
    }
}
