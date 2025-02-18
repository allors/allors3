// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrandBuilderExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Allors.Database.Domain.TestPopulation
{
    public static partial class SerialisedItemCharacteristicTypeExtensions
    {
        public static SerialisedItemCharacteristicTypeBuilder WithDefaults(this SerialisedItemCharacteristicTypeBuilder @this)
        {
            var faker = @this.Transaction.Faker();

            var unitOfMeasures = new UnitsOfMeasure(@this.Transaction).Extent().ToArray();

            @this.WithName(faker.Random.Word())
                 .WithUnitOfMeasure(faker.PickRandom(unitOfMeasures))
                 .WithIsActive(true)
                 .WithIsPublic(false);

            return @this;
        }
    }
}
