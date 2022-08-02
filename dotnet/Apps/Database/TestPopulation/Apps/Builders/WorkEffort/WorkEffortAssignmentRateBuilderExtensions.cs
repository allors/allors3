// <copyright file="WorkTaskBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>

using System;

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class WorkEffortAssignmentRateBuilderExtensions
    {
        public static WorkEffortAssignmentRateBuilder WithDefaults(this WorkEffortAssignmentRateBuilder @this)
        {
            var faker = @this.Transaction.Faker();

            var rateTypes = new RateTypes(@this.Transaction).Extent().ToArray();

            @this.WithRateType(faker.PickRandom(rateTypes))
                 .WithRate(Math.Round(faker.Random.Decimal(1M, 20M)))
                 .WithFrequency(new TimeFrequencies(@this.Transaction).Hour);

            return @this;
        }
    }
}
