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

    public class SupplierOfferingExistSupplierRule : Rule
    {
        public SupplierOfferingExistSupplierRule(MetaPopulation m) : base(m, new Guid("aa226c51-1c2a-4d0f-8dec-2338f32c9b9a")) =>
            this.Patterns = new Pattern[]
            {
                m.SupplierOffering.RolePattern(v => v.Price),
                m.SupplierOffering.RolePattern(v => v.Supplier),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SupplierOffering>())
            {
                if (@this.ExistSupplier)
                {
                    foreach (var purchaseInvoice in @this.Supplier.PurchaseInvoicesWhereBilledFrom.Where(v => v.ExistPurchaseInvoiceState && (v.PurchaseInvoiceState.IsCreated || v.PurchaseInvoiceState.IsRevising)))
                    {
                        purchaseInvoice.DerivationTrigger = Guid.NewGuid();
                    }

                    foreach (var purchaseOrder in ((Organisation)@this.Supplier).PurchaseOrdersWhereTakenViaSupplier.Where(v => v.ExistPurchaseOrderState && v.PurchaseOrderState.IsCreated))
                    {
                        purchaseOrder.DerivationTrigger = Guid.NewGuid();
                    }
                }
            }
        }
    }
}
