// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrandBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class OrganisationContactRelationshipBuilderExtensions
    {
        public static OrganisationContactRelationshipBuilder WithDefaults(this OrganisationContactRelationshipBuilder @this, Organisation organisation)
        {
            var faker = @this.Transaction.Faker();

            var contact = organisation.CurrentContacts.First();

            @this
                .WithOrganisation(organisation)
                .WithFromDate(faker.Date.Recent().Date)
                .WithContact(contact);

            return @this;
        }
    }
}
