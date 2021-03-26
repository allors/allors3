// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class SalesInvoiceItemDerivation : DomainDerivation
    {
        public SalesInvoiceItemDerivation(M m) : base(m, new Guid("37C0910B-7C48-46B5-8F7A-F6B2E70BE05C")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.SalesInvoiceItem.Product),
                new RolePattern(m.SalesInvoiceItem.ProductFeatures),
                new RolePattern(m.SalesInvoiceItem.Part),
                new RolePattern(m.SalesInvoiceItem.SerialisedItem),
                new RolePattern(m.SalesInvoiceItem.InvoiceItemType),
                new RolePattern(m.SalesInvoiceItem.AssignedVatRegime),
                new RolePattern(m.SalesInvoiceItem.AssignedIrpfRegime),
                new AssociationPattern(m.SalesInvoice.SalesInvoiceItems),
                new RolePattern(m.SalesInvoice.SalesInvoiceState) { Steps =  new IPropertyType[] {this.M.SalesInvoice.SalesInvoiceItems} },
                new RolePattern(m.SalesInvoice.DerivedVatRegime) { Steps =  new IPropertyType[] {this.M.SalesInvoice.SalesInvoiceItems} },
                new RolePattern(m.SalesInvoice.DerivedIrpfRegime) { Steps =  new IPropertyType[] {this.M.SalesInvoice.SalesInvoiceItems} },
                new RolePattern(m.SalesInvoice.InvoiceDate) { Steps =  new IPropertyType[] {this.M.SalesInvoice.SalesInvoiceItems} },
                new RolePattern(m.PaymentApplication.AmountApplied) { Steps =  new IPropertyType[] {this.M.PaymentApplication.InvoiceItem}, OfType = m.SalesInvoiceItem.Class },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;
            var changeSet = cycle.ChangeSet;

            foreach (var @this in matches.Cast<SalesInvoiceItem>())
            {
                var salesInvoice = @this.SalesInvoiceWhereSalesInvoiceItem;
                var salesInvoiceItemStates = new SalesInvoiceItemStates(transaction);

                validation.AssertExistsAtMostOne(@this, this.M.SalesInvoiceItem.Product, this.M.SalesInvoiceItem.ProductFeatures, this.M.SalesInvoiceItem.Part);
                validation.AssertExistsAtMostOne(@this, this.M.SalesInvoiceItem.SerialisedItem, this.M.SalesInvoiceItem.ProductFeatures, this.M.SalesInvoiceItem.Part);

                if (@this.ExistSerialisedItem
                    && salesInvoice != null
                    && salesInvoice.ExistSalesInvoiceType
                    && !@this.ExistNextSerialisedItemAvailability
                    && salesInvoice.SalesInvoiceType.Equals(new SalesInvoiceTypes(@this.Transaction()).SalesInvoice))
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

                @this.DerivedVatRegime = @this.ExistAssignedVatRegime ? @this.AssignedVatRegime : @this.SalesInvoiceWhereSalesInvoiceItem?.DerivedVatRegime;
                @this.VatRate = @this.DerivedVatRegime?.VatRates.First(v => v.FromDate <= salesInvoice.InvoiceDate && (!v.ExistThroughDate || v.ThroughDate >= salesInvoice.InvoiceDate));

                @this.DerivedIrpfRegime = @this.ExistAssignedIrpfRegime ? @this.AssignedIrpfRegime : @this.SalesInvoiceWhereSalesInvoiceItem?.DerivedIrpfRegime;
                @this.IrpfRate = @this.DerivedIrpfRegime?.IrpfRates.First(v => v.FromDate <= salesInvoice.InvoiceDate && (!v.ExistThroughDate || v.ThroughDate >= salesInvoice.InvoiceDate));

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
