// <copyright file="SalesInvoiceItemBuilderExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>

namespace Allors.Database.Domain.TestPopulation
{
    using System.Linq;
    using Meta;
    using InvoiceItemType = InvoiceItemType;
    using SerialisedItemAvailability = SerialisedItemAvailability;
    using UnifiedGood = UnifiedGood;

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
                // Floor 2, not 1: several invoice tests form a partial payment as `TotalIncVat - 1`,
                // which must stay > 0. A unit price of 1 makes it 0, so the derivation yields NotPaid
                // instead of PartiallyPaid and flakes those tests (this Faker is unseeded/random).
                .WithAssignedUnitPrice(faker.Random.UInt(2, 100));

            return @this;
        }

        public static SalesInvoiceItemBuilder WithProductItemDefaults(this SalesInvoiceItemBuilder @this)
        {
            var m = @this.Transaction.Database.Services.Get<MetaPopulation>();
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
                .WithAssignedUnitPrice(faker.Random.UInt(2, 100));

            return @this;
        }

        public static SalesInvoiceItemBuilder WithProductItemDefaults(this SalesInvoiceItemBuilder @this, UnifiedGood unifiedGood)
        {
            var m = @this.Transaction.Database.Services.Get<MetaPopulation>();
            var faker = @this.Transaction.Faker();
            var invoiceItemType = @this.Transaction.Extent<InvoiceItemType>().FirstOrDefault(v => v.UniqueId.Equals(InvoiceItemTypes.ProductItemId));

            @this.WithDescription(faker.Lorem.Sentences(2))
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithInvoiceItemType(invoiceItemType)
                .WithProduct(unifiedGood)
                .WithSerialisedItem(unifiedGood.SerialisedItems.FirstOrDefault())
                .WithNextSerialisedItemAvailability(faker.Random.ListItem(@this.Transaction.Extent<SerialisedItemAvailability>()))
                .WithMessage(faker.Lorem.Sentence())
                .WithQuantity(1)
                .WithAssignedUnitPrice(faker.Random.UInt(2, 100));

            return @this;
        }


        public static SalesInvoiceItemBuilder WithPartItemDefaults(this SalesInvoiceItemBuilder @this)
        {
            var m = @this.Transaction.Database.Services.Get<MetaPopulation>();
            var faker = @this.Transaction.Faker();
            var invoiceItemType = @this.Transaction.Extent<InvoiceItemType>().FirstOrDefault(v => v.UniqueId.Equals(InvoiceItemTypes.PartItemId));

            var unifiedGoodExtent = @this.Transaction.Extent<UnifiedGood>();
            unifiedGoodExtent.Filter.AddEquals(m.UnifiedGood.InventoryItemKind, new InventoryItemKinds(@this.Transaction).Serialised);
            var serializedPart = unifiedGoodExtent.First();

            @this.WithDescription(faker.Lorem.Sentences(2))
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithInvoiceItemType(invoiceItemType)
                .WithPart(serializedPart)
                .WithMessage(faker.Lorem.Sentence())
                .WithQuantity(1)
                .WithAssignedUnitPrice(faker.Random.UInt(2, 100));

            return @this;
        }
    }
}
