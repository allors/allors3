// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Meta;
    using Database.Derivations;
    using Resources;

    public class PurchaseInvoiceDerivation : DomainDerivation
    {
        public PurchaseInvoiceDerivation(M m) : base(m, new Guid("7F6A083E-1409-4158-B302-603F0973A98C")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.PurchaseInvoiceItem.Quantity) { Steps =  new IPropertyType[] {m.PurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem} },
                new ChangedPattern(this.M.PurchaseInvoiceItem.AssignedUnitPrice) { Steps =  new IPropertyType[] {m.PurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem} },
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

                var internalOrganisations = new Organisations(@this.Strategy.Session).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

                if (!@this.ExistBilledTo && internalOrganisations.Count() == 1)
                {
                    @this.BilledTo = internalOrganisations.First();
                }

                if (!@this.ExistInvoiceNumber)
                {
                    @this.InvoiceNumber = @this.BilledTo.NextPurchaseInvoiceNumber(@this.InvoiceDate.Year);
                    @this.SortableInvoiceNumber = NumberFormatter.SortableNumber(@this.BilledTo.PurchaseInvoiceNumberPrefix, @this.InvoiceNumber, @this.InvoiceDate.Year.ToString());
                }

                if (@this.BilledFrom is Organisation supplier)
                {
                    if (!@this.BilledTo.ActiveSuppliers.Contains(supplier))
                    {
                        cycle.Validation.AddError($"{@this} {@this.Meta.BilledFrom} {ErrorMessages.PartyIsNotASupplier}");
                    }
                }

                @this.VatRegime ??= @this.BilledFrom?.VatRegime;
                @this.IrpfRegime ??= (@this.BilledFrom as Organisation)?.IrpfRegime;

                @this.PurchaseOrders = @this.InvoiceItems.SelectMany(v => v.OrderItemBillingsWhereInvoiceItem).Select(v => v.OrderItem.OrderWhereValidOrderItem).ToArray();

                var validInvoiceItems = @this.PurchaseInvoiceItems.Where(v => v.IsValid).ToArray();
                @this.ValidInvoiceItems = validInvoiceItems;

                var purchaseInvoiceStates = new PurchaseInvoiceStates(@this.Strategy.Session);
                var purchaseInvoiceItemStates = new PurchaseInvoiceItemStates(@this.Strategy.Session);

                @this.AmountPaid = @this.PaymentApplicationsWhereInvoice.Sum(v => v.AmountApplied);

                //// Perhaps payments are recorded at the item level.
                if (@this.AmountPaid == 0)
                {
                    @this.AmountPaid = @this.InvoiceItems.Sum(v => v.AmountPaid);
                }

                foreach (var invoiceItem in validInvoiceItems)
                {
                    if (invoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoiceState.IsCreated)
                    {
                        invoiceItem.PurchaseInvoiceItemState = purchaseInvoiceItemStates.Created;
                    }
                    else if (invoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoiceState.IsAwaitingApproval)
                    {
                        invoiceItem.PurchaseInvoiceItemState = purchaseInvoiceItemStates.AwaitingApproval;
                    }
                    else if (invoiceItem.AmountPaid == 0)
                    {
                        invoiceItem.PurchaseInvoiceItemState = purchaseInvoiceItemStates.NotPaid;
                    }
                    else if (invoiceItem.ExistAmountPaid && invoiceItem.AmountPaid >= invoiceItem.TotalIncVat)
                    {
                        invoiceItem.PurchaseInvoiceItemState = purchaseInvoiceItemStates.Paid;
                    }
                    else
                    {
                        invoiceItem.PurchaseInvoiceItemState = purchaseInvoiceItemStates.PartiallyPaid;
                    }
                }

                if (validInvoiceItems.Any())
                {
                    if (!@this.PurchaseInvoiceState.IsCreated && !@this.PurchaseInvoiceState.IsAwaitingApproval)
                    {
                        if (@this.PurchaseInvoiceItems.All(v => v.PurchaseInvoiceItemState.IsPaid))
                        {
                            @this.PurchaseInvoiceState = purchaseInvoiceStates.Paid;
                        }
                        else if (@this.PurchaseInvoiceItems.All(v => v.PurchaseInvoiceItemState.IsNotPaid))
                        {
                            @this.PurchaseInvoiceState = purchaseInvoiceStates.NotPaid;
                        }
                        else
                        {
                            @this.PurchaseInvoiceState = purchaseInvoiceStates.PartiallyPaid;
                        }
                    }
                }

                if (!@this.PurchaseInvoiceState.IsCreated && !@this.PurchaseInvoiceState.IsAwaitingApproval)
                {
                    foreach (var invoiceItem in validInvoiceItems)
                    {
                        if (invoiceItem.ExistSerialisedItem
                            && @this.BilledTo.SerialisedItemSoldOns.Contains(new SerialisedItemSoldOns(@this.Session()).PurchaseInvoiceConfirm))
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
                                invoiceItem.SerialisedItem.Ownership = new Ownerships(@this.Session()).Own;
                            }
                        }
                    }
                }

                // If disbursements are not matched at invoice level
                if (!@this.PurchaseInvoiceState.IsRevising
                    && @this.AmountPaid != 0)
                {
                    if (@this.AmountPaid >= decimal.Round(@this.TotalIncVat, 2))
                    {
                        @this.PurchaseInvoiceState = purchaseInvoiceStates.Paid;
                    }
                    else
                    {
                        @this.PurchaseInvoiceState = purchaseInvoiceStates.PartiallyPaid;
                    }

                    foreach (var invoiceItem in validInvoiceItems)
                    {
                        if (@this.AmountPaid >= decimal.Round(@this.TotalIncVat, 2))
                        {
                            invoiceItem.PurchaseInvoiceItemState = purchaseInvoiceItemStates.Paid;
                        }
                        else
                        {
                            invoiceItem.PurchaseInvoiceItemState = purchaseInvoiceItemStates.PartiallyPaid;
                        }
                    }
                }

                //AppsOnDeriveInvoiceItems
                foreach (PurchaseInvoiceItem purchaseInvoiceItem in @this.ValidInvoiceItems)
                {
                    if (purchaseInvoiceItem.PurchaseInvoiceItemState.IsNotPaid
                        && purchaseInvoiceItem.ExistSerialisedItem
                        && purchaseInvoiceItem.InvoiceItemType.IsProductItem)
                    {
                        var serialisedItem = purchaseInvoiceItem.SerialisedItem;
                        var deriveRoles = purchaseInvoiceItem.SerialisedItem;

                        serialisedItem.RemoveAssignedPurchasePrice();
                        deriveRoles.PurchasePrice = purchaseInvoiceItem.TotalExVat;
                    }

                    //purchaseInvoiceItem.AppsOnDerivePrices();
                    purchaseInvoiceItem.UnitBasePrice = 0;
                    purchaseInvoiceItem.UnitDiscount = 0;
                    purchaseInvoiceItem.UnitSurcharge = 0;

                    if (purchaseInvoiceItem.AssignedUnitPrice.HasValue)
                    {
                        purchaseInvoiceItem.UnitBasePrice = purchaseInvoiceItem.AssignedUnitPrice.Value;
                        purchaseInvoiceItem.UnitPrice = purchaseInvoiceItem.AssignedUnitPrice.Value;
                    }
                    else
                    {
                        var invoice = purchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem;
                        if (purchaseInvoiceItem.ExistPart)
                        {
                            purchaseInvoiceItem.UnitBasePrice = new SupplierOfferings(purchaseInvoiceItem.Strategy.Session).PurchasePrice(invoice.BilledFrom, invoice.InvoiceDate, purchaseInvoiceItem.Part);
                        }
                    }

                    if (purchaseInvoiceItem.ExistUnitBasePrice)
                    {
                        purchaseInvoiceItem.VatRegime = purchaseInvoiceItem.AssignedVatRegime ?? purchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem.VatRegime;
                        purchaseInvoiceItem.VatRate = purchaseInvoiceItem.VatRegime?.VatRate;

                        purchaseInvoiceItem.IrpfRegime = purchaseInvoiceItem.AssignedIrpfRegime ?? purchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem.IrpfRegime;
                        purchaseInvoiceItem.IrpfRate = purchaseInvoiceItem.IrpfRegime?.IrpfRate;

                        purchaseInvoiceItem.TotalBasePrice = purchaseInvoiceItem.UnitBasePrice * purchaseInvoiceItem.Quantity;
                        purchaseInvoiceItem.TotalDiscount = purchaseInvoiceItem.UnitDiscount * purchaseInvoiceItem.Quantity;
                        purchaseInvoiceItem.TotalSurcharge = purchaseInvoiceItem.UnitSurcharge * purchaseInvoiceItem.Quantity;
                        purchaseInvoiceItem.UnitPrice = purchaseInvoiceItem.UnitBasePrice - purchaseInvoiceItem.UnitDiscount + purchaseInvoiceItem.UnitSurcharge;

                        purchaseInvoiceItem.UnitVat = purchaseInvoiceItem.ExistVatRate ? purchaseInvoiceItem.UnitPrice * purchaseInvoiceItem.VatRate.Rate / 100 : 0;
                        purchaseInvoiceItem.UnitIrpf = purchaseInvoiceItem.ExistIrpfRate ? purchaseInvoiceItem.UnitPrice * purchaseInvoiceItem.IrpfRate.Rate / 100 : 0;
                        purchaseInvoiceItem.TotalVat = purchaseInvoiceItem.UnitVat * purchaseInvoiceItem.Quantity;
                        purchaseInvoiceItem.TotalExVat = purchaseInvoiceItem.UnitPrice * purchaseInvoiceItem.Quantity;
                        purchaseInvoiceItem.TotalIrpf = purchaseInvoiceItem.UnitIrpf * purchaseInvoiceItem.Quantity;
                        purchaseInvoiceItem.TotalIncVat = purchaseInvoiceItem.TotalExVat + purchaseInvoiceItem.TotalVat;
                        purchaseInvoiceItem.GrandTotal = purchaseInvoiceItem.TotalIncVat - purchaseInvoiceItem.TotalIrpf;
                    }
                }

                //AppsOnDeriveInvoiceTotals
                @this.TotalBasePrice = 0;
                @this.TotalDiscount = 0;
                @this.TotalSurcharge = 0;
                @this.TotalVat = 0;
                @this.TotalExVat = 0;
                @this.TotalIncVat = 0;
                @this.TotalIrpf = 0;
                @this.TotalExtraCharge = 0;
                @this.GrandTotal = 0;

                foreach (PurchaseInvoiceItem item in @this.PurchaseInvoiceItems)
                {
                    @this.TotalBasePrice += item.TotalBasePrice;
                    @this.TotalSurcharge += item.TotalSurcharge;
                    @this.TotalIrpf += item.TotalIrpf;
                    @this.TotalVat += item.TotalVat;
                    @this.TotalExVat += item.TotalExVat;
                    @this.TotalIncVat += item.TotalIncVat;
                    @this.GrandTotal += item.GrandTotal;
                }

                var discount = 0M;
                var discountVat = 0M;
                var discountIrpf = 0M;
                var surcharge = 0M;
                var surchargeVat = 0M;
                var surchargeIrpf = 0M;
                var fee = 0M;
                var feeVat = 0M;
                var feeIrpf = 0M;
                var shipping = 0M;
                var shippingVat = 0M;
                var shippingIrpf = 0M;
                var miscellaneous = 0M;
                var miscellaneousVat = 0M;
                var miscellaneousIrpf = 0M;

                foreach (OrderAdjustment orderAdjustment in @this.OrderAdjustments)
                {
                    if (orderAdjustment.GetType().Name.Equals(typeof(DiscountAdjustment).Name))
                    {
                        discount = orderAdjustment.Percentage.HasValue ?
                                        Math.Round(@this.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                        orderAdjustment.Amount ?? 0;

                        @this.TotalDiscount += discount;

                        if (@this.ExistVatRegime)
                        {
                            discountVat = Math.Round(discount * @this.VatRegime.VatRate.Rate / 100, 2);
                        }

                        if (@this.ExistIrpfRegime)
                        {
                            discountIrpf = Math.Round(discount * @this.IrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(SurchargeAdjustment).Name))
                    {
                        surcharge = orderAdjustment.Percentage.HasValue ?
                                            Math.Round(@this.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                            orderAdjustment.Amount ?? 0;

                        @this.TotalSurcharge += surcharge;

                        if (@this.ExistVatRegime)
                        {
                            surchargeVat = Math.Round(surcharge * @this.VatRegime.VatRate.Rate / 100, 2);
                        }

                        if (@this.ExistIrpfRegime)
                        {
                            surchargeIrpf = Math.Round(surcharge * @this.IrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(Fee).Name))
                    {
                        fee = orderAdjustment.Percentage.HasValue ?
                                    Math.Round(@this.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                    orderAdjustment.Amount ?? 0;

                        @this.TotalFee += fee;

                        if (@this.ExistVatRegime)
                        {
                            feeVat = Math.Round(fee * @this.VatRegime.VatRate.Rate / 100, 2);
                        }

                        if (@this.ExistIrpfRegime)
                        {
                            feeIrpf = Math.Round(fee * @this.IrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(ShippingAndHandlingCharge).Name))
                    {
                        shipping = orderAdjustment.Percentage.HasValue ?
                                        Math.Round(@this.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                        orderAdjustment.Amount ?? 0;

                        @this.TotalShippingAndHandling += shipping;

                        if (@this.ExistVatRegime)
                        {
                            shippingVat = Math.Round(shipping * @this.VatRegime.VatRate.Rate / 100, 2);
                        }

                        if (@this.ExistIrpfRegime)
                        {
                            shippingIrpf = Math.Round(shipping * @this.IrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(MiscellaneousCharge).Name))
                    {
                        miscellaneous = orderAdjustment.Percentage.HasValue ?
                                        Math.Round(@this.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                        orderAdjustment.Amount ?? 0;

                        @this.TotalExtraCharge += miscellaneous;

                        if (@this.ExistVatRegime)
                        {
                            miscellaneousVat = Math.Round(miscellaneous * @this.VatRegime.VatRate.Rate / 100, 2);
                        }

                        if (@this.ExistIrpfRegime)
                        {
                            miscellaneousIrpf = Math.Round(miscellaneous * @this.IrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }
                }

                @this.TotalExtraCharge = fee + shipping + miscellaneous;

                @this.TotalExVat = @this.TotalExVat - discount + surcharge + fee + shipping + miscellaneous;
                @this.TotalVat = @this.TotalVat - discountVat + surchargeVat + feeVat + shippingVat + miscellaneousVat;
                @this.TotalIncVat = @this.TotalIncVat - discount - discountVat + surcharge + surchargeVat + fee + feeVat + shipping + shippingVat + miscellaneous + miscellaneousVat;
                @this.TotalIrpf = @this.TotalIrpf + discountIrpf - surchargeIrpf - feeIrpf - shippingIrpf - miscellaneousIrpf;
                @this.GrandTotal = @this.TotalIncVat - @this.TotalIrpf;

                //DeriveWorkflow
                @this.WorkItemDescription = $"PurchaseInvoice: {@this.InvoiceNumber} [{@this.BilledFrom?.PartyName}]";

                if (@this.PurchaseInvoiceState.IsAwaitingApproval)
                {
                    if (!@this.OpenTasks.OfType<PurchaseInvoiceApproval>().Any())
                    {
                        var approval = new PurchaseInvoiceApprovalBuilder(@this.Session()).WithPurchaseInvoice(@this).Build();
                        approval.WorkItem = approval.PurchaseInvoice;
                    }
                }

                var singleton = @this.Session().GetSingleton();

                @this.AddSecurityToken(new SecurityTokens(@this.Session()).DefaultSecurityToken);

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
