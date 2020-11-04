// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public class SalesInvoiceItemDerivation : DomainDerivation
    {
        public SalesInvoiceItemDerivation(M m) : base(m, new Guid("37C0910B-7C48-46B5-8F7A-F6B2E70BE05C")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.SalesInvoiceItem.Product),
                new ChangedPattern(m.SalesInvoiceItem.ProductFeatures),
                new ChangedPattern(m.SalesInvoiceItem.Part),
                new ChangedPattern(m.SalesInvoiceItem.SerialisedItem),
                new ChangedPattern(m.SalesInvoiceItem.InvoiceItemType),
                new ChangedPattern(m.SalesInvoiceItem.AssignedVatRegime),
                new ChangedPattern(m.SalesInvoiceItem.AssignedIrpfRegime),
                new ChangedPattern(m.SalesInvoice.SalesInvoiceState) { Steps =  new IPropertyType[] {this.M.SalesInvoice.SalesInvoiceItems} },
                new ChangedPattern(m.SalesInvoice.VatRegime) { Steps =  new IPropertyType[] {this.M.SalesInvoice.SalesInvoiceItems} },
                new ChangedPattern(m.SalesInvoice.IrpfRegime) { Steps =  new IPropertyType[] {this.M.SalesInvoice.SalesInvoiceItems} },
                new ChangedPattern(m.PaymentApplication.AmountApplied) { Steps =  new IPropertyType[] {this.M.PaymentApplication.InvoiceItem}, OfType = m.SalesInvoiceItem.Class },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;
            var changeSet = cycle.ChangeSet;

            foreach (var @this in matches.Cast<SalesInvoiceItem>())
            {
                var salesInvoice = @this.SalesInvoiceWhereSalesInvoiceItem;
                var salesInvoiceItemStates = new SalesInvoiceItemStates(session);

                validation.AssertExistsAtMostOne(@this, this.M.SalesInvoiceItem.Product, this.M.SalesInvoiceItem.ProductFeatures, this.M.SalesInvoiceItem.Part);
                validation.AssertExistsAtMostOne(@this, this.M.SalesInvoiceItem.SerialisedItem, this.M.SalesInvoiceItem.ProductFeatures, this.M.SalesInvoiceItem.Part);

                if (@this.ExistSerialisedItem && !@this.ExistNextSerialisedItemAvailability && salesInvoice.SalesInvoiceType.Equals(new SalesInvoiceTypes(@this.Session()).SalesInvoice))
                {
                    validation.AssertExists(@this, @this.Meta.NextSerialisedItemAvailability);
                }

                if (@this.Part != null && @this.Part.InventoryItemKind.IsSerialised && @this.Quantity != 1)
                {
                    validation.AddError($"{@this}, {this.M.SalesInvoiceItem.Quantity},{ ErrorMessages.InvalidQuantity}");
                }

                if (@this.Part != null && @this.Part.InventoryItemKind.IsNonSerialised && @this.Quantity == 0)
                {
                    validation.AddError($"{@this}, {this.M.SalesInvoiceItem.Quantity},{ ErrorMessages.InvalidQuantity}");
                }

                if (@this.ExistInvoiceItemType && @this.InvoiceItemType.MaxQuantity.HasValue && @this.Quantity > @this.InvoiceItemType.MaxQuantity.Value)
                {
                    validation.AddError($"{@this}, {this.M.SalesInvoiceItem.Quantity},{ ErrorMessages.InvalidQuantity}");
                }

                @this.VatRegime = @this.ExistAssignedVatRegime ? @this.AssignedVatRegime : @this.SalesInvoiceWhereSalesInvoiceItem?.VatRegime;
                @this.VatRate = @this.VatRegime?.VatRate;

                @this.IrpfRegime = @this.ExistAssignedIrpfRegime ? @this.AssignedIrpfRegime : @this.SalesInvoiceWhereSalesInvoiceItem?.IrpfRegime;
                @this.IrpfRate = @this.IrpfRegime?.IrpfRate;

                if (@this.ExistInvoiceItemType && @this.IsSubTotalItem().Result == true && @this.Quantity <= 0)
                {
                    validation.AssertExists(@this, @this.Meta.Quantity);
                }

                var amountPaid = 0M;
                foreach (PaymentApplication paymentApplication in @this.PaymentApplicationsWhereInvoiceItem)
                {
                    amountPaid += paymentApplication.AmountApplied;
                }

                if (amountPaid != @this.AmountPaid)
                {
                    @this.AmountPaid = amountPaid;
                }

                if (salesInvoice != null
                    && salesInvoice.ExistSalesInvoiceState
                    && salesInvoice.SalesInvoiceState.IsReadyForPosting
                    && @this.ExistSalesInvoiceItemState
                    && @this.SalesInvoiceItemState.IsCancelledByInvoice)
                {
                    @this.SalesInvoiceItemState = salesInvoiceItemStates.ReadyForPosting;
                }

                // SalesInvoiceItem States
                if (salesInvoice != null
                    && salesInvoice.ExistSalesInvoiceState
                    && @this.ExistSalesInvoiceItemState
                    && @this.IsValid)
                {
                    if (salesInvoice.SalesInvoiceState.IsWrittenOff)
                    {
                        @this.SalesInvoiceItemState = salesInvoiceItemStates.WrittenOff;
                    }

                    if (salesInvoice.SalesInvoiceState.IsCancelled)
                    {
                        @this.SalesInvoiceItemState = salesInvoiceItemStates.CancelledByInvoice;
                    }
                }
            }
        }
    }
}
