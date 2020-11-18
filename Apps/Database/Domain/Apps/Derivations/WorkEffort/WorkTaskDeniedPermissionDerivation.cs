// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Meta;
    using Database.Derivations;

    public class WorkTaskDeniedPermissionDerivation : DomainDerivation
    {
        public WorkTaskDeniedPermissionDerivation(M m) : base(m, new Guid("bbff43c6-24ad-4038-9bbd-d666c41751f6")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(m.WorkTask.TransitionalDeniedPermissions),
            new ChangedPattern(m.WorkTask.CanInvoice),
            new ChangedPattern(m.ServiceEntry.WorkEffort){ Steps =  new IPropertyType[] {m.ServiceEntry.WorkEffort} },
            new ChangedPattern(m.ServiceEntry.ThroughDate){ Steps =  new IPropertyType[] {m.ServiceEntry.WorkEffort} },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkTask>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;

                if (!@this.CanInvoice)
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get((Class)@this.Strategy.Class, @this.Meta.Invoice));
                }
                else
                {
                    @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Session).Get((Class)@this.Strategy.Class, @this.Meta.Invoice));
                }

                var completePermission = new Permissions(@this.Strategy.Session).Get((Class)@this.Strategy.Class, @this.Meta.Complete);

                if (@this.ServiceEntriesWhereWorkEffort.Any(v => !v.ExistThroughDate))
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get((Class)@this.Strategy.Class, @this.Meta.Complete));
                }
                else
                {
                    if (@this.WorkEffortState.IsInProgress)
                    {
                        @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Session).Get((Class)@this.Strategy.Class, @this.Meta.Complete));
                    }
                }
            }
        }
    }
}
