// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public class DeliverableTurnoverDerivation : DomainDerivation
    {
        public DeliverableTurnoverDerivation(M m) : base(m, new Guid("8685bf98-9ecc-494b-89f5-91da6d66acb8")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.DeliverableTurnover.WorkEffort),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<DeliverableTurnover>())
            {
                validation.AssertAtLeastOne(@this, @this.M.DeliverableTurnover.WorkEffort, @this.M.DeliverableTurnover.EngagementItem);
            }
        }
    }
}
