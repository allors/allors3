// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EmailAddressBuilderExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class RequestForQuoteBuilderExtensions
    {
        public static RequestForQuoteBuilder WithDefaults(this RequestForQuoteBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            @this
                .WithRequestDate(faker.Date.Recent())
                .WithDescription(faker.Random.Words(10))
                .WithRecipient(internalOrganisation)
                .WithRequestNumber(faker.Random.AlphaNumeric(5));

            return @this;
        }
    }
}
