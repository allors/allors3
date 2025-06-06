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

    public class PurchaseInvoiceRule : Rule
    {
        public PurchaseInvoiceRule(MetaPopulation m) : base(m, new Guid("27dddf28-8864-443a-a5f5-3caa64b490d9")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseInvoice.RolePattern(v => v.PurchaseInvoiceItems),
                m.PurchaseInvoiceItem.RolePattern(v => v.PurchaseInvoiceItemState, v => v.PurchaseInvoiceWherePurchaseInvoiceItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseInvoice>())
            {
                @this.ValidInvoiceItems = @this.PurchaseInvoiceItems.Where(v => v.IsValid).ToArray();

                //Sync
                foreach (var invoiceItem in @this.PurchaseInvoiceItems)
                {
                    invoiceItem.DelegatedAccess = @this;
                }

                @this.ResetPrintDocument();
            }
        }
    }
}
