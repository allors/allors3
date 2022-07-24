// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManufacturerIdentificationExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class ManufacturerIdentificationExtensions
    {
        public static ManufacturerIdentificationBuilder WithDefaults(this ManufacturerIdentificationBuilder @this)
        {
            var faker = @this.Transaction.Faker();
            @this.WithIdentification(faker.Random.AlphaNumeric(9))
                .WithProductIdentificationType(new ProductIdentificationTypes(@this.Transaction).Manufacturer);
            return @this;
        }
    }
}
