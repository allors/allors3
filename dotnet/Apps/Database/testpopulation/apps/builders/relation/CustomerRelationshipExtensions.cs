// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PhoneCommunicationBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.TestPopulation
{
    using System;
    using System.Linq;
    using Database.Domain;

    public static partial class CustomerRelationshipExtensions
    {
        public static CustomerRelationshipBuilder WithDefaults(this CustomerRelationshipBuilder @this, Organisation internalOrganisation)
        {
            var faker = @this.Transaction.Faker();

            var customer = new Organisations(@this.Transaction).Extent().ToArray().Except(new[] { internalOrganisation }).FirstOrDefault();

            @this
                .WithFromDate(faker.Date.Recent().Date)
                .WithThroughDate(faker.Date.Soon().Date)
                .WithInternalOrganisation(internalOrganisation)
                .WithCustomer(customer);


            return @this;
        }
    }
}
