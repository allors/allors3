// <copyright file="PurchaseInvoiceSerialisedItemDerivation.cs" company="Allors bvba">
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

    public class PurchaseInvoiceSerialisedItemRule : Rule
    {
        public PurchaseInvoiceSerialisedItemRule(MetaPopulation m) : base(m, new Guid("21aa08df-18b8-4d50-b882-455fb453a67a")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseInvoice.RolePattern(v => v.PurchaseInvoiceState),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseInvoice>())
            {
                if (@this.PurchaseInvoiceState.IsNotPaid)
                {
                    foreach (PurchaseInvoiceItem invoiceItem in @this.ValidInvoiceItems)
                    {
                        if (invoiceItem.ExistSerialisedItem)
                        {
                            invoiceItem.SerialisedItem.RemoveAssignedPurchasePrice();
                            invoiceItem.SerialisedItem.PurchasePrice = invoiceItem.TotalExVat;
                        }

                        if (invoiceItem.ExistSerialisedItem
                            && @this.BilledTo.SerialisedItemSoldOns.Contains(new SerialisedItemSoldOns(@this.Transaction()).PurchaseInvoiceConfirm))
                        {
                            if ((@this.BilledFrom as InternalOrganisation)?.IsInternalOrganisation == false)
                            {
                                invoiceItem.SerialisedItem.Buyer = @this.BilledTo;
                            }

                            // who comes first?
                            // Item you purchased can be on sold via sales invoice even before purchase invoice is created and confirmed!!
                            if (!invoiceItem.SerialisedItem.SalesInvoiceItemsWhereSerialisedItem.Any(v => (v.SalesInvoiceWhereSalesInvoiceItem.BillToCustomer as Organisation)?.IsInternalOrganisation == false))
                            {
                                invoiceItem.SerialisedItem.OwnedBy = @this.BilledTo;
                                invoiceItem.SerialisedItem.Ownership = new Ownerships(@this.Transaction()).Own;
                            }
                        }
                    }
                }
            }
        }
    }
}
