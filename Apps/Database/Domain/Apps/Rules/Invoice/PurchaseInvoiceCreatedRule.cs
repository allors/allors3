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

    public class PurchaseInvoiceCreatedRule : Rule
    {
        public PurchaseInvoiceCreatedRule(MetaPopulation m) : base(m, new Guid("be4a5700-8b16-4d7d-9c4a-ff5ba9cb07af")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseInvoice.RolePattern(v => v.PurchaseInvoiceState),
                m.PurchaseInvoice.RolePattern(v => v.AssignedVatRegime),
                m.PurchaseInvoice.RolePattern(v => v.AssignedIrpfRegime),
                m.PurchaseInvoice.RolePattern(v => v.InvoiceDate),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseInvoice>().Where(v => v.PurchaseInvoiceState.IsCreated))
            {
                @this.DerivedVatRegime = @this.AssignedVatRegime;
                @this.DerivedIrpfRegime = @this.AssignedIrpfRegime;

                if (@this.ExistInvoiceDate)
                {
                    @this.DerivedVatRate = @this.DerivedVatRegime?.VatRates.First(v => v.FromDate <= @this.InvoiceDate && (!v.ExistThroughDate || v.ThroughDate >= @this.InvoiceDate));
                    @this.DerivedIrpfRate = @this.DerivedIrpfRegime?.IrpfRates.First(v => v.FromDate <= @this.InvoiceDate && (!v.ExistThroughDate || v.ThroughDate >= @this.InvoiceDate));
                }
            }
        }
    }
}
