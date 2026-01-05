// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EmailCommunicationBuilderExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.TestPopulation
{
    using System;
    using System.Linq;
    using Database.Domain;

    public static partial class EmailCommunicationBuilderExtensions
    {
        public static EmailCommunicationBuilder WithDefaults(this EmailCommunicationBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var administrator = (Person)new UserGroups(@this.Transaction).Administrators.Members.FirstOrDefault();

            @this.WithDescription(faker.Lorem.Sentence(20));
            @this.WithSubject(faker.Lorem.Sentence(5));
            @this.WithFromParty(internalOrganisation.ActiveEmployees.FirstOrDefault());
            @this.WithToParty(internalOrganisation.ActiveCustomers.FirstOrDefault());
            @this.WithFromEmail(internalOrganisation.ActiveEmployees.First().GeneralEmail);
            @this.WithToEmail(internalOrganisation.ActiveCustomers.First().GeneralEmail);
            @this.WithEventPurpose(new CommunicationEventPurposes(@this.Transaction).Meeting);
            @this.WithOwner(administrator);
            @this.WithActualStart(DateTime.UtcNow);

            return @this;
        }
    }
}
