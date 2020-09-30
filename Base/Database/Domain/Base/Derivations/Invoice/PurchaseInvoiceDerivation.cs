// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;
    using Resources;

    public class PurchaseInvoiceDerivation : DomainDerivation
    {
        public PurchaseInvoiceDerivation(M m) : base(m, new Guid("7F6A083E-1409-4158-B302-603F0973A98C")) =>
            this.Patterns = new[]
            {
                new CreatedPattern(M.PurchaseInvoice.Class)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var purchaseInvoice in matches.Cast<PurchaseInvoice>())
            {
                var internalOrganisations = new Organisations(purchaseInvoice.Strategy.Session).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

                if (!purchaseInvoice.ExistBilledTo && internalOrganisations.Count() == 1)
                {
                    purchaseInvoice.BilledTo = internalOrganisations.First();
                }

                if (!purchaseInvoice.ExistInvoiceNumber)
                {
                    purchaseInvoice.InvoiceNumber = purchaseInvoice.BilledTo.NextPurchaseInvoiceNumber(purchaseInvoice.InvoiceDate.Year);
                    purchaseInvoice.SortableInvoiceNumber = purchaseInvoice.Session().GetSingleton().SortableNumber(purchaseInvoice.BilledTo.PurchaseInvoiceNumberPrefix, purchaseInvoice.InvoiceNumber, purchaseInvoice.InvoiceDate.Year.ToString());
                }

                if (purchaseInvoice.BilledFrom is Organisation supplier)
                {
                    if (!purchaseInvoice.BilledTo.ActiveSuppliers.Contains(supplier))
                    {
                        cycle.Validation.AddError($"{purchaseInvoice} {purchaseInvoice.Meta.BilledFrom} {ErrorMessages.PartyIsNotASupplier}");
                    }
                }

                purchaseInvoice.VatRegime ??= purchaseInvoice.BilledFrom?.VatRegime;
                purchaseInvoice.IrpfRegime ??= (purchaseInvoice.BilledFrom as Organisation)?.IrpfRegime;

                purchaseInvoice.PurchaseOrders = purchaseInvoice.InvoiceItems.SelectMany(v => v.OrderItemBillingsWhereInvoiceItem).Select(v => v.OrderItem.OrderWhereValidOrderItem).ToArray();

                var validInvoiceItems = purchaseInvoice.PurchaseInvoiceItems.Where(v => v.IsValid).ToArray();
                purchaseInvoice.ValidInvoiceItems = validInvoiceItems;

                var purchaseInvoiceStates = new PurchaseInvoiceStates(purchaseInvoice.Strategy.Session);
                var purchaseInvoiceItemStates = new PurchaseInvoiceItemStates(purchaseInvoice.Strategy.Session);

                purchaseInvoice.AmountPaid = purchaseInvoice.PaymentApplicationsWhereInvoice.Sum(v => v.AmountApplied);

                //// Perhaps payments are recorded at the item level.
                if (purchaseInvoice.AmountPaid == 0)
                {
                    purchaseInvoice.AmountPaid = purchaseInvoice.InvoiceItems.Sum(v => v.AmountPaid);
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
                    if (!purchaseInvoice.PurchaseInvoiceState.IsCreated && !purchaseInvoice.PurchaseInvoiceState.IsAwaitingApproval)
                    {
                        if (purchaseInvoice.PurchaseInvoiceItems.All(v => v.PurchaseInvoiceItemState.IsPaid))
                        {
                            purchaseInvoice.PurchaseInvoiceState = purchaseInvoiceStates.Paid;
                        }
                        else if (purchaseInvoice.PurchaseInvoiceItems.All(v => v.PurchaseInvoiceItemState.IsNotPaid))
                        {
                            purchaseInvoice.PurchaseInvoiceState = purchaseInvoiceStates.NotPaid;
                        }
                        else
                        {
                            purchaseInvoice.PurchaseInvoiceState = purchaseInvoiceStates.PartiallyPaid;
                        }
                    }
                }

                if (!purchaseInvoice.PurchaseInvoiceState.IsCreated && !purchaseInvoice.PurchaseInvoiceState.IsAwaitingApproval)
                {
                    foreach (var invoiceItem in validInvoiceItems)
                    {
                        if (invoiceItem.ExistSerialisedItem
                            && purchaseInvoice.BilledTo.SerialisedItemSoldOns.Contains(new SerialisedItemSoldOns(purchaseInvoice.Session()).PurchaseInvoiceConfirm))
                        {
                            if ((purchaseInvoice.BilledFrom as InternalOrganisation)?.IsInternalOrganisation == false)
                            {
                                invoiceItem.SerialisedItem.Buyer = purchaseInvoice.BilledTo;
                            }

                            // who comes first?
                            // Item you purchased can be on sold via sales invoice even before purchase invoice is created and confirmed!!
                            if (!invoiceItem.SerialisedItem.SalesInvoiceItemsWhereSerialisedItem.Any(v => (v.SalesInvoiceWhereSalesInvoiceItem.BillToCustomer as Organisation)?.IsInternalOrganisation == false))
                            {
                                invoiceItem.SerialisedItem.OwnedBy = purchaseInvoice.BilledTo;
                                invoiceItem.SerialisedItem.Ownership = new Ownerships(purchaseInvoice.Session()).Own;
                            }
                        }
                    }
                }

                // If disbursements are not matched at invoice level
                if (!purchaseInvoice.PurchaseInvoiceState.IsRevising
                    && purchaseInvoice.AmountPaid != 0)
                {
                    if (purchaseInvoice.AmountPaid >= decimal.Round(purchaseInvoice.TotalIncVat, 2))
                    {
                        purchaseInvoice.PurchaseInvoiceState = purchaseInvoiceStates.Paid;
                    }
                    else
                    {
                        purchaseInvoice.PurchaseInvoiceState = purchaseInvoiceStates.PartiallyPaid;
                    }

                    foreach (var invoiceItem in validInvoiceItems)
                    {
                        if (purchaseInvoice.AmountPaid >= decimal.Round(purchaseInvoice.TotalIncVat, 2))
                        {
                            invoiceItem.PurchaseInvoiceItemState = purchaseInvoiceItemStates.Paid;
                        }
                        else
                        {
                            invoiceItem.PurchaseInvoiceItemState = purchaseInvoiceItemStates.PartiallyPaid;
                        }
                    }
                }

                //BaseOnDeriveInvoiceItems
                foreach (PurchaseInvoiceItem purchaseInvoiceItem in purchaseInvoice.ValidInvoiceItems)
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

                    //purchaseInvoiceItem.BaseOnDerivePrices();
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

                //BaseOnDeriveInvoiceTotals
                purchaseInvoice.TotalBasePrice = 0;
                purchaseInvoice.TotalDiscount = 0;
                purchaseInvoice.TotalSurcharge = 0;
                purchaseInvoice.TotalVat = 0;
                purchaseInvoice.TotalExVat = 0;
                purchaseInvoice.TotalIncVat = 0;
                purchaseInvoice.TotalIrpf = 0;
                purchaseInvoice.TotalExtraCharge = 0;
                purchaseInvoice.GrandTotal = 0;

                foreach (PurchaseInvoiceItem item in purchaseInvoice.PurchaseInvoiceItems)
                {
                    purchaseInvoice.TotalBasePrice += item.TotalBasePrice;
                    purchaseInvoice.TotalSurcharge += item.TotalSurcharge;
                    purchaseInvoice.TotalIrpf += item.TotalIrpf;
                    purchaseInvoice.TotalVat += item.TotalVat;
                    purchaseInvoice.TotalExVat += item.TotalExVat;
                    purchaseInvoice.TotalIncVat += item.TotalIncVat;
                    purchaseInvoice.GrandTotal += item.GrandTotal;
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

                foreach (OrderAdjustment orderAdjustment in purchaseInvoice.OrderAdjustments)
                {
                    if (orderAdjustment.GetType().Name.Equals(typeof(DiscountAdjustment).Name))
                    {
                        discount = orderAdjustment.Percentage.HasValue ?
                                        Math.Round(purchaseInvoice.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                        orderAdjustment.Amount ?? 0;

                        purchaseInvoice.TotalDiscount += discount;

                        if (purchaseInvoice.ExistVatRegime)
                        {
                            discountVat = Math.Round(discount * purchaseInvoice.VatRegime.VatRate.Rate / 100, 2);
                        }

                        if (purchaseInvoice.ExistIrpfRegime)
                        {
                            discountIrpf = Math.Round(discount * purchaseInvoice.IrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(SurchargeAdjustment).Name))
                    {
                        surcharge = orderAdjustment.Percentage.HasValue ?
                                            Math.Round(purchaseInvoice.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                            orderAdjustment.Amount ?? 0;

                        purchaseInvoice.TotalSurcharge += surcharge;

                        if (purchaseInvoice.ExistVatRegime)
                        {
                            surchargeVat = Math.Round(surcharge * purchaseInvoice.VatRegime.VatRate.Rate / 100, 2);
                        }

                        if (purchaseInvoice.ExistIrpfRegime)
                        {
                            surchargeIrpf = Math.Round(surcharge * purchaseInvoice.IrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(Fee).Name))
                    {
                        fee = orderAdjustment.Percentage.HasValue ?
                                    Math.Round(purchaseInvoice.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                    orderAdjustment.Amount ?? 0;

                        purchaseInvoice.TotalFee += fee;

                        if (purchaseInvoice.ExistVatRegime)
                        {
                            feeVat = Math.Round(fee * purchaseInvoice.VatRegime.VatRate.Rate / 100, 2);
                        }

                        if (purchaseInvoice.ExistIrpfRegime)
                        {
                            feeIrpf = Math.Round(fee * purchaseInvoice.IrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(ShippingAndHandlingCharge).Name))
                    {
                        shipping = orderAdjustment.Percentage.HasValue ?
                                        Math.Round(purchaseInvoice.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                        orderAdjustment.Amount ?? 0;

                        purchaseInvoice.TotalShippingAndHandling += shipping;

                        if (purchaseInvoice.ExistVatRegime)
                        {
                            shippingVat = Math.Round(shipping * purchaseInvoice.VatRegime.VatRate.Rate / 100, 2);
                        }

                        if (purchaseInvoice.ExistIrpfRegime)
                        {
                            shippingIrpf = Math.Round(shipping * purchaseInvoice.IrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(MiscellaneousCharge).Name))
                    {
                        miscellaneous = orderAdjustment.Percentage.HasValue ?
                                        Math.Round(purchaseInvoice.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                        orderAdjustment.Amount ?? 0;

                        purchaseInvoice.TotalExtraCharge += miscellaneous;

                        if (purchaseInvoice.ExistVatRegime)
                        {
                            miscellaneousVat = Math.Round(miscellaneous * purchaseInvoice.VatRegime.VatRate.Rate / 100, 2);
                        }

                        if (purchaseInvoice.ExistIrpfRegime)
                        {
                            miscellaneousIrpf = Math.Round(miscellaneous * purchaseInvoice.IrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }
                }

                purchaseInvoice.TotalExtraCharge = fee + shipping + miscellaneous;

                purchaseInvoice.TotalExVat = purchaseInvoice.TotalExVat - discount + surcharge + fee + shipping + miscellaneous;
                purchaseInvoice.TotalVat = purchaseInvoice.TotalVat - discountVat + surchargeVat + feeVat + shippingVat + miscellaneousVat;
                purchaseInvoice.TotalIncVat = purchaseInvoice.TotalIncVat - discount - discountVat + surcharge + surchargeVat + fee + feeVat + shipping + shippingVat + miscellaneous + miscellaneousVat;
                purchaseInvoice.TotalIrpf = purchaseInvoice.TotalIrpf + discountIrpf - surchargeIrpf - feeIrpf - shippingIrpf - miscellaneousIrpf;
                purchaseInvoice.GrandTotal = purchaseInvoice.TotalIncVat - purchaseInvoice.TotalIrpf;

                //DeriveWorkflow
                purchaseInvoice.WorkItemDescription = $"PurchaseInvoice: {purchaseInvoice.InvoiceNumber} [{purchaseInvoice.BilledFrom?.PartyName}]";

                if (purchaseInvoice.PurchaseInvoiceState.IsAwaitingApproval)
                {
                    if (!purchaseInvoice.OpenTasks.OfType<PurchaseInvoiceApproval>().Any())
                    {
                        var approval = new PurchaseInvoiceApprovalBuilder(purchaseInvoice.Session()).WithPurchaseInvoice(purchaseInvoice).Build();
                        approval.WorkItem = approval.PurchaseInvoice;
                    }
                }

                var singleton = purchaseInvoice.Session().GetSingleton();

                purchaseInvoice.AddSecurityToken(new SecurityTokens(purchaseInvoice.Session()).DefaultSecurityToken);

                //Sync
                foreach (PurchaseInvoiceItem invoiceItem in purchaseInvoice.PurchaseInvoiceItems)
                {
                    //invoiceItem.Sync(purchaseInvoice);
                    invoiceItem.SyncedInvoice = purchaseInvoice;
                }

                purchaseInvoice.ResetPrintDocument();

                var deletePermission = new Permissions(purchaseInvoice.Strategy.Session).Get(purchaseInvoice.Meta.ObjectType, purchaseInvoice.Meta.Delete, Operations.Execute);
                if (purchaseInvoice.IsDeletable)
                {
                    purchaseInvoice.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    purchaseInvoice.AddDeniedPermission(deletePermission);
                }

                if (!purchaseInvoice.ExistSalesInvoiceWherePurchaseInvoice
                    && (purchaseInvoice.BilledFrom as Organisation)?.IsInternalOrganisation == true
                    && (purchaseInvoice.PurchaseInvoiceState.IsPaid || purchaseInvoice.PurchaseInvoiceState.IsPartiallyPaid || purchaseInvoice.PurchaseInvoiceState.IsNotPaid))
                {
                    purchaseInvoice.RemoveDeniedPermission(new Permissions(purchaseInvoice.Strategy.Session).Get(purchaseInvoice.Meta.ObjectType, purchaseInvoice.Meta.CreateSalesInvoice, Operations.Execute));
                }
                else
                {
                    purchaseInvoice.AddDeniedPermission(new Permissions(purchaseInvoice.Strategy.Session).Get(purchaseInvoice.Meta.ObjectType, purchaseInvoice.Meta.CreateSalesInvoice, Operations.Execute));
                }
            }
        }
    }
}
