// <copyright file="SalesOrderItemBuilderExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>

namespace Allors.Database.Domain.TestPopulation
{
    using System.Linq;
    using Meta;
    using InvoiceItemType = InvoiceItemType;
    using NonUnifiedGood = NonUnifiedGood;
    using SerialisedItemAvailability = SerialisedItemAvailability;
    using UnifiedGood = UnifiedGood;

    public static partial class SalesOrderItemBuilderExtensions
    {
        public static SalesOrderItemBuilder WithDefaults(this SalesOrderItemBuilder @this)
        {
            var m = @this.Transaction.Database.Services.Get<MetaPopulation>();
            var faker = @this.Transaction.Faker();
            var invoiceItemTypes = @this.Transaction.Extent<InvoiceItemType>().ToList();

            var otherInvoiceItemTypes = invoiceItemTypes.Except(
                invoiceItemTypes.Where(v => v.UniqueId.Equals(InvoiceItemTypes.ProductItemId) || v.UniqueId.Equals(InvoiceItemTypes.PartItemId)).ToList())
                .ToList();

            var unifiedGoodExtent = @this.Transaction.Extent<UnifiedGood>();
            unifiedGoodExtent.Filter.AddEquals(m.UnifiedGood.InventoryItemKind, new InventoryItemKinds(@this.Transaction).Serialised);
            var serializedProduct = unifiedGoodExtent.FirstOrDefault();

            @this.WithDescription(faker.Lorem.Sentences(2))
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithInvoiceItemType(faker.Random.ListItem(otherInvoiceItemTypes))
                .WithQuantityOrdered(faker.Random.UInt(1, 10))
                .WithAssignedUnitPrice(faker.Random.UInt());

            return @this;
        }

        public static SalesOrderItemBuilder WithSerialisedProductDefaults(this SalesOrderItemBuilder @this)
        {
            var m = @this.Transaction.Database.Services.Get<MetaPopulation>();
            var faker = @this.Transaction.Faker();
            var invoiceItemType = @this.Transaction.Extent<InvoiceItemType>().FirstOrDefault(v => v.UniqueId.Equals(InvoiceItemTypes.ProductItemId));

            var unifiedGoodExtent = @this.Transaction.Extent<UnifiedGood>();
            unifiedGoodExtent.Filter.AddEquals(m.UnifiedGood.InventoryItemKind, new InventoryItemKinds(@this.Transaction).Serialised);
            var serializedProduct = unifiedGoodExtent.First();

            @this.WithDescription(faker.Lorem.Sentences(2))
                .WithInvoiceItemType(invoiceItemType)
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithProduct(serializedProduct)
                .WithSerialisedItem(serializedProduct.SerialisedItems.FirstOrDefault())
                .WithNextSerialisedItemAvailability(faker.Random.ListItem(@this.Transaction.Extent<SerialisedItemAvailability>()))
                .WithQuantityOrdered(1)
                .WithAssignedUnitPrice(faker.Random.UInt());

            return @this;
        }

        // TODO: Martien
        public static SalesOrderItemBuilder WithPartItemDefaults(this SalesOrderItemBuilder @this)
        {
            var m = @this.Transaction.Database.Services.Get<MetaPopulation>();
            var faker = @this.Transaction.Faker();
            var invoiceItemType = @this.Transaction.Extent<InvoiceItemType>().FirstOrDefault(v => v.UniqueId.Equals(InvoiceItemTypes.PartItemId));

            var unifiedGoodExtent = @this.Transaction.Extent<UnifiedGood>();
            unifiedGoodExtent.Filter.AddEquals(m.UnifiedGood.InventoryItemKind, new InventoryItemKinds(@this.Transaction).Serialised);
            var serializedPart = unifiedGoodExtent[0];

            @this.WithDescription(faker.Lorem.Sentences(2))
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithInvoiceItemType(invoiceItemType)
                .WithProduct(serializedPart)
                .WithSerialisedItem(serializedPart.SerialisedItems.FirstOrDefault())
                .WithNextSerialisedItemAvailability(faker.Random.ListItem(@this.Transaction.Extent<SerialisedItemAvailability>()))
                .WithQuantityOrdered(1)
                .WithAssignedUnitPrice(faker.Random.UInt());

            return @this;
        }

        public static SalesOrderItemBuilder WithNonSerialisedPartItemDefaults(this SalesOrderItemBuilder @this)
        {
            var m = @this.Transaction.Database.Services.Get<MetaPopulation>();
            var faker = @this.Transaction.Faker();
            var invoiceItemType = @this.Transaction.Extent<InvoiceItemType>().FirstOrDefault(v => v.UniqueId.Equals(InvoiceItemTypes.PartItemId));

            var product = @this.Transaction.Extent<NonUnifiedGood>().First(v => v.Part.InventoryItemKind.Equals(new InventoryItemKinds(@this.Transaction).NonSerialised));

            @this.WithDescription(faker.Lorem.Sentences(2))
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithInvoiceItemType(invoiceItemType)
                .WithProduct(product)
                .WithQuantityOrdered(faker.Random.UInt(2, 100))
                .WithAssignedUnitPrice(faker.Random.UInt());

            return @this;
        }
    }
}
