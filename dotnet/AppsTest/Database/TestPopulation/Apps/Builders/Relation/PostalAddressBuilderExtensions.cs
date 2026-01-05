// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostalAddressBuilderExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.TestPopulation
{
    using Meta;

    public static partial class PostalAddressBuilderExtensions
    {
        public static PostalAddressBuilder WithDefaults(this PostalAddressBuilder @this)
        {
            var m = @this.Transaction.Database.Services.Get<MetaPopulation>();
            var faker = @this.Transaction.Faker();

            @this.WithAddress1(faker.Address.StreetAddress())
                .WithAddress2(faker.Address.SecondaryAddress())
                .WithAddress3(faker.Address.BuildingNumber())
                .WithPostalCode(faker.Address.ZipCode())
                .WithLocality(faker.Address.City())
                .WithCountry(new Countries(@this.Transaction).FindBy(m.Country.IsoCode, faker.Address.CountryCode()))
                .WithLatitude(faker.Address.Latitude())
                .WithLongitude(faker.Address.Longitude());

            return @this;
        }
    }
}
