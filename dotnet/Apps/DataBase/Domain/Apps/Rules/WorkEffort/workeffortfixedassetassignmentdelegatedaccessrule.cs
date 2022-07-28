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
    using Derivations.Rules;

    public class WorkEffortFixedAssetAssignmentDelegatedAccessRule : Rule
    {
        public WorkEffortFixedAssetAssignmentDelegatedAccessRule(MetaPopulation m) : base(m, new Guid("4b364a10-4923-4a23-994a-1a3ab9d15d09")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkEffortFixedAssetAssignment.RolePattern(v => v.Assignment),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortFixedAssetAssignment>())
            {
                @this.DelegatedAccess = @this.Assignment;
            }
        }
    }
}
