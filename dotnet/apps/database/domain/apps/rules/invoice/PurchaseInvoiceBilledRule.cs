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
    using Resources;

    public class PurchaseInvoiceBilledRule : Rule
    {
        public PurchaseInvoiceBilledRule(MetaPopulation m) : base(m, new Guid("100544db-84dd-41c7-accf-c161799a2918")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseInvoice.RolePattern(v => v.BilledTo),
                m.PurchaseInvoice.RolePattern(v => v.BilledFrom),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseInvoice>())
            {
                if (@this.ExistCurrentVersion
                   && @this.CurrentVersion.ExistBilledTo
                   && @this.BilledTo != @this.CurrentVersion.BilledTo)
                {
                    validation.AddError(@this, this.M.PurchaseInvoice.BilledTo, ErrorMessages.InternalOrganisationChanged);
                }

                if (!@this.ExistInvoiceNumber && @this.ExistBilledTo)
                {
                    var year = @this.InvoiceDate.Year;
                    @this.InvoiceNumber = @this.BilledTo.NextPurchaseInvoiceNumber(year);

                    var fiscalYearInternalOrganisationSequenceNumbers = @this.BilledTo.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);
                    var prefix = @this.BilledTo.InvoiceSequence.IsEnforcedSequence ? @this.BilledTo.PurchaseInvoiceNumberPrefix : fiscalYearInternalOrganisationSequenceNumbers.PurchaseInvoiceNumberPrefix;
                    @this.SortableInvoiceNumber = @this.Transaction().GetSingleton().SortableNumber(prefix, @this.InvoiceNumber, year.ToString());
                }

                if (@this.BilledFrom is Organisation supplier)
                {
                    if (!@this.BilledTo.ActiveSuppliers.Contains(supplier))
                    {
                        cycle.Validation.AddError(@this, @this.Meta.BilledFrom, ErrorMessages.PartyIsNotASupplier);
                    }
                }

                //DeriveWorkflow
                @this.WorkItemDescription = $"PurchaseInvoice: {@this.InvoiceNumber} [{@this.BilledFrom?.PartyName}]";
            }
        }
    }
}
