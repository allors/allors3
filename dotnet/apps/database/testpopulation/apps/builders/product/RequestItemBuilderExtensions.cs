// <copyright file="RequestItemBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.TestPopulation
{
    using System.Linq;

    public static partial class RequestItemBuilderExtensions
    {
        public static RequestItemBuilder WithSerializedDefaults(this RequestItemBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var finishedGood = new UnifiedGoodBuilder(@this.Transaction).WithSerialisedDefaults(internalOrganisation).Build();

            @this.WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithMaximumAllowedPrice(faker.Random.UInt())
                .WithQuantity(1)
                .WithProduct(finishedGood)
                .WithRequiredByDate(@this.Transaction.Now().AddDays(7))
                .WithSerialisedItem(finishedGood.SerialisedItems.FirstOrDefault());

            return @this;
        }

        public static RequestItemBuilder WithNonSerializedDefaults(this RequestItemBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var finishedGood = new UnifiedGoodBuilder(@this.Transaction).WithNonSerialisedDefaults(internalOrganisation).Build();

            @this.WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithMaximumAllowedPrice(faker.Random.UInt())
                .WithQuantity(faker.Random.UShort())
                .WithProduct(finishedGood)
                .WithRequiredByDate(@this.Transaction.Now().AddDays(7));

            return @this;
        }
    }
}
