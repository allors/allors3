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

    public class SalesInvoiceReadyForPostingRule : Rule
    {
        public SalesInvoiceReadyForPostingRule(MetaPopulation m) : base(m, new Guid("cc2c0d60-b488-4eed-be97-13c2cc17fdf8")) =>
            this.Patterns = new Pattern[]
        {
            m.SalesInvoice.RolePattern(v => v.AssignedVatRegime),
            m.SalesInvoice.RolePattern(v => v.AssignedIrpfRegime),
            m.SalesInvoice.RolePattern(v => v.InvoiceDate),
            m.SalesInvoice.RolePattern(v => v.AssignedVatClause),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>().Where(v => v.SalesInvoiceState.IsReadyForPosting))
            {
                @this.DerivedVatRegime = @this.AssignedVatRegime;
                @this.DerivedIrpfRegime = @this.AssignedIrpfRegime;

                if (@this.ExistInvoiceDate)
                {
                    @this.DerivedVatRate = @this.DerivedVatRegime?.VatRates.First(v => v.FromDate <= @this.InvoiceDate && (!v.ExistThroughDate || v.ThroughDate >= @this.InvoiceDate));
                    @this.DerivedIrpfRate = @this.DerivedIrpfRegime?.IrpfRates.First(v => v.FromDate <= @this.InvoiceDate && (!v.ExistThroughDate || v.ThroughDate >= @this.InvoiceDate));
                }

                if (@this.ExistDerivedVatRegime)
                {
                    if (@this.DerivedVatRegime.ExistVatClause)
                    {
                        @this.DerivedVatClause = @this.DerivedVatRegime.VatClause;
                    }
                    else
                    {
                        @this.RemoveDerivedVatClause();
                    }
                }
                else
                {
                    @this.RemoveDerivedVatClause();
                }

                @this.DerivedVatClause = @this.ExistAssignedVatClause ? @this.AssignedVatClause : @this.DerivedVatClause;
            }
        }
    }
}
