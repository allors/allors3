// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;

    public class SubcontractorRelationshipDerivation : DomainDerivation
    {
        public SubcontractorRelationshipDerivation(M m) : base(m, new Guid("CF18B16D-C8BD-4097-A959-3AA3929D4A3D")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.SubContractorRelationship.Contractor),
                new ChangedPattern(this.M.SubContractorRelationship.FromDate),
                new ChangedPattern(this.M.SubContractorRelationship.ThroughDate),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SubContractorRelationship>())
            {
                @this.Parties = new[] { @this.Contractor, @this.SubContractor };

                if (@this.ExistContractor && @this.ExistSubContractor)
                {
                    if (@this.FromDate <= @this.Session().Now() && (!@this.ExistThroughDate || @this.ThroughDate >= @this.Session().Now()))
                    {
                        @this.Contractor.AddActiveSubContractor(@this.SubContractor);
                    }

                    if (@this.FromDate > @this.Session().Now() || (@this.ExistThroughDate && @this.ThroughDate < @this.Session().Now()))
                    {
                        @this.Contractor.RemoveActiveSubContractor(@this.SubContractor);
                    }

                    if (@this.SubContractor?.ContactsUserGroup != null)
                    {
                        foreach (OrganisationContactRelationship contactRelationship in @this.SubContractor.OrganisationContactRelationshipsWhereOrganisation)
                        {
                            if (contactRelationship.FromDate <= @this.Session().Now() &&
                                (!contactRelationship.ExistThroughDate || @this.ThroughDate >= @this.Session().Now()))
                            {
                                if (!@this.SubContractor.ContactsUserGroup.Members.Contains(contactRelationship.Contact))
                                {
                                    @this.SubContractor.ContactsUserGroup.AddMember(contactRelationship.Contact);
                                }
                            }
                            else
                            {
                                if (@this.SubContractor.ContactsUserGroup.Members.Contains(contactRelationship.Contact))
                                {
                                    @this.SubContractor.ContactsUserGroup.RemoveMember(contactRelationship.Contact);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

