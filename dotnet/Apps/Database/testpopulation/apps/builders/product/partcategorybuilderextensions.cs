// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrandBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class PartCategoryExtensions
    {
        public static PartCategoryBuilder WithDefaults(this PartCategoryBuilder @this)
        {
            var faker = @this.Transaction.Faker();

            @this.WithName(faker.Commerce.Categories(1)[0]);

            return @this;
        }
    }
}
