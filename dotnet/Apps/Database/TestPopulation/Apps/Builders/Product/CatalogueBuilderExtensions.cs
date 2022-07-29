// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrandBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class CatalogueBuilderExtensions
    {
        public static CatalogueBuilder WithDefaults(this CatalogueBuilder @this)
        {
            var faker = @this.Transaction.Faker();

            var scopes = new Scopes(@this.Transaction).Extent().ToArray();

            @this
                .WithCatScope(faker.PickRandom(scopes))
                .WithName(faker.Random.Words(2))
                .WithDescription(faker.Random.Words(5));

            return @this;
        }
    }
}
