// <copyright file="PurchaseOrderItemBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>

namespace Allors.Database.Domain.TestPopulation
{
    using System.Linq;

    public static partial class PurchaseOrderItemBuilderExtensions
    {
        public static PurchaseOrderItemBuilder WithSerializedPartDefaults(this PurchaseOrderItemBuilder @this, Part unifiedGood, SerialisedItem serialisedItem, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            @this.WithDescription(faker.Lorem.Sentences(2))
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithInvoiceItemType(new InvoiceItemTypes(@this.Transaction).ProductItem)
                .WithPart(unifiedGood)
                .WithStoredInFacility(faker.Random.ListItem(internalOrganisation.FacilitiesWhereOwner.ToArray()))
                .WithAssignedUnitPrice(faker.Random.UInt(5, 10))
                .WithSerialisedItem(serialisedItem)
                .WithQuantityOrdered(1)
                .WithAssignedDeliveryDate(@this.Transaction.Now().AddDays(5))
                .WithShippingInstruction(faker.Lorem.Sentences(3))
                .WithMessage(faker.Lorem.Sentence());

            return @this;
        }

        public static PurchaseOrderItemBuilder WithNonSerializedPartDefaults(this PurchaseOrderItemBuilder @this, Part nonUnifiedPart, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            @this.WithDescription(faker.Lorem.Sentences(2))
                .WithInvoiceItemType(new InvoiceItemTypes(@this.Transaction).PartItem)
                .WithPart(nonUnifiedPart)
                .WithStoredInFacility(faker.Random.ListItem(internalOrganisation.FacilitiesWhereOwner.ToArray()))
                .WithAssignedUnitPrice(faker.Random.UInt(5, 10))
                .WithQuantityOrdered(faker.Random.UInt(5, 15))
                .WithAssignedDeliveryDate(@this.Transaction.Now().AddDays(5))
                .WithShippingInstruction(faker.Lorem.Sentences(3))
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithMessage(faker.Lorem.Sentence());

            return @this;
        }
    }
}
