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

    public class WorkEffortInvoiceItemDeniedPermissionRule : Rule
    {
        public WorkEffortInvoiceItemDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("8e46be35-c9ba-4a90-a54e-e07f0ee8fae4")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkTask.RolePattern(v => v.TransitionalRevocations, v => v.WorkEffortInvoiceItemAssignmentsWhereAssignment.ObjectType.WorkEffortInvoiceItem),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortInvoiceItem>())
            {
                var revocation = new Revocations(@this.Strategy.Transaction).WorkEffortInvoiceItemDeleteRevocation;
                if (@this.IsDeletable)
                {
                    @this.RemoveRevocation(revocation);
                }
                else
                {
                    @this.AddRevocation(revocation);
                }
            }
        }
    }
}
