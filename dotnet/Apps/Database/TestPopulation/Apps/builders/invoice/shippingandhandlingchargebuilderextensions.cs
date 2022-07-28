// <copyright file="ShippingAndHandlingChargeBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class ShippingAndHandlingChargeBuilderExtensions
    {
        public static ShippingAndHandlingChargeBuilder WithAmountDefaults(this ShippingAndHandlingChargeBuilder @this)
        {
            var faker = @this.Transaction.Faker();

            @this.WithAmount(decimal.Round(faker.Random.Decimal(10, 100), 2))
                .WithDescription(faker.Lorem.Sentence());

            return @this;
        }

        public static ShippingAndHandlingChargeBuilder WithPercentageDefaults(this ShippingAndHandlingChargeBuilder @this)
        {
            var faker = @this.Transaction.Faker();

            @this.WithPercentage(decimal.Round(faker.Random.Decimal(1, 5), 2))
                .WithDescription(faker.Lorem.Sentence());

            return @this;
        }
    }
}
