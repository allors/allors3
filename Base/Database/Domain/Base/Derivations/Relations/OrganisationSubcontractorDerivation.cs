// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class OrganisationSubContractorChangedDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("C7C44D1F-11F1-4A48-8385-491089090F44");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new ChangedRolePattern(M.OrganisationContactRelationship.FromDate.RoleType),
            new ChangedRolePattern(M.OrganisationContactRelationship.ThroughDate.RoleType),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var createdSubContractorRelationship = changeSet.Created.Select(v => v.GetObject()).OfType<SubContractorRelationship>();

            changeSet.AssociationsByRoleType.TryGetValue(M.SubContractorRelationship.FromDate.RoleType, out var changedSubContractorRelationship);
            var subContractorRelationshipWhereFromDateChanged = changedSubContractorRelationship?.Select(session.Instantiate).OfType<SubContractorRelationship>();

            foreach (var subContractorRelationship in createdSubContractorRelationship)
            {
                ValidateDate(session, subContractorRelationship);
            }

            if (subContractorRelationshipWhereFromDateChanged?.Any() == true)
            {
                foreach (var subContractorRelationship in subContractorRelationshipWhereFromDateChanged)
                {
                    ValidateDate(session, subContractorRelationship);
                }
            }

            static void ValidateDate(ISession session, SubContractorRelationship subContractorRelationship)
            {
                if (subContractorRelationship.Contractor != null)
                {
                    if (!(subContractorRelationship.FromDate <= session.Now()
                    && (!subContractorRelationship.ExistThroughDate
                    || subContractorRelationship.ThroughDate >= session.Now())))
                    {
                        subContractorRelationship.Contractor
                            .RemoveActiveSubContractor(subContractorRelationship.SubContractor);
                    }
                    else
                    {
                        subContractorRelationship.Contractor
                            .AddActiveSubContractor(subContractorRelationship.SubContractor);
                    }
                }
            }
        }
    }
}
