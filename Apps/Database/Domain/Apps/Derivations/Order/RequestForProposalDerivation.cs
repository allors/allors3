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

    public class RequestForProposalDerivation : DomainDerivation
    {
        public RequestForProposalDerivation(M m) : base(m, new Guid("E2C5250C-5C18-4720-BBFE-859AC31D8D49")) =>
            this.Patterns = new[]
            {
                new ChangedPattern(this.M.RequestForProposal.RequestItems)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
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
