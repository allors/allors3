// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrandBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class BasePriceExtensions
    {
        public static BasePriceBuilder WithDefaults(this BasePriceBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            @this.WithFromDate(faker.Date.Recent())
                 .WithThroughDate(faker.Date.Soon())
                 //TODO: Koen rounding in builders
                 .WithPrice(Math.Round(faker.Random.Decimal(1M, 100M), 2))
                 .WithPricedBy(internalOrganisation);

            return @this;
        }
    }
}
