// <copyright file="ProductQuoteBuilderExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

            var customer = faker.Random.ListItem(internalOrganisation.ActiveCustomers.ToArray());

            @this.WithContactPerson(customer.CurrentContacts.FirstOrDefault())
                .WithDescription(faker.Lorem.Sentence())
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithIssuer(internalOrganisation)
                .WithIssueDate(@this.Transaction.Now().AddDays(-2))
                .WithValidFromDate(@this.Transaction.Now().AddDays(-2))
                .WithValidThroughDate(@this.Transaction.Now().AddDays(2))
                .WithReceiver(customer)
                .WithFullfillContactMechanism(customer.GeneralCorrespondence);

            return @this;
        }

        // TODO: Martien
        public static ProductQuoteBuilder WithSerializedDefaults(this ProductQuoteBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var quoteItem = new QuoteItemBuilder(@this.Transaction).WithSerializedDefaults(internalOrganisation).Build();
            var customer = faker.Random.ListItem(internalOrganisation.ActiveCustomers.ToArray());

            @this.WithContactPerson(customer.CurrentContacts.FirstOrDefault());
            @this.WithFullfillContactMechanism(customer.CurrentPartyContactMechanisms.Select(v => v.ContactMechanism).FirstOrDefault());
            @this.WithDescription(faker.Lorem.Sentence());
            @this.WithComment(faker.Lorem.Sentence());
            @this.WithInternalComment(faker.Lorem.Sentence());
            @this.WithIssuer(internalOrganisation);
            @this.WithIssueDate(@this.Transaction.Now().AddDays(-2));
            @this.WithValidFromDate(@this.Transaction.Now().AddDays(-2));
            @this.WithValidThroughDate(@this.Transaction.Now().AddDays(2));
            @this.WithReceiver(customer);
            @this.WithQuoteItem(quoteItem);

            return @this;
        }
    }
}
