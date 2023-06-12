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

    public class PurchaseInvoiceCreatedInvoiceItemRule : Rule
    {
        public PurchaseInvoiceCreatedInvoiceItemRule(MetaPopulation m) : base(m, new Guid("8c875a7d-ac54-4fcf-bd63-7223ed3217b4")) =>
            this.Patterns = new Pattern[]
            {
                m.OrderItemBilling.RolePattern(v => v.InvoiceItem, v => v.InvoiceItem.ObjectType.AsPurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.PurchaseInvoice.RolePattern(v => v.PurchaseInvoiceItems),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseInvoice>().Where(v => v.PurchaseInvoiceState.IsCreated || v.PurchaseInvoiceState.IsRevising))
            {
                @this.PurchaseOrders = @this.InvoiceItems
                    .SelectMany(v => v.OrderItemBillingsWhereInvoiceItem)
                    .Select(v => v.OrderItem.OrderWhereValidOrderItem)
                    .OfType<PurchaseOrder>();
            }
        }
    }
}
