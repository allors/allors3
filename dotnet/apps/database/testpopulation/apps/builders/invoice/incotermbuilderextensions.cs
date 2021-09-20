// <copyright file="IncoTermBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class IncoTermBuilderExtensions
    {
        public static IncoTermBuilder WithDefaults(this IncoTermBuilder @this)
        {
            var faker = @this.Transaction.Faker();

            @this.WithTermValue(faker.Lorem.Sentence())
                .WithTermType(faker.Random.ListItem(@this.Transaction.Extent<IncoTermType>()))
                .WithDescription(faker.Lorem.Sentence());

            return @this;
        }
    }
}
