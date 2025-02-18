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

    public class WorkTaskDeniedPermissionRule : Rule
    {
        public WorkTaskDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("bbff43c6-24ad-4038-9bbd-d666c41751f6")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkTask.RolePattern(v => v.TransitionalRevocations),
            m.WorkTask.RolePattern(v => v.CanInvoice),
            m.WorkTask.RolePattern(v => v.Customer),
            m.WorkTask.RolePattern(v => v.ExecutedBy),
            m.WorkEffort.AssociationPattern(v => v.ServiceEntriesWhereWorkEffort),
            m.ServiceEntry.RolePattern(v => v.ThroughDate, v => v.WorkEffort.ObjectType.ServiceEntriesWhereWorkEffort.ObjectType.WorkEffort),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkTask>())
            {
                @this.Revocations = @this.TransitionalRevocations;

                var completeRevocation = new Revocations(@this.Strategy.Transaction).WorkTaskCompleteRevocation;
                var invoiceRevocation = new Revocations(@this.Strategy.Transaction).WorkTaskInvoiceRevocation;
                var reviseRevocation = new Revocations(@this.Strategy.Transaction).WorkTaskReviseRevocation;

                if (!@this.CanInvoice)
                {
                    @this.AddRevocation(invoiceRevocation);
                }
                else
                {
                    @this.RemoveRevocation(invoiceRevocation);
                }

                if (@this.ServiceEntriesWhereWorkEffort.Any(v => !v.ExistThroughDate))
                {
                    @this.AddRevocation(completeRevocation);
                }
                else if (@this.WorkEffortState.IsInProgress)
                {
                    @this.RemoveRevocation(completeRevocation);
                }

                if (@this.WorkEffortState.IsFinished && !@this.ExecutedBy.Equals(@this.Customer))
                {
                    @this.AddRevocation(reviseRevocation);
                }
            }
        }
    }
}
