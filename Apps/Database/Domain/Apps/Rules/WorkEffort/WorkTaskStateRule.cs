
// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class WorkTaskStateRule : Rule
    {
        public WorkTaskStateRule(MetaPopulation m) : base(m, new Guid("cf686b8c-3a64-43b2-a525-cff9d818cdad")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkTask.RolePattern(v => v.ActualStart),
            m.WorkTask.RolePattern(v => v.ExecutedBy),
            m.WorkTask.RolePattern(v => v.WorkEffortState),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkTask>())
            {
                if (@this.ExistActualStart && @this.WorkEffortState.IsCreated)
                {
                    @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Transaction).InProgress;
                }

                if (@this.WorkEffortState.IsFinished && @this.CanInvoice)
                {
                    @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Transaction).Completed;
                }
            }
        }
    }
}
