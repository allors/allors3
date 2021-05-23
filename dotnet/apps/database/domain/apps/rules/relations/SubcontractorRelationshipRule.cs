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

    public class SubcontractorRelationshipRule : Rule
    {
        public SubcontractorRelationshipRule(MetaPopulation m) : base(m, new Guid("CF18B16D-C8BD-4097-A959-3AA3929D4A3D")) =>
            this.Patterns = new Pattern[]
            {
                m.SubContractorRelationship.RolePattern(v => v.Contractor),
                m.SubContractorRelationship.RolePattern(v => v.SubContractor),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SubContractorRelationship>())
            {
                @this.Parties = new[] { @this.Contractor, @this.SubContractor };
            }
        }
    }
}

