// <copyright file="SalesOrderItemBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>

namespace Allors.Domain.TestPopulation
{
    using System.Linq;

    public static partial class SalesOrderItemBuilderExtensions
    {
        public static SalesOrderItemBuilder WithDefaults(this SalesOrderItemBuilder @this)
        {
            var m = @this.Session.Database.State().M;
            var faker = @this.Session.Faker();
            var invoiceItemTypes = @this.Session.Extent<InvoiceItemType>().ToList();

            var otherInvoiceItemTypes = invoiceItemTypes.Except(
                invoiceItemTypes.Where(v => v.UniqueId.Equals(InvoiceItemTypes.ProductItemId) || v.UniqueId.Equals(InvoiceItemTypes.PartItemId)).ToList())
                .ToList();

            var unifiedGoodExtent = @this.Session.Extent<UnifiedGood>();
            unifiedGoodExtent.Filter.AddEquals(m.UnifiedGood.InventoryItemKind, new InventoryItemKinds(@this.Session).Serialised);
            var serializedProduct = unifiedGoodExtent.First();

            @this.WithDescription(faker.Lorem.Sentences(2));
            @this.WithComment(faker.Lorem.Sentence());
            @this.WithInternalComment(faker.Lorem.Sentence());
            @this.WithInvoiceItemType(faker.Random.ListItem(otherInvoiceItemTypes));
            @this.WithProduct(serializedProduct);
            @this.WithSerialisedItem(serializedProduct.SerialisedItems.First);
            @this.WithNextSerialisedItemAvailability(faker.Random.ListItem(@this.Session.Extent<SerialisedItemAvailability>()));
            @this.WithQuantityOrdered(1);
            @this.WithAssignedUnitPrice(faker.Random.UInt());

            return @this;
        }

        public static SalesOrderItemBuilder WithProductItemDefaults(this SalesOrderItemBuilder @this)
        {
            var m = @this.Session.Database.State().M;
            var faker = @this.Session.Faker();
            var invoiceItemType = @this.Session.Extent<InvoiceItemType>().FirstOrDefault(v => v.UniqueId.Equals(InvoiceItemTypes.ProductItemId));

            var unifiedGoodExtent = @this.Session.Extent<UnifiedGood>();
            unifiedGoodExtent.Filter.AddEquals(m.UnifiedGood.InventoryItemKind, new InventoryItemKinds(@this.Session).Serialised);
            var serializedProduct = unifiedGoodExtent.First();

            @this.WithDescription(faker.Lorem.Sentences(2));
            @this.WithComment(faker.Lorem.Sentence());
            @this.WithInternalComment(faker.Lorem.Sentence());
            @this.WithInvoiceItemType(invoiceItemType);
            @this.WithProduct(serializedProduct);
            @this.WithSerialisedItem(serializedProduct.SerialisedItems.First);
            @this.WithNextSerialisedItemAvailability(faker.Random.ListItem(@this.Session.Extent<SerialisedItemAvailability>()));
            @this.WithQuantityOrdered(1);
            @this.WithAssignedUnitPrice(faker.Random.UInt());

            return @this;
        }

        public static SalesOrderItemBuilder WithNonSerialisedPartItemDefaults(this SalesOrderItemBuilder @this)
        {
            var m = @this.Session.Database.State().M;
            var faker = @this.Session.Faker();
            var invoiceItemType = @this.Session.Extent<InvoiceItemType>().FirstOrDefault(v => v.UniqueId.Equals(InvoiceItemTypes.PartItemId));

            var product = @this.Session.Extent<NonUnifiedGood>().First(v => v.Part.InventoryItemKind.Equals(new InventoryItemKinds(@this.Session).NonSerialised));

            @this.WithDescription(faker.Lorem.Sentences(2));
            @this.WithComment(faker.Lorem.Sentence());
            @this.WithInternalComment(faker.Lorem.Sentence());
            @this.WithInvoiceItemType(invoiceItemType);
            @this.WithProduct(product);
            @this.WithQuantityOrdered(faker.Random.UInt(2, 100));
            @this.WithAssignedUnitPrice(faker.Random.UInt());

            return @this;
        }
    }
}
