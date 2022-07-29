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

    public class WorkEffortInvoiceItemAssignmentDelegatedAccessRule : Rule
    {
        public WorkEffortInvoiceItemAssignmentDelegatedAccessRule(MetaPopulation m) : base(m, new Guid("2cd91619-c3d8-4c0e-a7e9-56972788ea2f")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkEffortInvoiceItemAssignment.RolePattern(v => v.Assignment),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortInvoiceItemAssignment>())
            {
                @this.DelegatedAccess = @this.Assignment;
            }
        }
    }
}
