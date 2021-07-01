// <copyright file="SalesInvoiceItemBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>

namespace Allors.Database.Domain.TestPopulation
{
    using System.Linq;

    public static partial class SalesInvoiceItemBuilderExtensions
    {
        public static SalesInvoiceItemBuilder WithDefaults(this SalesInvoiceItemBuilder @this)
        {
            var faker = @this.Transaction.Faker();
            var invoiceItemTypes = @this.Transaction.Extent<InvoiceItemType>().ToList();

            var otherInvoiceItemTypes = invoiceItemTypes.Except(
                invoiceItemTypes.Where(v => v.UniqueId.Equals(InvoiceItemTypes.ProductItemId) || v.UniqueId.Equals(InvoiceItemTypes.PartItemId)).ToList())
                .ToList();

            @this.WithDescription(faker.Lorem.Sentences(2))
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithInvoiceItemType(faker.Random.ListItem(otherInvoiceItemTypes))
                .WithMessage(faker.Lorem.Sentence())
                .WithQuantity(1)
                .WithAssignedUnitPrice(faker.Random.UInt(1, 100));

            return @this;
        }

        public static SalesInvoiceItemBuilder WithProductItemDefaults(this SalesInvoiceItemBuilder @this)
        {
            var m = @this.Transaction.Database.Services().M;
            var faker = @this.Transaction.Faker();
            var invoiceItemType = @this.Transaction.Extent<InvoiceItemType>().FirstOrDefault(v => v.UniqueId.Equals(InvoiceItemTypes.ProductItemId));

            var unifiedGoodExtent = @this.Transaction.Extent<UnifiedGood>();
            unifiedGoodExtent.Filter.AddEquals(m.UnifiedGood.InventoryItemKind, new InventoryItemKinds(@this.Transaction).Serialised);
            var serializedProduct = unifiedGoodExtent.First();

            @this.WithDescription(faker.Lorem.Sentences(2))
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithInvoiceItemType(invoiceItemType)
                .WithProduct(serializedProduct)
                .WithSerialisedItem(serializedProduct.SerialisedItems.FirstOrDefault())
                .WithNextSerialisedItemAvailability(faker.Random.ListItem(@this.Transaction.Extent<SerialisedItemAvailability>()))
                .WithMessage(faker.Lorem.Sentence())
                .WithQuantity(1)
                .WithAssignedUnitPrice(faker.Random.UInt(1, 100));

            return @this;
        }

        public static SalesInvoiceItemBuilder WithPartItemDefaults(this SalesInvoiceItemBuilder @this)
        {
            var m = @this.Transaction.Database.Services().M;
            var faker = @this.Transaction.Faker();
            var invoiceItemType = @this.Transaction.Extent<InvoiceItemType>().FirstOrDefault(v => v.UniqueId.Equals(InvoiceItemTypes.ProductItemId));

            var unifiedGoodExtent = @this.Transaction.Extent<UnifiedGood>();
            unifiedGoodExtent.Filter.AddEquals(m.UnifiedGood.InventoryItemKind, new InventoryItemKinds(@this.Transaction).Serialised);
            var serializedPart = unifiedGoodExtent.First();

            @this.WithDescription(faker.Lorem.Sentences(2))
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithInvoiceItemType(invoiceItemType)
                .WithProduct(serializedPart)
                .WithSerialisedItem(serializedPart.SerialisedItems.FirstOrDefault())
                .WithNextSerialisedItemAvailability(faker.Random.ListItem(@this.Transaction.Extent<SerialisedItemAvailability>()))
                .WithMessage(faker.Lorem.Sentence())
                .WithQuantity(1)
                .WithAssignedUnitPrice(faker.Random.UInt(1, 100));

            return @this;
        }
    }
}
