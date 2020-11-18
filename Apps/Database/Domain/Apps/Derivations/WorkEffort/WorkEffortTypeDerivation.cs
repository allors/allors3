// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Domain.Derivations;
    using Allors.Database.Meta;
    using Database.Derivations;

    public class WorkEffortTypeDerivation : DomainDerivation
    {
        public WorkEffortTypeDerivation(M m) : base(m, new Guid("765b3252-66a0-4358-8f7b-1765a1d8cf53")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.WorkEffortType.Description),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortType>())
            {
                validation.AssertExists(@this, @this.M.WorkEffortType.Description);

                @this.CurrentWorkEffortPartStandards = @this.WorkEffortPartStandards
                    .Where(v => v.FromDate <= @this.Session().Now() && (!v.ExistThroughDate || v.ThroughDate >= @this.Session().Now()))
                    .ToArray();

                @this.InactiveWorkEffortPartStandards = @this.WorkEffortPartStandards
                    .Except(@this.CurrentWorkEffortPartStandards)
                    .ToArray();
            }
        }
    }
}
