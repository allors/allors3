// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class SubcontractorRelationshipDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("CF18B16D-C8BD-4097-A959-3AA3929D4A3D");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.SubContractorRelationship.Class),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var subcontractorRelationship in matches.Cast<SubContractorRelationship>())
            {
                subcontractorRelationship.Parties = new[] { subcontractorRelationship.Contractor, subcontractorRelationship.SubContractor };

                if (subcontractorRelationship.ExistContractor && subcontractorRelationship.ExistSubContractor)
                {
                    if (subcontractorRelationship.FromDate <= subcontractorRelationship.Session().Now() && (!subcontractorRelationship.ExistThroughDate || subcontractorRelationship.ThroughDate >= subcontractorRelationship.Session().Now()))
                    {
                        subcontractorRelationship.Contractor.AddActiveSubContractor(subcontractorRelationship.SubContractor);
                    }

                    if (subcontractorRelationship.FromDate > subcontractorRelationship.Session().Now() || (subcontractorRelationship.ExistThroughDate && subcontractorRelationship.ThroughDate < subcontractorRelationship.Session().Now()))
                    {
                        subcontractorRelationship.Contractor.RemoveActiveSubContractor(subcontractorRelationship.SubContractor);
                    }

                    if (subcontractorRelationship.SubContractor?.ContactsUserGroup != null)
                    {
                        foreach (OrganisationContactRelationship contactRelationship in subcontractorRelationship.SubContractor.OrganisationContactRelationshipsWhereOrganisation)
                        {
                            if (contactRelationship.FromDate <= subcontractorRelationship.Session().Now() &&
                                (!contactRelationship.ExistThroughDate || subcontractorRelationship.ThroughDate >= subcontractorRelationship.Session().Now()))
                            {
                                if (!subcontractorRelationship.SubContractor.ContactsUserGroup.Members.Contains(contactRelationship.Contact))
                                {
                                    subcontractorRelationship.SubContractor.ContactsUserGroup.AddMember(contactRelationship.Contact);
                                }
                            }
                            else
                            {
                                if (subcontractorRelationship.SubContractor.ContactsUserGroup.Members.Contains(contactRelationship.Contact))
                                {
                                    subcontractorRelationship.SubContractor.ContactsUserGroup.RemoveMember(contactRelationship.Contact);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

