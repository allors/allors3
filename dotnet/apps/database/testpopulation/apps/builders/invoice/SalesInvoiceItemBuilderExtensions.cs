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

            @this.WithDescription(faker.Lorem.Sentences(2));
            @this.WithComment(faker.Lorem.Sentence());
            @this.WithInternalComment(faker.Lorem.Sentence());
            @this.WithInvoiceItemType(faker.Random.ListItem(otherInvoiceItemTypes));
            @this.WithMessage(faker.Lorem.Sentence());
            @this.WithQuantity(1);
            @this.WithAssignedUnitPrice(faker.Random.UInt(1, 100));

            return @this;
        }

        public static SalesInvoiceItemBuilder WithProductItemDefaults(this SalesInvoiceItemBuilder @this)
        {
            var m = @this.Transaction.Database.Services().M;
            var faker = @this.Transaction.Faker();
            var invoiceItemType = @this.Transaction.Extent<InvoiceItemType>().Where(v => v.UniqueId.Equals(InvoiceItemTypes.ProductItemId)).FirstOrDefault();

            var unifiedGoodExtent = @this.Transaction.Extent<UnifiedGood>();
            unifiedGoodExtent.Filter.AddEquals(m.UnifiedGood.InventoryItemKind, new InventoryItemKinds(@this.Transaction).Serialised);
            var serializedProduct = unifiedGoodExtent.First();

            @this.WithDescription(faker.Lorem.Sentences(2));
            @this.WithComment(faker.Lorem.Sentence());
            @this.WithInternalComment(faker.Lorem.Sentence());
            @this.WithInvoiceItemType(invoiceItemType);
            @this.WithProduct(serializedProduct);
            @this.WithSerialisedItem(serializedProduct.SerialisedItems.First);
            @this.WithNextSerialisedItemAvailability(faker.Random.ListItem(@this.Transaction.Extent<SerialisedItemAvailability>()));
            @this.WithMessage(faker.Lorem.Sentence());
            @this.WithQuantity(1);
            @this.WithAssignedUnitPrice(faker.Random.UInt(1, 100));

            return @this;
        }

        public static SalesInvoiceItemBuilder WithPartItemDefaults(this SalesInvoiceItemBuilder @this)
        {
            var m = @this.Transaction.Database.Services().M;
            var faker = @this.Transaction.Faker();
            var invoiceItemType = @this.Transaction.Extent<InvoiceItemType>().Where(v => v.UniqueId.Equals(InvoiceItemTypes.ProductItemId)).FirstOrDefault();

            var unifiedGoodExtent = @this.Transaction.Extent<UnifiedGood>();
            unifiedGoodExtent.Filter.AddEquals(m.UnifiedGood.InventoryItemKind, new InventoryItemKinds(@this.Transaction).Serialised);
            var serializedPart = unifiedGoodExtent.First();

            @this.WithDescription(faker.Lorem.Sentences(2));
            @this.WithComment(faker.Lorem.Sentence());
            @this.WithInternalComment(faker.Lorem.Sentence());
            @this.WithInvoiceItemType(invoiceItemType);
            @this.WithProduct(serializedPart);
            @this.WithSerialisedItem(serializedPart.SerialisedItems.First);
            @this.WithNextSerialisedItemAvailability(faker.Random.ListItem(@this.Transaction.Extent<SerialisedItemAvailability>()));
            @this.WithMessage(faker.Lorem.Sentence());
            @this.WithQuantity(1);
            @this.WithAssignedUnitPrice(faker.Random.UInt(1, 100));

            return @this;
        }
    }
}
