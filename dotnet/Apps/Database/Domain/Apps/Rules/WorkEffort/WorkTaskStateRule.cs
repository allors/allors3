
// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

    public class WorkTaskStateRule : Rule
    {
        public WorkTaskStateRule(MetaPopulation m) : base(m, new Guid("cf686b8c-3a64-43b2-a525-cff9d818cdad")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkTask.RolePattern(v => v.DerivationTrigger),
            m.WorkTask.RolePattern(v => v.CanInvoice),
            m.WorkTask.RolePattern(v => v.ActualStart),
            m.WorkTask.RolePattern(v => v.ExecutedBy),
            m.WorkTask.RolePattern(v => v.WorkEffortState),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkTask>())
            {
                @this.DeriveWorkTaskState(validation);
            }
        }
    }

    public static class WorkTaskStateRuleExtensions
    {
        public static void DeriveWorkTaskState(this WorkTask @this, IValidation validation)
        {
            if (@this.ExistActualStart && @this.WorkEffortState.IsCreated)
            {
                @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Transaction).InProgress;
            }

            if (!@this.ExistActualStart && @this.WorkEffortState.IsInProgress)
            {
                @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Transaction).Created;
            }
        }
    }
}
