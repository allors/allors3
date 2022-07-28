// <copyright file="OrderTermBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class OrderTermBuilderExtensions
    {
        public static OrderTermBuilder WithDefaults(this OrderTermBuilder @this)
        {
            var faker = @this.Transaction.Faker();

            @this.WithTermValue(faker.Lorem.Sentence())
                .WithTermType(faker.Random.ListItem(@this.Transaction.Extent<OrderTermType>()))
                .WithDescription(faker.Lorem.Sentence());

            return @this;
        }
    }
}
