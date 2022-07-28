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

    public class WorkEffortInvoiceItemDelegatedAccessRule : Rule
    {
        public WorkEffortInvoiceItemDelegatedAccessRule(MetaPopulation m) : base(m, new Guid("c2c5c00d-622e-443a-81bc-c5e660a3a27d")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkEffortInvoiceItem.AssociationPattern(v => v.WorkEffortInvoiceItemAssignmentWhereWorkEffortInvoiceItem),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortInvoiceItem>())
            {
                @this.DelegatedAccess = @this.WorkEffortInvoiceItemAssignmentWhereWorkEffortInvoiceItem?.Assignment;
            }
        }
    }
}
