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

    public class RequestForProposalDeriveRequestItemsRule : Rule
    {
        public RequestForProposalDeriveRequestItemsRule(MetaPopulation m) : base(m, new Guid("2eb48653-bed2-4f58-8120-fa1f021b7c0b")) =>
            this.Patterns = new[]
            {
                m.RequestForProposal.RolePattern(v => v.RequestItems)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RequestForProposal>())
            {
                foreach (RequestItem requestItem in @this.RequestItems)
                {
                    requestItem.Sync(@this);
                }
            }
        }
    }
}
