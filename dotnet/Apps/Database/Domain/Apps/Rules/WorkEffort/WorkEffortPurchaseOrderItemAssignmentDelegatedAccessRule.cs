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

    public class WorkEffortPurchaseOrderItemAssignmentDelegatedAccessRule : Rule
    {
        public WorkEffortPurchaseOrderItemAssignmentDelegatedAccessRule(MetaPopulation m) : base(m, new Guid("52353306-2a73-4949-9691-83d1362b50c1")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkEffortPurchaseOrderItemAssignment.RolePattern(v => v.Assignment),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortPurchaseOrderItemAssignment>())
            {
                if (@this.ExistAssignment)
                {
                    @this.DelegatedAccess = @this.Assignment;
                }
            }
        }
    }
}
