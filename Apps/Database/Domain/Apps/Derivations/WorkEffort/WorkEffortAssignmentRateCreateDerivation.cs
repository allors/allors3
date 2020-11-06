// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public class WorkEffortAssignmentRateCreateDerivation : DomainDerivation
    {
        public WorkEffortAssignmentRateCreateDerivation(M m) : base(m, new Guid("0ab43a5f-3ec7-412e-93df-f3556bc3c7db")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.WorkEffortAssignmentRate.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<WorkEffortAssignmentRate>())
            {
                if (!@this.ExistFrequency)
                {
                    @this.Frequency = new TimeFrequencies(@this.Strategy.Session).Hour;
                }
            }
        }
    }
}
