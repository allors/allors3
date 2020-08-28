// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;

    public static partial class DabaseExtensions
    {
        public class EngagementCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdEngagements = changeSet.Created.Select(session.Instantiate).OfType<Engagement>();

                foreach(var engagement in createdEngagements)
                {
                    if (!engagement.ExistBillToContactMechanism && engagement.ExistBillToParty)
                    {
                        engagement.BillToContactMechanism = engagement.BillToParty.BillingAddress;
                    }

                    if (!engagement.ExistPlacingContactMechanism && engagement.ExistPlacingParty)
                    {
                        engagement.PlacingContactMechanism = engagement.PlacingParty.OrderAddress;
                    }
                }
            }
        }

        public static void EngagementRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("c007f685-8f17-4341-84b0-1d02df69405d")] = new CatalogueCreationDerivation();
        }
    }
}
