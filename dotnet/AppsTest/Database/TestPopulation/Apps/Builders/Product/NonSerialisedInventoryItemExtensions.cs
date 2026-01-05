// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrandBuilderExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class NonSerialisedInventoryItemExtensions
    {
        public static NonSerialisedInventoryItemBuilder WithDefaults(this NonSerialisedInventoryItemBuilder @this, Part part)
        {
            var faker = @this.Transaction.Faker();

            @this
                .WithPart(part)
                .WithFacility(part.DefaultFacility)
                .WithUnitOfMeasure(part.UnitOfMeasure)
                .WithPartLocation(faker.Random.Word());

            return @this;
        }
    }
}
