// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Allors.Workspace.Domain;

    public static partial class DabaseExtensions
    {
        public class SubcontractorRelationshipCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdSubcontractorRelationship = changeSet.Created.Select(v=>v.GetObject()).OfType<SubContractorRelationship>();

                foreach (var subcontractorRelationship in createdSubcontractorRelationship)
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
        public static void subcontractorRelationshipRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("0ee84c31-f712-4c9f-9d05-620009a685f3")] = new EmploymentCreationDerivation();
        }
    }
}

