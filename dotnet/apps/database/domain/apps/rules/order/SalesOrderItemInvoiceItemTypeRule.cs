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

    public class SalesOrderItemInvoiceItemTypeRule : Rule
    {
        public SalesOrderItemInvoiceItemTypeRule(MetaPopulation m) : base(m, new Guid("0bd194e2-b72b-4502-8609-a53c38cc2cb8")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrderItem.RolePattern(v => v.Product),
                m.SalesOrderItem.RolePattern(v => v.InvoiceItemType),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrderItem>())
            {

                if (@this.ExistProduct && !@this.ExistInvoiceItemType)
                {
                    @this.InvoiceItemType = new InvoiceItemTypes(@this.Transaction()).ProductItem;
                }
            }
        }
    }
}
