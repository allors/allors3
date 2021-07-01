// <copyright file="RequestForQuoteBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class RequestForQuoteBuilderExtensions
    {
        public static RequestForQuoteBuilder WithSerializedDefaults(this RequestForQuoteBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var requestItem = new RequestItemBuilder(@this.Transaction).WithSerializedDefaults(internalOrganisation).Build();

            @this.WithDescription(faker.Lorem.Sentence())
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithRecipient(internalOrganisation)
                .WithRequestDate(@this.Transaction.Now().AddDays(-2))
                .WithRequiredResponseDate(@this.Transaction.Now().AddDays(2))
                .WithRequestItem(requestItem)
                .WithEmailAddress(faker.Internet.Email())
                .WithTelephoneNumber(faker.Phone.PhoneNumber());

            return @this;
        }

        public static RequestForQuoteBuilder WithNonSerializedDefaults(this RequestForQuoteBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var requestItem = new RequestItemBuilder(@this.Transaction).WithNonSerializedDefaults(internalOrganisation).Build();

            @this.WithDescription(faker.Lorem.Sentence())
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithRecipient(internalOrganisation)
                .WithRequestDate(@this.Transaction.Now().AddDays(-2))
                .WithRequiredResponseDate(@this.Transaction.Now().AddDays(2))
                .WithRequestItem(requestItem)
                .WithEmailAddress(faker.Internet.Email())
                .WithTelephoneNumber(faker.Phone.PhoneNumber());

            return @this;
        }
    }
}
