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
    using Derivations.Rules;

    public class WorkTaskDeniedPermissionRule : Rule
    {
        public WorkTaskDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("bbff43c6-24ad-4038-9bbd-d666c41751f6")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkTask.RolePattern(v => v.TransitionalDeniedPermissions),
            m.WorkTask.RolePattern(v => v.CanInvoice),
            m.WorkTask.RolePattern(v => v.Customer),
            m.WorkTask.RolePattern(v => v.ExecutedBy),
            m.WorkEffort.AssociationPattern(v => v.ServiceEntriesWhereWorkEffort),
            m.ServiceEntry.RolePattern(v => v.ThroughDate, v => v.WorkEffort.WorkEffort.ServiceEntriesWhereWorkEffort.ServiceEntry.WorkEffort),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkTask>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;

                if (!@this.CanInvoice)
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get((Class)@this.Strategy.Class, @this.Meta.Invoice));
                }
                else
                {
                    @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Transaction).Get((Class)@this.Strategy.Class, @this.Meta.Invoice));
                }

                var completePermission = new Permissions(@this.Strategy.Transaction).Get((Class)@this.Strategy.Class, @this.Meta.Complete);

                if (@this.ServiceEntriesWhereWorkEffort.Any(v => !v.ExistThroughDate))
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get((Class)@this.Strategy.Class, @this.Meta.Complete));
                }
                else
                {
                    if (@this.WorkEffortState.IsInProgress)
                    {
                        @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Transaction).Get((Class)@this.Strategy.Class, @this.Meta.Complete));
                    }
                }

                if (@this.WorkEffortState.IsFinished)
                {
                    if (@this.ExecutedBy.Equals(@this.Customer))
                    {
                        @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Transaction).Get((Class)@this.Strategy.Class, @this.Meta.Revise));
                    }
                }
            }
        }
    }
}
