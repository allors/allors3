// <copyright file="ProductQuoteBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.TestPopulation
{
    using System.Linq;

    public static partial class ProductQuoteBuilderExtensions
    {
        public static ProductQuoteBuilder WithDefaults(this ProductQuoteBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var customer = faker.Random.ListItem(internalOrganisation.ActiveCustomers);

            @this.WithContactPerson(customer.CurrentContacts.FirstOrDefault());
            @this.WithDescription(faker.Lorem.Sentence());
            @this.WithComment(faker.Lorem.Sentence());
            @this.WithInternalComment(faker.Lorem.Sentence());
            @this.WithIssuer(internalOrganisation);
            @this.WithIssueDate(@this.Transaction.Now().AddDays(-2));
            @this.WithValidFromDate(@this.Transaction.Now().AddDays(-2));
            @this.WithValidThroughDate(@this.Transaction.Now().AddDays(2));
            @this.WithRequiredResponseDate(@this.Transaction.Now().AddDays(2));
            @this.WithReceiver(customer);
            @this.WithFullfillContactMechanism(customer.GeneralCorrespondence);

            return @this;
        }
    }
}
