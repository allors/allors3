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
        public class OrganisationRollupCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdOrganisationRollUp = changeSet.Created.Select(session.Instantiate).OfType<OrganisationRollUp>();

                foreach (var organisationRollUp in createdOrganisationRollUp)
                {
                    organisationRollUp.Parties = new Party[] { organisationRollUp.Child, organisationRollUp.Parent };

                    if (!organisationRollUp.ExistParent | !organisationRollUp.ExistChild)
                    {
                        // TODO: Move Delete
                        organisationRollUp.Delete();
                    }
                }
            }
        }

        public static void OrganisationRollUpRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("eeb389b3-1614-4ed1-8eb2-913fb4f5ac45")] = new OrganisationRollupCreationDerivation();
        }
    }
}
