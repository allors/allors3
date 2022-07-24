// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EmailAddressBuilderExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.TestPopulation
{
    public static partial class partycontactmechanismextensions
    {
        public static PartyContactMechanismBuilder WithDefaults(this PartyContactMechanismBuilder @this, ContactMechanismPurpose contactMechanismPurpose, ContactMechanism contactMechanism)
        {
            var faker = @this.Transaction.Faker();

            @this.WithFromDate(faker.Date.Recent())
                .WithThroughDate(faker.Date.Soon())
                .WithContactPurpose(contactMechanismPurpose)
                .WithContactMechanism(contactMechanism);

            return @this;
        }
    }
}
