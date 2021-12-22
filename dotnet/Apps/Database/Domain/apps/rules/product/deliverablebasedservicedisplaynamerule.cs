// <copyright file="DeliverableBasedServiceDisplayNameRule.cs" company="Allors bvba">
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
    using Derivations.Rules;

    public class DeliverableBasedServiceDisplayNameRule : Rule
    {
        public DeliverableBasedServiceDisplayNameRule(MetaPopulation m) : base(m, new Guid("739c47e6-b7a7-4309-9b54-021851b52102")) =>
            this.Patterns = new Pattern[]
            {
                m.DeliverableBasedService.RolePattern(v => v.Name),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<DeliverableBasedService>())
            {
                @this.DisplayName = @this.Name ?? "N/A";
            }
        }
    }
}
