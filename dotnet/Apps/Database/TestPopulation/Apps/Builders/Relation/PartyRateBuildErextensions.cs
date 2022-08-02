// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EmailAddressBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class partyratebuilderextensions
    {
        public static PartyRateBuilder WithDefaults(this PartyRateBuilder @this)
        {
            var faker = @this.Transaction.Faker();

            @this.WithRateType(new RateTypes(@this.Transaction).WeekendRate)
                .WithFromDate(faker.Date.Recent())
                .WithThroughDate(faker.Date.Soon())
                .WithRate(Math.Round(faker.Random.Decimal(1M, 100M), 2))
                .WithFrequency(new TimeFrequencies(@this.Transaction).Hour);

            return @this;
        }
    }
}
