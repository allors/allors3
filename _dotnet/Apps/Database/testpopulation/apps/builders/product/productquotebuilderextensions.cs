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

            var customer = faker.Random.ListItem(internalOrganisation.ActiveCustomers.ToArray());

            @this.WithContactPerson(customer.CurrentContacts.FirstOrDefault())
                .WithDescription(faker.Lorem.Sentence())
                .WithComment(faker.Lorem.Sentence())
                .WithInternalComment(faker.Lorem.Sentence())
                .WithIssuer(internalOrganisation)
                .WithIssueDate(@this.Transaction.Now().AddDays(-2))
                .WithValidFromDate(@this.Transaction.Now().AddDays(-2))
                .WithValidThroughDate(@this.Transaction.Now().AddDays(2))
                .WithRequiredResponseDate(@this.Transaction.Now().AddDays(2))
                .WithReceiver(customer)
                .WithFullfillContactMechanism(customer.GeneralCorrespondence);

            return @this;
        }
    }
}
