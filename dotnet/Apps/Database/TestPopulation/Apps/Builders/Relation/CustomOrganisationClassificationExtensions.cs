// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PhoneCommunicationBuilderExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.TestPopulation
{
    using Database.Domain;

    public static partial class CustomOrganisationClassificationExtensions
    {
        public static CustomOrganisationClassificationBuilder WithDefaults(this CustomOrganisationClassificationBuilder @this)
        {
            var faker = @this.Transaction.Faker();

            @this
                .WithName(faker.Name.Prefix());

            return @this;
        }
    }
}
