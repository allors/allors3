// <copyright file="FacilityBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.TestPopulation
{
    using System.Linq;

    public static partial class FacilityBuilderExtensions
    {
        public static FacilityBuilder WithDefaults(this FacilityBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();
            var customer = faker.Random.ListItem(internalOrganisation.ActiveCustomers.ToArray());
            var postalAddress = new PostalAddressBuilder(@this.Transaction).WithDefaults().Build();

            @this.WithName(faker.Name.FullName())
                .WithDescription(faker.Lorem.Sentence())
                .WithLatitude(faker.Address.Latitude())
                .WithLongitude(faker.Address.Longitude())
                .WithOwner(internalOrganisation)
                .WithSquareFootage(faker.Random.Decimal(100, 10000))
                .WithFacilityType(faker.Random.ListItem(@this.Transaction.Extent<FacilityType>()))
                .WithFacilityContactMechanism(customer.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault());

            return @this;
        }
    }
}
