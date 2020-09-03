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
            new CreatedPattern(M.SubContractorRelationship.Class),
            new ChangedConcreteRolePattern(M.SubContractorRelationship.FromDate),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;

            foreach (var subContractorRelationship in matches.Cast<SubContractorRelationship>())
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
