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

    public class SalesInvoiceItemDeniedPermissionRule : Rule
    {
        public SalesInvoiceItemDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("c7ede487-9920-4e47-bb72-1b8f27bdd552")) =>
            this.Patterns = new Pattern[]
        {
            m.SalesInvoiceItem.RolePattern(v => v.TransitionalRevocations),
            m.WorkTask.RolePattern(v => v.TransitionalRevocations, v => v.WorkEffortSalesInvoiceItemAssignmentsWhereAssignment.WorkEffortSalesInvoiceItemAssignment.SalesInvoiceItem),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoiceItem>())
            {
                @this.Revocations = @this.TransitionalRevocations;

                var revocation = new Revocations(@this.Strategy.Transaction).SalesInvoiceItemDeleteRevocation;
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
