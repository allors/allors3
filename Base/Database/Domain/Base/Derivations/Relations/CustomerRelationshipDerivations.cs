// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class CustomerRelationshipCreationCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdCustomerRelationship = changeSet.Created.Select(session.Instantiate).OfType<CustomerRelationship>();

                foreach(var customerRelationship in createdCustomerRelationship)
                {
                    customerRelationship.Parties = new Party[] { customerRelationship.Customer, customerRelationship.InternalOrganisation };
                }
            }
        }

        public static void CustomerRelationshipRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("69a6b14b-b944-4fa1-a642-7be21e652f88")] = new CustomerRelationshipCreationCreationDerivation();
        }
    }
}
