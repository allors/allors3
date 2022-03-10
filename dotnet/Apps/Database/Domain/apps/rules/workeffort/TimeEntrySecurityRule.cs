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
    using Derivations.Rules;
    using Meta;

    public class TimeEntrySecurityRule : Rule
    {
        public TimeEntrySecurityRule(MetaPopulation m) : base(m, new Guid("8733329d-2cd7-4555-bab1-ef4b1e21b540")) =>
            this.Patterns = new Pattern[]
        {
            m.TimeEntry.RolePattern(v => v.WorkEffort),
            m.TimeEntry.RolePattern(v => v.Worker),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<TimeEntry>())
            {
                @this.DelegatedAccess = @this.WorkEffort;
                @this.AddSecurityToken(@this.Worker?.OwnerSecurityToken);
            }
        }
    }
}
