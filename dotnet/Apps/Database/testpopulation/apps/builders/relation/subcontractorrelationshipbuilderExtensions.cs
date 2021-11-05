// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PhoneCommunicationBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.TestPopulation
{
    using Database.Domain;

    public static partial class SubContractorRelationshipBuilderExtensions
    {
        public static SubContractorRelationshipBuilder WithDefaults(this SubContractorRelationshipBuilder @this, Organisation internalOrganisation, Organisation organisation)
        {
            var faker = @this.Transaction.Faker();

            @this.WithFromDate(faker.Date.Recent())
                 .WithThroughDate(faker.Date.Soon(refDate: faker.Date.Recent()
                                        .AddDays(faker.Random.Number(1, 10))))
                 .WithContractor(internalOrganisation)
                 .WithSubContractor(organisation);

            return @this;
        }
    }
}
