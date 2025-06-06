// <copyright file="MiscellaneousChargeBuilderExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class MiscellaneousChargeBuilderExtensions
    {
        public static MiscellaneousChargeBuilder WithAmountDefaults(this MiscellaneousChargeBuilder @this)
        {
            var faker = @this.Transaction.Faker();

            @this.WithAmount(decimal.Round(faker.Random.Decimal(10, 100), 2))
                .WithDescription(faker.Lorem.Sentence());

            return @this;
        }

        public static MiscellaneousChargeBuilder WithPercentageDefaults(this MiscellaneousChargeBuilder @this)
        {
            var faker = @this.Transaction.Faker();

            @this.WithPercentage(decimal.Round(faker.Random.Decimal(1, 5), 2))
                .WithDescription(faker.Lorem.Sentence());

            return @this;
        }
    }
}
