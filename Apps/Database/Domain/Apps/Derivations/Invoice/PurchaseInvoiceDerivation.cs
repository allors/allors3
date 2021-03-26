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

    public class PurchaseInvoiceDerivation : DomainDerivation
    {
        public PurchaseInvoiceDerivation(M m) : base(m, new Guid("7F6A083E-1409-4158-B302-603F0973A98C")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PurchaseInvoice, m.PurchaseInvoice.BilledTo),
                new RolePattern(m.PurchaseInvoice, m.PurchaseInvoice.BilledFrom),
                new RolePattern(m.PurchaseInvoice, m.PurchaseInvoice.PurchaseInvoiceItems),
                new RolePattern(m.PurchaseInvoiceItem, m.PurchaseInvoiceItem.PurchaseInvoiceItemState) { Steps =  new IPropertyType[] {m.PurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem} },
                new RolePattern(m.PurchaseInvoiceItem, m.PurchaseInvoiceItem.AmountPaid) { Steps =  new IPropertyType[] {m.PurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem} },
                new AssociationPattern(m.PaymentApplication.Invoice) { OfType = m.PurchaseInvoice.Class },
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
                    validation.AddError($"{@this} {this.M.PurchaseInvoice.BilledTo} {ErrorMessages.InternalOrganisationChanged}");
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
                        cycle.Validation.AddError($"{@this} {@this.Meta.BilledFrom} {ErrorMessages.PartyIsNotASupplier}");
                    }
                }

                @this.ValidInvoiceItems = @this.PurchaseInvoiceItems.Where(v => v.IsValid).ToArray();

                @this.AmountPaid = @this.PaymentApplicationsWhereInvoice.Sum(v => v.AmountApplied);

                //// Perhaps payments are recorded at the item level.
                if (@this.AmountPaid == 0)
                {
                    @this.AmountPaid = @this.ValidInvoiceItems.Sum(v => v.AmountPaid);
                }

                //DeriveWorkflow
                @this.WorkItemDescription = $"PurchaseInvoice: {@this.InvoiceNumber} [{@this.BilledFrom?.PartyName}]";

                //Sync
                foreach (PurchaseInvoiceItem invoiceItem in @this.PurchaseInvoiceItems)
                {
                    //invoiceItem.Sync(purchaseInvoice);
                    invoiceItem.SyncedInvoice = @this;
                }

                @this.ResetPrintDocument();
            }
        }
    }
}
