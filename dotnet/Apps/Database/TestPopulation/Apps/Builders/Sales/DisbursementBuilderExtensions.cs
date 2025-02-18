// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrandBuilderExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class DisbursementBuilderExtensions
    {
        public static DisbursementBuilder WithDefaults(this DisbursementBuilder @this)
        {
            var faker = @this.Transaction.Faker();

            @this
                .WithAmount(faker.Random.Number(10, 100))
                .WithEffectiveDate(faker.Date.Soon());

            return @this;
        }
    }
}
