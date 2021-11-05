// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrandBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

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
