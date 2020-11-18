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

    public class ProposalDerivation : DomainDerivation
    {
        public ProposalDerivation(M m) : base(m, new Guid("F51A25BD-3FB7-4539-A541-5F19F124AA9F")) =>
            this.Patterns = new[]
            {
                new ChangedPattern(this.M.Proposal.QuoteItems)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Proposal>())
            {
                foreach (QuoteItem quoteItem in @this.QuoteItems)
                {
                    quoteItem.Sync(@this);
                }
            }
        }
    }
}
