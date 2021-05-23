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
    using Database.Derivations;
    using Resources;

    public class SalesInvoiceItemInvoiceItemTypeRule : Rule
    {
        public SalesInvoiceItemInvoiceItemTypeRule(MetaPopulation m) : base(m, new Guid("bec4fac0-cf18-4487-af15-0985c8990c44")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesInvoiceItem.RolePattern(v => v.InvoiceItemType),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;
            var changeSet = cycle.ChangeSet;

            foreach (var @this in matches.Cast<SalesInvoiceItem>())
            {
                if (@this.ExistInvoiceItemType && @this.InvoiceItemType.MaxQuantity.HasValue && @this.Quantity > @this.InvoiceItemType.MaxQuantity.Value)
                {
                    validation.AddError($"{@this}, {this.M.SalesInvoiceItem.Quantity},{ ErrorMessages.InvalidQuantity}");
                }
            }
        }
    }
}
