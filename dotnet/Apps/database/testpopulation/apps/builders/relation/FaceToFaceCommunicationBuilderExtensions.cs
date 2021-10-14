// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FaceToFaceCommunicationBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.TestPopulation
{
    using System;
    using System.Linq;
    using Database.Domain;

    public static partial class FaceToFaceCommunicationBuilderExtensions
    {
        public static FaceToFaceCommunicationBuilder WithDefaults(this FaceToFaceCommunicationBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var administrator = (Person)new UserGroups(@this.Transaction).Administrators.Members.FirstOrDefault();

            @this.WithDescription(faker.Lorem.Sentence(20));
            @this.WithSubject(faker.Lorem.Sentence(5));
            @this.WithEventPurpose(new CommunicationEventPurposes(@this.Transaction).Meeting);
            @this.WithFromParty(internalOrganisation.ActiveEmployees.FirstOrDefault());
            @this.WithToParty(internalOrganisation.ActiveCustomers.FirstOrDefault());
            @this.WithOwner(administrator);
            @this.WithActualStart(DateTime.UtcNow);

            return @this;
        }
    }
}
