// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrandBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class BrandBuilderExtensions
    {
        public static BrandBuilder WithDefaults(this BrandBuilder @this)
        {
            var faker = @this.Transaction.Faker();

            @this.WithName(string.Join(" ", faker.Lorem.Words(3)))
                .WithLogoImage(new MediaBuilder(@this.Transaction).WithInDataUri(faker.Image.DataUri(width: 200, height: 56)).Build())
                .WithModel(new ModelBuilder(@this.Transaction).WithName(faker.Lorem.Word()).Build())
                .WithModel(new ModelBuilder(@this.Transaction).WithName(faker.Lorem.Word()).Build())
                .WithModel(new ModelBuilder(@this.Transaction).WithName(faker.Lorem.Word()).Build());

            return @this;
        }
    }
}
