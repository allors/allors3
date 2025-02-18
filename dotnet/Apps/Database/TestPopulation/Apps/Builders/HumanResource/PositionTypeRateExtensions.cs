// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EmailAddressBuilderExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class positiontyperateextensions
    {
        public static PositionTypeRateBuilder WithDefaults(this PositionTypeRateBuilder @this)
        {
            var faker = @this.Transaction.Faker();

            @this.WithRateType(new RateTypes(@this.Transaction).WeekendRate)
                .WithFromDate(faker.Date.Recent())
                .WithRate(Math.Round(faker.Random.Decimal(1M, 100M), 2))
                .WithFrequency(new TimeFrequencies(@this.Transaction).Hour);

            return @this;
        }
    }
}
