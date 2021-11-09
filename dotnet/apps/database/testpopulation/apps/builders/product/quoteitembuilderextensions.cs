// <copyright file="QuoteItemBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.TestPopulation
{
    using System.Linq;

    public static partial class QuoteItemBuilderExtensions
    {
        public static QuoteItemBuilder WithSerializedDefaults(this QuoteItemBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var serializedProduct = new UnifiedGoodBuilder(@this.Transaction).WithSerialisedDefaults(internalOrganisation).Build();

            @this.WithDetails(faker.Lorem.Sentence())
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithEstimatedDeliveryDate(@this.Transaction.Now().AddDays(5))
                .WithInvoiceItemType(new InvoiceItemTypes(@this.Transaction).ProductItem)
                .WithSaleKind(faker.Random.ListItem(@this.Transaction.Extent<SaleKind>()))
                .WithRentalType(faker.Random.ListItem(@this.Transaction.Extent<RentalType>()))
                .WithProduct(serializedProduct)
                .WithSerialisedItem(serializedProduct.SerialisedItems.FirstOrDefault())
                .WithUnitOfMeasure(new UnitsOfMeasure(@this.Transaction).Piece)
                .WithAssignedUnitPrice(faker.Random.UInt())
                .WithQuantity(1);
            return @this;
        }
    }
}
