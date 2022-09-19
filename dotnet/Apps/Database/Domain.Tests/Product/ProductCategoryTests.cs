// <copyright file="ProductCategoryTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class ProductCategoryTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductCategoryTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenProductCategory_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new ProductCategoryBuilder(this.Transaction);
            var productCategory = builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithName("category");
            builder.Build();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void GivenProductCategory_WhenDeriving_ThenProductCategoriesWhereDescendantAreSet()
        {
            var productCategory1 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1")
                .Build();
            var productCategory2 = new ProductCategoryBuilder(this.Transaction)
                .WithName("2")
                .Build();
            var productCategory11 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.1")
                .WithPrimaryParent(productCategory1)
                .WithSecondaryParent(productCategory2)
                .Build();
            var productCategory12 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.2")
                .WithPrimaryParent(productCategory1)
                .WithSecondaryParent(productCategory2)
                .Build();
            var productCategory111 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.1.1")
                .WithPrimaryParent(productCategory11)
                .Build();
            var productCategory121 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.2.1")
                .WithPrimaryParent(productCategory12)
                .Build();
            var productCategory122 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.2.2")
                .WithPrimaryParent(productCategory12)
                .Build();

            this.Transaction.Derive();

            Assert.False(productCategory1.ExistProductCategoriesWhereDescendant);
            Assert.False(productCategory2.ExistProductCategoriesWhereDescendant);

            Assert.Equal(2, productCategory11.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory1, productCategory11.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory11.ProductCategoriesWhereDescendant);

            Assert.Equal(2, productCategory12.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory1, productCategory12.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory12.ProductCategoriesWhereDescendant);

            Assert.Equal(3, productCategory111.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory11, productCategory111.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory1, productCategory111.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory111.ProductCategoriesWhereDescendant);

            Assert.Equal(3, productCategory121.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory12, productCategory121.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory1, productCategory121.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory121.ProductCategoriesWhereDescendant);

            Assert.Equal(3, productCategory122.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory12, productCategory122.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory1, productCategory122.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory122.ProductCategoriesWhereDescendant);
        }

        [Fact]
        public void GivenProductCategory_WhenDeriving_ThenDisplayNameIsSet()
        {
            var productCategory1 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1")
                .Build();
            var productCategory2 = new ProductCategoryBuilder(this.Transaction)
                .WithName("2")
                .Build();
            var productCategory11 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.1")
                .WithPrimaryParent(productCategory1)
                .WithSecondaryParent(productCategory2)
                .Build();
            var productCategory12 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.2")
                .WithPrimaryParent(productCategory1)
                .WithSecondaryParent(productCategory2)
                .Build();
            var productCategory111 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.1.1")
                .WithPrimaryParent(productCategory11)
                .Build();
            var productCategory121 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.2.1")
                .WithPrimaryParent(productCategory12)
                .Build();

            this.Transaction.Derive();

            Assert.Equal("1", productCategory1.DisplayName);
            Assert.Equal("2", productCategory2.DisplayName);
            Assert.Equal("1.1/1", productCategory11.DisplayName);
            Assert.Equal("1.2/1", productCategory12.DisplayName);
            Assert.Equal("1/1.1/1.1.1", productCategory111.DisplayName);
            Assert.Equal("1/1.2/1.2.1", productCategory121.DisplayName);
        }

        [Fact]
        public void GivenProductCategory_WhenNewParentsAreInserted_ThenProductCategoriesWhereDescendantAreRecalculated()
        {
            var productCategory1 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1")
                .Build();
            var productCategory2 = new ProductCategoryBuilder(this.Transaction)
                .WithName("2")
                .Build();
            var productCategory11 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.1")
                .WithPrimaryParent(productCategory1)
                .WithSecondaryParent(productCategory2)
                .Build();
            var productCategory12 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.2")
                .WithPrimaryParent(productCategory1)
                .WithSecondaryParent(productCategory2)
                .Build();
            var productCategory111 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.1.1")
                .WithPrimaryParent(productCategory11)
                .Build();
            var productCategory121 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.2.1")
                .WithPrimaryParent(productCategory12)
                .Build();
            var productCategory122 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.2.2")
                .WithPrimaryParent(productCategory12)
                .Build();

            this.Transaction.Derive();

            Assert.False(productCategory1.ExistProductCategoriesWhereDescendant);
            Assert.False(productCategory2.ExistProductCategoriesWhereDescendant);

            Assert.Equal(2, productCategory11.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory1, productCategory11.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory11.ProductCategoriesWhereDescendant);

            Assert.Equal(2, productCategory12.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory1, productCategory12.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory12.ProductCategoriesWhereDescendant);

            Assert.Equal(3, productCategory111.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory11, productCategory111.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory1, productCategory111.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory111.ProductCategoriesWhereDescendant);

            Assert.Equal(3, productCategory121.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory12, productCategory121.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory1, productCategory121.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory121.ProductCategoriesWhereDescendant);

            Assert.Equal(3, productCategory122.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory12, productCategory122.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory1, productCategory122.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory122.ProductCategoriesWhereDescendant);

            var productCategory3 = new ProductCategoryBuilder(this.Transaction)
                .WithName("3")
                .Build();
            productCategory11.AddSecondaryParent(productCategory3);

            this.Transaction.Derive();

            Assert.False(productCategory1.ExistProductCategoriesWhereDescendant);
            Assert.False(productCategory2.ExistProductCategoriesWhereDescendant);
            Assert.False(productCategory3.ExistProductCategoriesWhereDescendant);

            Assert.Equal(3, productCategory11.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory1, productCategory11.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory11.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory3, productCategory11.ProductCategoriesWhereDescendant);

            Assert.Equal(2, productCategory12.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory1, productCategory12.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory12.ProductCategoriesWhereDescendant);

            Assert.Equal(4, productCategory111.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory11, productCategory111.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory1, productCategory111.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory111.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory3, productCategory111.ProductCategoriesWhereDescendant);

            Assert.Equal(3, productCategory121.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory12, productCategory121.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory1, productCategory121.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory121.ProductCategoriesWhereDescendant);

            Assert.Equal(3, productCategory122.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory12, productCategory122.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory1, productCategory122.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory122.ProductCategoriesWhereDescendant);

            var productCategory13 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.3")
                .WithPrimaryParent(productCategory1)
                .Build();
            productCategory122.AddSecondaryParent(productCategory13);

            this.Transaction.Derive();

            Assert.False(productCategory1.ExistProductCategoriesWhereDescendant);
            Assert.False(productCategory2.ExistProductCategoriesWhereDescendant);
            Assert.False(productCategory3.ExistProductCategoriesWhereDescendant);

            Assert.Equal(3, productCategory11.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory1, productCategory11.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory11.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory3, productCategory11.ProductCategoriesWhereDescendant);

            Assert.Equal(2, productCategory12.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory1, productCategory12.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory12.ProductCategoriesWhereDescendant);

            Assert.Single(productCategory13.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory1, productCategory13.ProductCategoriesWhereDescendant);

            Assert.Equal(4, productCategory111.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory11, productCategory111.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory1, productCategory111.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory111.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory3, productCategory111.ProductCategoriesWhereDescendant);

            Assert.Equal(3, productCategory121.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory12, productCategory121.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory1, productCategory121.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory121.ProductCategoriesWhereDescendant);

            Assert.Equal(4, productCategory122.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory12, productCategory122.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory13, productCategory122.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory1, productCategory122.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory122.ProductCategoriesWhereDescendant);
        }

        [Fact]
        public void GivenProductCategory_WhenNewParentsAreRemoved_ThenProductCategoriesWhereDescendantAreRecalculated()
        {
            var productCategory1 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1")
                .Build();
            var productCategory2 = new ProductCategoryBuilder(this.Transaction)
                .WithName("2")
                .Build();
            var productCategory11 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.1")
                .WithPrimaryParent(productCategory1)
                .WithSecondaryParent(productCategory2)
                .Build();
            var productCategory12 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.2")
                .WithPrimaryParent(productCategory1)
                .WithSecondaryParent(productCategory2)
                .Build();
            var productCategory111 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.1.1")
                .WithPrimaryParent(productCategory11)
                .Build();
            var productCategory121 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.2.1")
                .WithPrimaryParent(productCategory12)
                .Build();
            var productCategory122 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.2.2")
                .WithPrimaryParent(productCategory12)
                .Build();

            this.Transaction.Derive();

            Assert.False(productCategory1.ExistProductCategoriesWhereDescendant);
            Assert.False(productCategory2.ExistProductCategoriesWhereDescendant);

            Assert.Equal(2, productCategory11.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory1, productCategory11.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory11.ProductCategoriesWhereDescendant);

            Assert.Equal(2, productCategory12.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory1, productCategory12.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory12.ProductCategoriesWhereDescendant);

            Assert.Equal(3, productCategory111.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory11, productCategory111.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory1, productCategory111.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory111.ProductCategoriesWhereDescendant);

            Assert.Equal(3, productCategory121.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory12, productCategory121.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory1, productCategory121.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory121.ProductCategoriesWhereDescendant);

            Assert.Equal(3, productCategory122.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory12, productCategory122.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory1, productCategory122.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory122.ProductCategoriesWhereDescendant);

            productCategory11.RemoveSecondaryParent(productCategory2);

            this.Transaction.Derive();

            Assert.False(productCategory1.ExistProductCategoriesWhereDescendant);
            Assert.False(productCategory2.ExistProductCategoriesWhereDescendant);

            Assert.Single(productCategory11.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory1, productCategory11.ProductCategoriesWhereDescendant);

            Assert.Equal(2, productCategory12.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory1, productCategory12.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory12.ProductCategoriesWhereDescendant);

            Assert.Equal(2, productCategory111.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory11, productCategory111.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory1, productCategory111.ProductCategoriesWhereDescendant);

            Assert.Equal(3, productCategory121.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory12, productCategory121.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory1, productCategory121.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory121.ProductCategoriesWhereDescendant);

            Assert.Equal(3, productCategory122.ProductCategoriesWhereDescendant.Count());
            Assert.Contains(productCategory12, productCategory122.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory1, productCategory122.ProductCategoriesWhereDescendant);
            Assert.Contains(productCategory2, productCategory122.ProductCategoriesWhereDescendant);
        }

        [Fact]
        public void GivenProductCategory_WhenDeriving_ThenChildrenAreSet()
        {
            var productCategory1 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1")
                .Build();
            var productCategory2 = new ProductCategoryBuilder(this.Transaction)
                .WithName("2")
                .Build();
            var productCategory11 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.1")
                .WithPrimaryParent(productCategory1)
                .Build();
            var productCategory12 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.2")
                .WithPrimaryParent(productCategory1)
                .WithSecondaryParent(productCategory2)
                .Build();
            var productCategory111 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.1.1")
                .WithPrimaryParent(productCategory11)
                .Build();
            var productCategory121 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.2.1")
                .WithPrimaryParent(productCategory12)
                .Build();
            var productCategory122 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.2.2")
                .WithPrimaryParent(productCategory12)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(2, productCategory1.Children.Count());
            Assert.Contains(productCategory11, productCategory1.Children);
            Assert.Contains(productCategory12, productCategory1.Children);

            Assert.Single(productCategory2.Children);
            Assert.Contains(productCategory12, productCategory2.Children);

            Assert.Single(productCategory11.Children);
            Assert.Contains(productCategory111, productCategory11.Children);

            Assert.Equal(2, productCategory12.Children.Count());
            Assert.Contains(productCategory121, productCategory12.Children);
            Assert.Contains(productCategory122, productCategory12.Children);

            Assert.False(productCategory111.ExistChildren);
            Assert.False(productCategory121.ExistChildren);
            Assert.False(productCategory122.ExistChildren);
        }

        [Fact]
        public void GivenProductCategoryHierarchy_WhenDeriving_ThenAllProductAreSet()
        {
            var good1 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName("good1")
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("good1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                            .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                                .WithIdentification("1")
                                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                            .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build())
                .Build();

            var good2 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName("good2")
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("good2")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                            .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                                .WithIdentification("2")
                                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                            .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build())
                .Build();

            var good11 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName("good11")
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("good11")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                            .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                                .WithIdentification("3")
                                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                            .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build())
                .Build();

            var good12 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName("good12")
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("good12")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                            .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                                .WithIdentification("4")
                                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                            .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build())
                .Build();

            var good111 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName("good111")
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("good111")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                    .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                        .WithIdentification("5")
                        .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                    .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build())
                .Build();

            var good121 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName("good121")
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("good121")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                    .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                        .WithIdentification("6")
                        .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                    .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build())
                .Build();

            var good122 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName("good122")
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("good122")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                    .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                        .WithIdentification("7")
                        .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                    .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build())
                .Build();

            var productCategory1 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1")
                .WithProduct(good1)
                .Build();
            var productCategory2 = new ProductCategoryBuilder(this.Transaction)
                .WithName("2")
                .WithProduct(good2)
                .Build();
            var productCategory11 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.1")
                .WithPrimaryParent(productCategory1)
                .WithProduct(good11)
                .Build();
            var productCategory12 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.2")
                .WithPrimaryParent(productCategory1)
                .WithSecondaryParent(productCategory2)
                .WithProduct(good12)
                .Build();
            var productCategory111 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.1.1")
                .WithPrimaryParent(productCategory11)
                .WithProduct(good111)
                .Build();
            var productCategory121 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.2.1")
                .WithPrimaryParent(productCategory12)
                .WithProduct(good121)
                .Build();
            var productCategory122 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.2.2")
                .WithPrimaryParent(productCategory12)
                .WithProduct(good122)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(6, productCategory1.AllProducts.Count());
            Assert.Contains(good1, productCategory1.AllProducts);
            Assert.Contains(good11, productCategory1.AllProducts);
            Assert.Contains(good12, productCategory1.AllProducts);
            Assert.Contains(good111, productCategory1.AllProducts);
            Assert.Contains(good121, productCategory1.AllProducts);
            Assert.Contains(good122, productCategory1.AllProducts);

            Assert.Equal(4, productCategory2.AllProducts.Count());
            Assert.Contains(good2, productCategory2.AllProducts);
            Assert.Contains(good12, productCategory2.AllProducts);
            Assert.Contains(good121, productCategory2.AllProducts);
            Assert.Contains(good122, productCategory2.AllProducts);

            Assert.Equal(2, productCategory11.AllProducts.Count());
            Assert.Contains(good11, productCategory11.AllProducts);
            Assert.Contains(good111, productCategory11.AllProducts);

            Assert.Equal(3, productCategory12.AllProducts.Count());
            Assert.Contains(good12, productCategory12.AllProducts);
            Assert.Contains(good121, productCategory12.AllProducts);
            Assert.Contains(good122, productCategory12.AllProducts);

            Assert.Single(productCategory111.AllProducts);
            Assert.Contains(good111, productCategory111.AllProducts);

            Assert.Single(productCategory121.AllProducts);
            Assert.Contains(good121, productCategory121.AllProducts);

            Assert.Single(productCategory122.AllProducts);
            Assert.Contains(good122, productCategory122.AllProducts);
        }

        [Fact]
        public void GivenProductCategoryHierarchy_WhenDeriving_ThenAllSerialisedItemsForSaleAreSet()
        {
            var good1 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName("good1")
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("good1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                            .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                                .WithIdentification("1")
                                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                            .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised)
                            .Build())
                .Build();

            var serialisedItem1 = new SerialisedItemBuilder(this.Transaction).WithSerialNumber("1").WithAvailableForSale(true).Build();
            var serialisedItem1Not = new SerialisedItemBuilder(this.Transaction).WithSerialNumber("1Not").Build();  // This one must be excluded
            good1.Part.AddSerialisedItem(serialisedItem1);
            good1.Part.AddSerialisedItem(serialisedItem1Not);

            var good2 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName("good2")
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("good2")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                            .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                                .WithIdentification("2")
                                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                            .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised)
                            .Build())
                .Build();

            var serialisedItem2a = new SerialisedItemBuilder(this.Transaction).WithSerialNumber("2a").WithAvailableForSale(true).Build();
            var serialisedItem2b = new SerialisedItemBuilder(this.Transaction).WithSerialNumber("2b").WithAvailableForSale(true).Build();
            good2.Part.AddSerialisedItem(serialisedItem2a);
            good2.Part.AddSerialisedItem(serialisedItem2b);

            var good11 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName("good11")
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("good11")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                            .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                                .WithIdentification("3")
                                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                            .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised)
                            .Build())
                .Build();

            var serialisedItem11 = new SerialisedItemBuilder(this.Transaction).WithSerialNumber("11").WithAvailableForSale(true).Build();
            good11.Part.AddSerialisedItem(serialisedItem11);

            var good12 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName("good12")
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("good12")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                            .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                                .WithIdentification("4")
                                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                            .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised)
                            .Build())
                .Build();

            var serialisedItem12 = new SerialisedItemBuilder(this.Transaction).WithSerialNumber("12").WithAvailableForSale(true).Build();
            good12.Part.AddSerialisedItem(serialisedItem12);

            var good111 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName("good111")
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("good111")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                            .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                                .WithIdentification("5")
                                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                            .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised)
                            .Build())
                .Build();

            var serialisedItem111 = new SerialisedItemBuilder(this.Transaction).WithSerialNumber("111").WithAvailableForSale(true).Build();
            good111.Part.AddSerialisedItem(serialisedItem111);

            var good121 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName("good121")
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("good121")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                            .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                                .WithIdentification("6")
                                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                            .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised)
                            .Build())
                .Build();

            var serialisedItem121 = new SerialisedItemBuilder(this.Transaction).WithSerialNumber("121").WithAvailableForSale(true).Build();
            good121.Part.AddSerialisedItem(serialisedItem121);

            var good122 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName("good122")
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("good122")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                            .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                                .WithIdentification("7")
                                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                            .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised)
                            .Build())
                .Build();

            var serialisedItem122 = new SerialisedItemBuilder(this.Transaction).WithSerialNumber("122").WithAvailableForSale(true).Build();
            good122.Part.AddSerialisedItem(serialisedItem122);

            var productCategory1 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1")
                .WithProduct(good1)
                .Build();
            var productCategory2 = new ProductCategoryBuilder(this.Transaction)
                .WithName("2")
                .WithProduct(good2)
                .Build();
            var productCategory11 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.1")
                .WithPrimaryParent(productCategory1)
                .WithProduct(good11)
                .Build();
            var productCategory12 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.2")
                .WithPrimaryParent(productCategory1)
                .WithSecondaryParent(productCategory2)
                .WithProduct(good12)
                .Build();
            var productCategory111 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.1.1")
                .WithPrimaryParent(productCategory11)
                .WithProduct(good111)
                .Build();
            var productCategory121 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.2.1")
                .WithPrimaryParent(productCategory12)
                .WithProduct(good121)
                .Build();
            var productCategory122 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.2.2")
                .WithPrimaryParent(productCategory12)
                .WithProduct(good122)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(6, productCategory1.AllSerialisedItemsForSale.Count());
            Assert.Contains(serialisedItem1, productCategory1.AllSerialisedItemsForSale);
            Assert.Contains(serialisedItem11, productCategory1.AllSerialisedItemsForSale);
            Assert.Contains(serialisedItem12, productCategory1.AllSerialisedItemsForSale);
            Assert.Contains(serialisedItem111, productCategory1.AllSerialisedItemsForSale);
            Assert.Contains(serialisedItem121, productCategory1.AllSerialisedItemsForSale);
            Assert.Contains(serialisedItem122, productCategory1.AllSerialisedItemsForSale);

            Assert.Equal(5, productCategory2.AllSerialisedItemsForSale.Count());
            Assert.Contains(serialisedItem2a, productCategory2.AllSerialisedItemsForSale);
            Assert.Contains(serialisedItem2b, productCategory2.AllSerialisedItemsForSale);
            Assert.Contains(serialisedItem12, productCategory2.AllSerialisedItemsForSale);
            Assert.Contains(serialisedItem121, productCategory2.AllSerialisedItemsForSale);
            Assert.Contains(serialisedItem122, productCategory2.AllSerialisedItemsForSale);

            Assert.Equal(2, productCategory11.AllSerialisedItemsForSale.Count());
            Assert.Contains(serialisedItem11, productCategory11.AllSerialisedItemsForSale);
            Assert.Contains(serialisedItem111, productCategory11.AllSerialisedItemsForSale);

            Assert.Equal(3, productCategory12.AllSerialisedItemsForSale.Count());
            Assert.Contains(serialisedItem12, productCategory12.AllSerialisedItemsForSale);
            Assert.Contains(serialisedItem121, productCategory12.AllSerialisedItemsForSale);
            Assert.Contains(serialisedItem122, productCategory12.AllSerialisedItemsForSale);

            Assert.Single(productCategory111.AllSerialisedItemsForSale);
            Assert.Contains(serialisedItem111, productCategory111.AllSerialisedItemsForSale);

            Assert.Single(productCategory121.AllSerialisedItemsForSale);
            Assert.Contains(serialisedItem121, productCategory121.AllSerialisedItemsForSale);

            Assert.Single(productCategory122.AllSerialisedItemsForSale);
            Assert.Contains(serialisedItem122, productCategory122.AllSerialisedItemsForSale);
        }

        [Fact]
        public void GivenProductCategoryHierarchy_WhenDeriving_ThenAllNonSerialisedInventoryItemsForSaleAreSet()
        {
            var good1 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName("good1")
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("good1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                            .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                                .WithIdentification("1")
                                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                            .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                            .WithDefaultFacility(this.InternalOrganisation.FacilitiesWhereOwner.FirstOrDefault())
                            .Build())
                .Build();

            this.Transaction.Derive();

            new NonSerialisedInventoryItemBuilder(this.Transaction).WithPart(good1.Part).WithNonSerialisedInventoryItemState(new NonSerialisedInventoryItemStates(this.Transaction).Scrap).Build();

            this.Transaction.Derive();

            var good2 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName("good2")
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("good2")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                            .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                                .WithIdentification("2")
                                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                            .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                            .WithDefaultFacility(this.InternalOrganisation.FacilitiesWhereOwner.FirstOrDefault())
                            .Build())
                .Build();

            this.Transaction.Derive();

            new NonSerialisedInventoryItemBuilder(this.Transaction).WithPart(good2.Part).WithNonSerialisedInventoryItemState(new NonSerialisedInventoryItemStates(this.Transaction).SlightlyDamaged).Build();

            this.Transaction.Derive();

            var good11 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName("good11")
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("good11")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                            .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                                .WithIdentification("3")
                                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                            .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                            .WithDefaultFacility(this.InternalOrganisation.FacilitiesWhereOwner.FirstOrDefault())
                            .Build())
                .Build();

            this.Transaction.Derive();

            var good12 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName("good12")
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("good12")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                            .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                                .WithIdentification("4")
                                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                            .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                            .WithDefaultFacility(this.InternalOrganisation.FacilitiesWhereOwner.FirstOrDefault())
                            .Build())
                .Build();

            this.Transaction.Derive();

            var good111 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName("good111")
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("good111")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                            .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                                .WithIdentification("5")
                                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                            .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                            .WithDefaultFacility(this.InternalOrganisation.FacilitiesWhereOwner.FirstOrDefault())
                            .Build())
                .Build();

            this.Transaction.Derive();

            var good121 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName("good121")
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("good121")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                            .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                                .WithIdentification("6")
                                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                            .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                            .WithDefaultFacility(this.InternalOrganisation.FacilitiesWhereOwner.FirstOrDefault())
                            .Build())
                .Build();

            this.Transaction.Derive();

            var good122 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithName("good122")
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("good122")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithVatRegime(new VatRegimes(this.Transaction).BelgiumStandard)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                            .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                                .WithIdentification("7")
                                .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                            .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                            .WithDefaultFacility(this.InternalOrganisation.FacilitiesWhereOwner.FirstOrDefault())
                            .Build())
                .Build();

            this.Transaction.Derive();

            var productCategory1 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1")
                .WithProduct(good1)
                .Build();
            this.Transaction.Derive();

            var productCategory2 = new ProductCategoryBuilder(this.Transaction)
                .WithName("2")
                .WithProduct(good2)
                .Build();
            this.Transaction.Derive();

            var productCategory11 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.1")
                .WithPrimaryParent(productCategory1)
                .WithProduct(good11)
                .Build();
            this.Transaction.Derive();

            var productCategory12 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.2")
                .WithPrimaryParent(productCategory1)
                .WithSecondaryParent(productCategory2)
                .WithProduct(good12)
                .Build();
            this.Transaction.Derive();

            var productCategory111 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.1.1")
                .WithPrimaryParent(productCategory11)
                .WithProduct(good111)
                .Build();
            this.Transaction.Derive();

            var productCategory121 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.2.1")
                .WithPrimaryParent(productCategory12)
                .WithProduct(good121)
                .Build();
            this.Transaction.Derive();

            var productCategory122 = new ProductCategoryBuilder(this.Transaction)
                .WithName("1.2.2")
                .WithPrimaryParent(productCategory12)
                .WithProduct(good122)
                .Build();
            this.Transaction.Derive();

            Assert.Equal(6, productCategory1.AllNonSerialisedInventoryItemsForSale.Count());

            Assert.Equal(5, productCategory2.AllNonSerialisedInventoryItemsForSale.Count());

            Assert.Equal(2, productCategory11.AllNonSerialisedInventoryItemsForSale.Count());

            Assert.Equal(3, productCategory12.AllNonSerialisedInventoryItemsForSale.Count());

            Assert.Single(productCategory111.AllNonSerialisedInventoryItemsForSale);

            Assert.Single(productCategory121.AllNonSerialisedInventoryItemsForSale);

            Assert.Single(productCategory122.AllNonSerialisedInventoryItemsForSale);
        }
    }

    public class ProductCategoryRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductCategoryRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedNameDeriveDisplayName()
        {
            var productCategory = new ProductCategoryBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Null(productCategory.DisplayName);

            productCategory.Name = "name";
            this.Derive();

            Assert.Equal("name", productCategory.DisplayName);
        }

        [Fact]
        public void ChangedLocalisedTextTextDeriveName()
        {
            var defaultLocale = this.Transaction.GetSingleton().DefaultLocale;

            var productCategory = new ProductCategoryBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Null(productCategory.Name);

            productCategory.AddLocalisedName(new LocalisedTextBuilder(this.Transaction).WithLocale(defaultLocale).WithText("name").Build());
            this.Derive();

            Assert.Equal("name", productCategory.Name);
        }

        [Fact]
        public void ChangedLocalisedTextTextDeriveDescription()
        {
            var defaultLocale = this.Transaction.GetSingleton().DefaultLocale;

            var productCategory = new ProductCategoryBuilder(this.Transaction).Build();
            this.Derive();

            Assert.Null(productCategory.Description);

            productCategory.AddLocalisedDescription(new LocalisedTextBuilder(this.Transaction).WithLocale(defaultLocale).WithText("description").Build());
            this.Derive();

            Assert.Equal("description", productCategory.Description);
        }

        [Fact]
        public void ChangedCategoryImageDeriveCategoryImage()
        {
            var noImageAvailableImage = this.Transaction.GetSingleton().Settings.NoImageAvailableImage;

            var productCategory = new ProductCategoryBuilder(this.Transaction).WithName("name").Build();
            this.Derive();

            productCategory.RemoveCategoryImage();
            this.Derive();

            Assert.Equal(noImageAvailableImage, productCategory.CategoryImage);
        }

        [Fact]
        public void ChangedPrimaryParentValidationError()
        {
            var productCategory1 = new ProductCategoryBuilder(this.Transaction).Build();
            this.Derive();

            var productCategory2 = new ProductCategoryBuilder(this.Transaction).WithPrimaryParent(productCategory1).Build();
            this.Derive();

            productCategory1.PrimaryParent = productCategory2;

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains("Cycle detected in"));
        }

        [Fact]
        public void ChangedPrimaryParentDeriveDisplayName()
        {
            var productCategory1 = new ProductCategoryBuilder(this.Transaction).WithName("1").Build();
            this.Derive();

            var productCategory11 = new ProductCategoryBuilder(this.Transaction).WithName("1.1").WithPrimaryParent(productCategory1).Build();
            this.Derive();

            this.Derive();

            Assert.Equal("1.1/1", productCategory11.DisplayName);
        }

        [Fact]
        public void ChangedPrimaryParentDeriveChildren()
        {
            var productCategory1 = new ProductCategoryBuilder(this.Transaction).Build();
            this.Derive();

            var productCategory11 = new ProductCategoryBuilder(this.Transaction).WithPrimaryParent(productCategory1).Build();
            this.Derive();

            Assert.Single(productCategory1.Children);
            Assert.Contains(productCategory11, productCategory1.Children);
        }

        [Fact]
        public void ChangedSecondaryParentDeriveChildren()
        {
            var productCategory1 = new ProductCategoryBuilder(this.Transaction).Build();
            this.Derive();

            var productCategory11 = new ProductCategoryBuilder(this.Transaction).WithPrimaryParent(productCategory1).Build();
            this.Derive();

            var productCategory12 = new ProductCategoryBuilder(this.Transaction).WithSecondaryParent(productCategory1).Build();
            this.Derive();

            Assert.Equal(2, productCategory1.Children.Count());
            Assert.Contains(productCategory11, productCategory1.Children);
            Assert.Contains(productCategory12, productCategory1.Children);
        }

        [Fact]
        public void ChangedChildrenDeriveDescendants()
        {
            var productCategory111 = new ProductCategoryBuilder(this.Transaction).Build();
            this.Derive();

            var productCategory11 = new ProductCategoryBuilder(this.Transaction).Build();
            this.Derive();

            var productCategory1 = new ProductCategoryBuilder(this.Transaction).Build();
            this.Derive();

            productCategory11.PrimaryParent = productCategory1;
            productCategory111.PrimaryParent = productCategory11;
            this.Derive();

            Assert.Equal(2, productCategory1.Descendants.Count());
            Assert.Contains(productCategory11, productCategory1.Descendants);
            Assert.Contains(productCategory111, productCategory1.Descendants);
        }

        [Fact]
        public void ChangedProductsDeriveAllProducts()
        {
            var productCategory1 = new ProductCategoryBuilder(this.Transaction)
                .WithProduct(new NonUnifiedGoodBuilder(this.Transaction).Build())
                .Build();
            this.Derive();

            var productCategory11 = new ProductCategoryBuilder(this.Transaction)
                .WithProduct(new NonUnifiedGoodBuilder(this.Transaction).Build())
                .WithPrimaryParent(productCategory1)
                .Build();
            this.Derive();

            var productCategory111 = new ProductCategoryBuilder(this.Transaction)
                .WithProduct(new NonUnifiedGoodBuilder(this.Transaction).Build())
                .WithPrimaryParent(productCategory11)
                .Build();
            this.Derive();

            Assert.Equal(3, productCategory1.AllProducts.Count());

            var good = new NonUnifiedGoodBuilder(this.Transaction).Build();
            productCategory111.AddProduct(good);
            this.Derive();

            Assert.Equal(4, productCategory1.AllProducts.Count());
            Assert.Contains(good, productCategory1.AllProducts);
        }

        [Fact]
        public void ChangedProductsDeriveAllParts()
        {
            var productCategory1 = new ProductCategoryBuilder(this.Transaction)
                .WithProduct(new NonUnifiedGoodBuilder(this.Transaction)
                                .WithPart(new NonUnifiedPartBuilder(this.Transaction).Build())
                                .Build())
                .Build();
            this.Derive();

            var productCategory11 = new ProductCategoryBuilder(this.Transaction)
                .WithProduct(new NonUnifiedGoodBuilder(this.Transaction)
                                .WithPart(new NonUnifiedPartBuilder(this.Transaction).Build())
                                .Build())
                .WithPrimaryParent(productCategory1)
                .Build();
            this.Derive();

            var productCategory111 = new ProductCategoryBuilder(this.Transaction)
                .WithProduct(new NonUnifiedGoodBuilder(this.Transaction)
                                .WithPart(new NonUnifiedPartBuilder(this.Transaction).Build())
                                .Build())
                .WithPrimaryParent(productCategory11)
                .Build();
            this.Derive();

            Assert.Equal(3, productCategory1.AllParts.Count());

            var good = new NonUnifiedGoodBuilder(this.Transaction)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction).Build())
                .Build();
            productCategory111.AddProduct(good);
            this.Derive();

            Assert.Equal(4, productCategory1.AllParts.Count());
            Assert.Contains(good.Part, productCategory1.AllParts);
        }

        [Fact]
        public void ChangedProductsDeriveAllSerialisedItemsForSale()
        {
            var productCategory1 = new ProductCategoryBuilder(this.Transaction)
                .WithProduct(new NonUnifiedGoodBuilder(this.Transaction)
                                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                                            .WithSerialisedItem(new SerialisedItemBuilder(this.Transaction).WithAvailableForSale(true).Build())
                                            .Build())
                                .Build())
                .Build();
            this.Derive();

            var productCategory11 = new ProductCategoryBuilder(this.Transaction)
                .WithProduct(new NonUnifiedGoodBuilder(this.Transaction)
                                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                                            .WithSerialisedItem(new SerialisedItemBuilder(this.Transaction).WithAvailableForSale(true).Build())
                                            .Build())
                                .Build())
                .WithPrimaryParent(productCategory1)
                .Build();
            this.Derive();

            var productCategory111 = new ProductCategoryBuilder(this.Transaction)
                .WithProduct(new NonUnifiedGoodBuilder(this.Transaction)
                                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                                            .WithSerialisedItem(new SerialisedItemBuilder(this.Transaction).WithAvailableForSale(true).Build())
                                            .Build())
                                .Build())
                .WithPrimaryParent(productCategory11)
                .Build();
            this.Derive();

            Assert.Equal(3, productCategory1.AllSerialisedItemsForSale.Count());

            var good = new NonUnifiedGoodBuilder(this.Transaction)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                            .WithSerialisedItem(new SerialisedItemBuilder(this.Transaction).WithAvailableForSale(true).Build())
                            .Build())
                .Build();
            productCategory111.AddProduct(good);
            this.Derive();

            Assert.Equal(4, productCategory1.AllSerialisedItemsForSale.Count());
            Assert.Contains(good.Part.SerialisedItems.ElementAt(0), productCategory1.AllSerialisedItemsForSale);
        }

        [Fact]
        public void ChangedProductsDeriveAllNonSerialisedInventoryItemsForSale()
        {
            var productCategory1 = new ProductCategoryBuilder(this.Transaction)
                .WithProduct(new NonUnifiedGoodBuilder(this.Transaction)
                                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                                            .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                                            .Build())
                                .Build())
                .Build();
            this.Derive();

            var productCategory11 = new ProductCategoryBuilder(this.Transaction)
                .WithProduct(new NonUnifiedGoodBuilder(this.Transaction)
                                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                                            .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                                            .Build())
                                .Build())
                .WithPrimaryParent(productCategory1)
                .Build();
            this.Derive();

            var productCategory111 = new ProductCategoryBuilder(this.Transaction)
                .WithProduct(new NonUnifiedGoodBuilder(this.Transaction)
                                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                                            .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                                            .Build())
                                .Build())
                .WithPrimaryParent(productCategory11)
                .Build();
            this.Derive();

            Assert.Equal(3, productCategory1.AllNonSerialisedInventoryItemsForSale.Count());

            var good = new NonUnifiedGoodBuilder(this.Transaction)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                            .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                            .Build())
                .Build();
            productCategory111.AddProduct(good);
            this.Derive();

            Assert.Equal(4, productCategory1.AllNonSerialisedInventoryItemsForSale.Count());
            Assert.Contains(good.Part.InventoryItemsWherePart.ElementAt(0), productCategory1.AllNonSerialisedInventoryItemsForSale);
        }
    }
}
