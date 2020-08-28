// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class SalesOrderCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdSalesOrder = changeSet.Created.Select(session.Instantiate).OfType<SalesOrder>();

                foreach (var salesOrder in createdSalesOrder)
                {
                    // SalesOrder Derivations and Validations
                    salesOrder.BillToCustomer ??= salesOrder.ShipToCustomer;
                    salesOrder.ShipToCustomer ??= salesOrder.BillToCustomer;
                    salesOrder.Customers = new[] { salesOrder.BillToCustomer, salesOrder.ShipToCustomer, salesOrder.PlacingCustomer };
                    salesOrder.Locale ??= salesOrder.BillToCustomer?.Locale ?? salesOrder.Strategy.Session.GetSingleton().DefaultLocale;
                    salesOrder.VatRegime ??= salesOrder.BillToCustomer?.VatRegime;
                    salesOrder.IrpfRegime ??= salesOrder.BillToCustomer?.IrpfRegime;
                    salesOrder.Currency ??= salesOrder.BillToCustomer?.PreferredCurrency ?? salesOrder.BillToCustomer?.Locale?.Country?.Currency ?? salesOrder.TakenBy?.PreferredCurrency;
                    salesOrder.TakenByContactMechanism ??= salesOrder.TakenBy?.OrderAddress ?? salesOrder.TakenBy?.BillingAddress ?? salesOrder.TakenBy?.GeneralCorrespondence;
                    salesOrder.BillToContactMechanism ??= salesOrder.BillToCustomer?.BillingAddress ?? salesOrder.BillToCustomer?.ShippingAddress ?? salesOrder.BillToCustomer?.GeneralCorrespondence;
                    salesOrder.BillToEndCustomerContactMechanism ??= salesOrder.BillToEndCustomer?.BillingAddress ?? salesOrder.BillToEndCustomer?.ShippingAddress ?? salesOrder.BillToCustomer?.GeneralCorrespondence;
                    salesOrder.ShipToEndCustomerAddress ??= salesOrder.ShipToEndCustomer?.ShippingAddress ?? salesOrder.ShipToCustomer?.GeneralCorrespondence as PostalAddress;
                    salesOrder.ShipFromAddress ??= salesOrder.TakenBy?.ShippingAddress;
                    salesOrder.ShipToAddress ??= salesOrder.ShipToCustomer?.ShippingAddress;
                    salesOrder.ShipmentMethod ??= salesOrder.ShipToCustomer?.DefaultShipmentMethod ?? salesOrder.Store.DefaultShipmentMethod;
                    salesOrder.PaymentMethod ??= salesOrder.ShipToCustomer?.PartyFinancialRelationshipsWhereFinancialParty?.FirstOrDefault(v => object.Equals(v.InternalOrganisation, salesOrder.TakenBy))?.DefaultPaymentMethod ?? salesOrder.Store.DefaultCollectionMethod;

                    if (!salesOrder.ExistOrderNumber && salesOrder.ExistStore)
                    {
                        salesOrder.OrderNumber = salesOrder.Store.NextSalesOrderNumber(salesOrder.OrderDate.Year);
                        salesOrder.SortableOrderNumber = salesOrder.Session().GetSingleton().SortableNumber(salesOrder.Store.SalesOrderNumberPrefix, salesOrder.OrderNumber, salesOrder.OrderDate.Year.ToString());
                    }

                    if (salesOrder.BillToCustomer?.BaseIsActiveCustomer(salesOrder.TakenBy, salesOrder.OrderDate) == false)
                    {
                        validation.AddError($"{salesOrder} {M.SalesOrder.BillToCustomer} {ErrorMessages.PartyIsNotACustomer}");
                    }

                    if (salesOrder.ShipToCustomer?.BaseIsActiveCustomer(salesOrder.TakenBy, salesOrder.OrderDate) == false)
                    {
                        validation.AddError($"{salesOrder} {M.SalesOrder.ShipToCustomer} {ErrorMessages.PartyIsNotACustomer}");
                    }

                    if (salesOrder.SalesOrderState.IsInProcess)
                    {
                        validation.AssertExists(salesOrder, salesOrder.Meta.ShipToAddress);
                        validation.AssertExists(salesOrder, salesOrder.Meta.BillToContactMechanism);
                    }

                    // SalesOrderItem Derivations and Validations
                    foreach (SalesOrderItem salesOrderItem in salesOrder.SalesOrderItems)
                    {
                        var salesOrderItemDerivedRoles = salesOrderItem;

                        salesOrderItem.ShipFromAddress ??= salesOrder.ShipFromAddress;
                        salesOrderItemDerivedRoles.ShipToAddress = salesOrderItem.AssignedShipToAddress ?? salesOrderItem.AssignedShipToParty?.ShippingAddress ?? salesOrder.ShipToAddress;
                        salesOrderItemDerivedRoles.ShipToParty = salesOrderItem.AssignedShipToParty ?? salesOrder.ShipToCustomer;
                        salesOrderItemDerivedRoles.DeliveryDate = salesOrderItem.AssignedDeliveryDate ?? salesOrder.DeliveryDate;
                        salesOrderItemDerivedRoles.VatRegime = salesOrderItem.AssignedVatRegime ?? salesOrder.VatRegime;
                        salesOrderItemDerivedRoles.VatRate = salesOrderItem.VatRegime?.VatRate;
                        salesOrderItemDerivedRoles.IrpfRegime = salesOrderItem.AssignedIrpfRegime ?? salesOrder.IrpfRegime;
                        salesOrderItemDerivedRoles.IrpfRate = salesOrderItem.IrpfRegime?.IrpfRate;

                        // TODO: Use versioning
                        if (salesOrderItem.ExistPreviousProduct && !salesOrderItem.PreviousProduct.Equals(salesOrderItem.Product))
                        {
                            validation.AddError($"{salesOrderItem} {M.SalesOrderItem.Product} {ErrorMessages.SalesOrderItemProductChangeNotAllowed}");
                        }
                        else
                        {
                            salesOrderItemDerivedRoles.PreviousProduct = salesOrderItem.Product;
                        }

                        if (salesOrderItem.ExistSalesOrderItemWhereOrderedWithFeature)
                        {
                            validation.AssertExists(salesOrderItem, M.SalesOrderItem.ProductFeature);
                            validation.AssertNotExists(salesOrderItem, M.SalesOrderItem.Product);
                        }
                        else
                        {
                            validation.AssertNotExists(salesOrderItem, M.SalesOrderItem.ProductFeature);
                        }

                        if (salesOrderItem.ExistProduct && salesOrderItem.ExistQuantityOrdered && salesOrderItem.QuantityOrdered < salesOrderItem.QuantityShipped)
                        {
                            validation.AddError($"{salesOrderItem} {M.SalesOrderItem.QuantityOrdered} {ErrorMessages.SalesOrderItemLessThanAlreadeyShipped}");
                        }

                        var isSubTotalItem = salesOrderItem.ExistInvoiceItemType && (salesOrderItem.InvoiceItemType.IsProductItem || salesOrderItem.InvoiceItemType.IsPartItem);
                        if (isSubTotalItem)
                        {
                            if (salesOrderItem.QuantityOrdered == 0)
                            {
                                validation.AddError($"{salesOrderItem} {M.SalesOrderItem.QuantityOrdered} QuantityOrdered is Required");
                            }
                        }
                        else
                        {
                            if (salesOrderItem.AssignedUnitPrice == 0)
                            {
                                validation.AddError($"{salesOrderItem} {M.SalesOrderItem.AssignedUnitPrice} Price is Required");
                            }
                        }

                        validation.AssertExistsAtMostOne(salesOrderItem, M.SalesOrderItem.Product, M.SalesOrderItem.ProductFeature);
                        validation.AssertExistsAtMostOne(salesOrderItem, M.SalesOrderItem.SerialisedItem, M.SalesOrderItem.ProductFeature);
                        validation.AssertExistsAtMostOne(salesOrderItem, M.SalesOrderItem.ReservedFromSerialisedInventoryItem, M.SalesOrderItem.ReservedFromNonSerialisedInventoryItem);
                        validation.AssertExistsAtMostOne(salesOrderItem, M.SalesOrderItem.AssignedUnitPrice, M.SalesOrderItem.DiscountAdjustments, M.SalesOrderItem.SurchargeAdjustments);
                    }

                    var validOrderItems = salesOrder.SalesOrderItems.Where(v => v.IsValid).ToArray();
                    salesOrder.ValidOrderItems = validOrderItems;

                    if (salesOrder.ExistVatRegime && salesOrder.VatRegime.ExistVatClause)
                    {
                        salesOrder.DerivedVatClause = salesOrder.VatRegime.VatClause;
                    }
                    else
                    {
                        string TakenbyCountry = null;

                        if (salesOrder.TakenBy.PartyContactMechanisms?.FirstOrDefault(v => v.ContactPurposes.Any(p => Equals(p, new ContactMechanismPurposes(session).RegisteredOffice)))?.ContactMechanism is PostalAddress registeredOffice)
                        {
                            TakenbyCountry = registeredOffice.Country.IsoCode;
                        }

                        var OutsideEUCustomer = salesOrder.BillToCustomer?.VatRegime?.Equals(new VatRegimes(session).Export);
                        var shipFromBelgium = salesOrder.ValidOrderItems?.Cast<SalesOrderItem>().All(v => Equals("BE", v.ShipFromAddress?.Country?.IsoCode));
                        var shipToEU = salesOrder.ValidOrderItems?.Cast<SalesOrderItem>().Any(v => Equals(true, v.ShipToAddress?.Country?.EuMemberState));
                        var sellerResponsibleForTransport = salesOrder.SalesTerms.Any(v => Equals(v.TermType, new IncoTermTypes(session).Cif) || Equals(v.TermType, new IncoTermTypes(session).Cfr));
                        var buyerResponsibleForTransport = salesOrder.SalesTerms.Any(v => Equals(v.TermType, new IncoTermTypes(session).Exw));

                        if (Equals(salesOrder.VatRegime, new VatRegimes(session).ServiceB2B))
                        {
                            salesOrder.DerivedVatClause = new VatClauses(session).ServiceB2B;
                        }
                        else if (Equals(salesOrder.VatRegime, new VatRegimes(session).IntraCommunautair))
                        {
                            salesOrder.DerivedVatClause = new VatClauses(session).Intracommunautair;
                        }
                        else if (TakenbyCountry == "BE"
                                 && OutsideEUCustomer.HasValue && OutsideEUCustomer.Value
                                 && shipFromBelgium.HasValue && shipFromBelgium.Value
                                 && shipToEU.HasValue && shipToEU.Value == false)
                        {
                            if (sellerResponsibleForTransport)
                            {
                                // You sell goods to a customer out of the EU and the goods are being sold and transported from Belgium to another country out of the EU and you transport the goods and importer is the customer
                                salesOrder.DerivedVatClause = new VatClauses(session).BeArt39Par1Item1;
                            }
                            else if (buyerResponsibleForTransport)
                            {
                                // You sell goods to a customer out of the EU and the goods are being sold and transported from Belgium to another country out of the EU  and the customer does the transport of the goods and importer is the customer
                                salesOrder.DerivedVatClause = new VatClauses(session).BeArt39Par1Item2;
                            }
                        }
                    }

                    salesOrder.DerivedVatClause = salesOrder.ExistAssignedVatClause ? salesOrder.AssignedVatClause : salesOrder.DerivedVatClause;

                    var salesOrderShipmentStates = new SalesOrderShipmentStates(salesOrder.Strategy.Session);
                    var salesOrderPaymentStates = new SalesOrderPaymentStates(salesOrder.Strategy.Session);
                    var salesOrderInvoiceStates = new SalesOrderInvoiceStates(salesOrder.Strategy.Session);

                    var salesOrderItemShipmentStates = new SalesOrderItemShipmentStates(session);
                    var salesOrderItemPaymentStates = new SalesOrderItemPaymentStates(session);
                    var salesOrderItemInvoiceStates = new SalesOrderItemInvoiceStates(session);

                    // SalesOrder Shipment State
                    if (validOrderItems.Any())
                    {
                        if (validOrderItems.All(v => v.SalesOrderItemShipmentState.Shipped))
                        {
                            salesOrder.SalesOrderShipmentState = salesOrderShipmentStates.Shipped;
                        }
                        else if (validOrderItems.All(v => v.SalesOrderItemShipmentState.NotShipped))
                        {
                            salesOrder.SalesOrderShipmentState = salesOrderShipmentStates.NotShipped;
                        }
                        else if (validOrderItems.Any(v => v.SalesOrderItemShipmentState.InProgress))
                        {
                            salesOrder.SalesOrderShipmentState = salesOrderShipmentStates.InProgress;
                        }
                        else
                        {
                            salesOrder.SalesOrderShipmentState = salesOrderShipmentStates.PartiallyShipped;
                        }

                        // SalesOrder Payment State
                        if (validOrderItems.All(v => v.SalesOrderItemPaymentState.Paid))
                        {
                            salesOrder.SalesOrderPaymentState = salesOrderPaymentStates.Paid;
                        }
                        else if (validOrderItems.All(v => v.SalesOrderItemPaymentState.NotPaid))
                        {
                            salesOrder.SalesOrderPaymentState = salesOrderPaymentStates.NotPaid;
                        }
                        else
                        {
                            salesOrder.SalesOrderPaymentState = salesOrderPaymentStates.PartiallyPaid;
                        }

                        // SalesOrder Invoice State
                        if (validOrderItems.All(v => v.SalesOrderItemInvoiceState.Invoiced))
                        {
                            salesOrder.SalesOrderInvoiceState = salesOrderInvoiceStates.Invoiced;
                        }
                        else if (validOrderItems.All(v => v.SalesOrderItemInvoiceState.NotInvoiced))
                        {
                            salesOrder.SalesOrderInvoiceState = salesOrderInvoiceStates.NotInvoiced;
                        }
                        else
                        {
                            salesOrder.SalesOrderInvoiceState = salesOrderInvoiceStates.PartiallyInvoiced;
                        }

                        // SalesOrder OrderState
                        if (salesOrder.SalesOrderShipmentState.Shipped && salesOrder.SalesOrderInvoiceState.Invoiced)
                        {
                            salesOrder.SalesOrderState = new SalesOrderStates(salesOrder.Strategy.Session).Completed;
                        }

                        if (salesOrder.SalesOrderState.IsCompleted && salesOrder.SalesOrderPaymentState.Paid)
                        {
                            salesOrder.SalesOrderState = new SalesOrderStates(salesOrder.Strategy.Session).Finished;
                        }
                    }

                    // TODO: Move to versioning
                    salesOrder.PreviousBillToCustomer = salesOrder.BillToCustomer;
                    salesOrder.PreviousShipToCustomer = salesOrder.ShipToCustomer;

                    var singleton = session.GetSingleton();

                    salesOrder.AddSecurityToken(new SecurityTokens(session).DefaultSecurityToken);

                    salesOrder.ResetPrintDocument();

                    // CanShip
                    if (salesOrder.SalesOrderState.Equals(new SalesOrderStates(salesOrder.Strategy.Session).InProcess))
                    {
                        var somethingToShip = false;
                        var allItemsAvailable = true;

                        foreach (var salesOrderItem1 in validOrderItems)
                        {
                            if (!salesOrder.PartiallyShip && salesOrderItem1.QuantityRequestsShipping != salesOrderItem1.QuantityOrdered)
                            {
                                allItemsAvailable = false;
                                break;
                            }

                            if (salesOrder.PartiallyShip && salesOrderItem1.QuantityRequestsShipping > 0)
                            {
                                somethingToShip = true;
                            }
                        }

                        salesOrder.CanShip = (!salesOrder.PartiallyShip && allItemsAvailable) || somethingToShip;
                    }
                    else
                    {
                        salesOrder.CanShip = false;
                    }

                    // CanInvoice
                    if (salesOrder.SalesOrderState.IsInProcess && object.Equals(salesOrder.Store.BillingProcess, new BillingProcesses(salesOrder.Strategy.Session).BillingForOrderItems))
                    {
                        salesOrder.CanInvoice = false;

                        foreach (var orderItem2 in validOrderItems)
                        {
                            var amountAlreadyInvoiced1 = orderItem2.OrderItemBillingsWhereOrderItem.Sum(v => v.Amount);

                            var leftToInvoice1 = (orderItem2.QuantityOrdered * orderItem2.UnitPrice) - amountAlreadyInvoiced1;

                            if (leftToInvoice1 > 0)
                            {
                                salesOrder.CanInvoice = true;
                            }
                        }
                    }
                    else
                    {
                        salesOrder.CanInvoice = false;
                    }

                    if (salesOrder.SalesOrderState.Equals(new SalesOrderStates(salesOrder.Strategy.Session).InProcess) &&
                        Equals(salesOrder.Store.BillingProcess, new BillingProcesses(salesOrder.Strategy.Session).BillingForShipmentItems))
                    {
                        salesOrder.RemoveDeniedPermission(new Permissions(salesOrder.Strategy.Session).Get(salesOrder.Meta.Class, salesOrder.Meta.Invoice, Operations.Execute));
                    }

                    if (salesOrder.CanShip && salesOrder.Store.AutoGenerateCustomerShipment)
                    {
                        salesOrder.Ship();
                    }

                    if (salesOrder.SalesOrderState.IsInProcess
                        && (!salesOrder.ExistLastSalesOrderState || !salesOrder.LastSalesOrderState.IsInProcess)
                        && salesOrder.TakenBy.SerialisedItemSoldOns.Contains(new SerialisedItemSoldOns(salesOrder.Session()).SalesOrderAccept))
                    {
                        foreach (SalesOrderItem item in salesOrder.ValidOrderItems.Where(v => ((SalesOrderItem)v).ExistSerialisedItem))
                        {
                            if (item.ExistNextSerialisedItemAvailability)
                            {
                                item.SerialisedItem.SerialisedItemAvailability = item.NextSerialisedItemAvailability;

                                if (item.NextSerialisedItemAvailability.Equals(new SerialisedItemAvailabilities(salesOrder.Session()).Sold))
                                {
                                    item.SerialisedItem.OwnedBy = salesOrder.ShipToCustomer;
                                    item.SerialisedItem.Ownership = new Ownerships(salesOrder.Session()).ThirdParty;
                                }
                            }

                            item.SerialisedItem.AvailableForSale = false;
                        }
                    }

                    Sync(validOrderItems, salesOrder);

                    if (salesOrder.CanShip)
                    {
                        salesOrder.RemoveDeniedPermission(new Permissions(salesOrder.Strategy.Session).Get(salesOrder.Meta.Class, salesOrder.Meta.Ship, Operations.Execute));
                    }
                    else
                    {
                        salesOrder.AddDeniedPermission(new Permissions(salesOrder.Strategy.Session).Get(salesOrder.Meta.Class, salesOrder.Meta.Ship, Operations.Execute));
                    }

                    if (salesOrder.CanInvoice)
                    {
                        salesOrder.RemoveDeniedPermission(new Permissions(salesOrder.Strategy.Session).Get(salesOrder.Meta.Class, salesOrder.Meta.Invoice, Operations.Execute));
                    }
                    else
                    {
                        salesOrder.AddDeniedPermission(new Permissions(salesOrder.Strategy.Session).Get(salesOrder.Meta.Class, salesOrder.Meta.Invoice, Operations.Execute));
                    }

                    if (!salesOrder.SalesOrderInvoiceState.NotInvoiced || !salesOrder.SalesOrderShipmentState.NotShipped)
                    {
                        salesOrder.AddDeniedPermission(new Permissions(salesOrder.Strategy.Session).Get(salesOrder.Meta.Class, salesOrder.Meta.SetReadyForPosting, Operations.Execute));
                        salesOrder.AddDeniedPermission(new Permissions(salesOrder.Strategy.Session).Get(salesOrder.Meta.Class, salesOrder.Meta.Post, Operations.Execute));
                        salesOrder.AddDeniedPermission(new Permissions(salesOrder.Strategy.Session).Get(salesOrder.Meta.Class, salesOrder.Meta.Reopen, Operations.Execute));
                        salesOrder.AddDeniedPermission(new Permissions(salesOrder.Strategy.Session).Get(salesOrder.Meta.Class, salesOrder.Meta.Approve, Operations.Execute));
                        salesOrder.AddDeniedPermission(new Permissions(salesOrder.Strategy.Session).Get(salesOrder.Meta.Class, salesOrder.Meta.Hold, Operations.Execute));
                        salesOrder.AddDeniedPermission(new Permissions(salesOrder.Strategy.Session).Get(salesOrder.Meta.Class, salesOrder.Meta.Continue, Operations.Execute));
                        salesOrder.AddDeniedPermission(new Permissions(salesOrder.Strategy.Session).Get(salesOrder.Meta.Class, salesOrder.Meta.Accept, Operations.Execute));
                        salesOrder.AddDeniedPermission(new Permissions(salesOrder.Strategy.Session).Get(salesOrder.Meta.Class, salesOrder.Meta.Revise, Operations.Execute));
                        salesOrder.AddDeniedPermission(new Permissions(salesOrder.Strategy.Session).Get(salesOrder.Meta.Class, salesOrder.Meta.Complete, Operations.Execute));
                        salesOrder.AddDeniedPermission(new Permissions(salesOrder.Strategy.Session).Get(salesOrder.Meta.Class, salesOrder.Meta.Reject, Operations.Execute));
                        salesOrder.AddDeniedPermission(new Permissions(salesOrder.Strategy.Session).Get(salesOrder.Meta.Class, salesOrder.Meta.Cancel, Operations.Execute));

                        var deniablePermissionByOperandTypeId = new Dictionary<Guid, Permission>();

                        foreach (Permission permission in salesOrder.Session().Extent<Permission>())
                        {
                            if (permission.ConcreteClassPointer == salesOrder.Strategy.Class.Id && permission.Operation == Operations.Write)
                            {
                                deniablePermissionByOperandTypeId.Add(permission.OperandTypePointer, permission);
                            }
                        }

                        foreach (var keyValuePair in deniablePermissionByOperandTypeId)
                        {
                            salesOrder.AddDeniedPermission(keyValuePair.Value);
                        }
                    }

                    var deletePermission = new Permissions(salesOrder.Strategy.Session).Get(salesOrder.Meta.ObjectType, salesOrder.Meta.Delete, Operations.Execute);
                    if (salesOrder.IsDeletable)
                    {
                        salesOrder.RemoveDeniedPermission(deletePermission);
                    }
                    else
                    {
                        salesOrder.AddDeniedPermission(deletePermission);
                    }

                    if (salesOrder.HasChangedStates())
                    {
                        //derivation.Mark(this);
                    }
                }

                void Sync(SalesOrderItem[] validOrderItems, SalesOrder salesOrder)
                {
                    // Second run to calculate price (because of order value break)
                    foreach (var salesOrderItem in validOrderItems)
                    {
                        foreach (SalesOrderItem featureItem in salesOrderItem.OrderedWithFeatures)
                        {
                            SyncPrices(featureItem, salesOrder, true);
                        }

                        salesOrderItem.Sync(salesOrder);
                        SyncPrices(salesOrderItem, salesOrder, true);
                    }

                    // Calculate Totals
                    salesOrder.TotalBasePrice = 0;
                    salesOrder.TotalDiscount = 0;
                    salesOrder.TotalSurcharge = 0;
                    salesOrder.TotalExVat = 0;
                    salesOrder.TotalFee = 0;
                    salesOrder.TotalShippingAndHandling = 0;
                    salesOrder.TotalExtraCharge = 0;
                    salesOrder.TotalVat = 0;
                    salesOrder.TotalIrpf = 0;
                    salesOrder.TotalIncVat = 0;
                    salesOrder.TotalListPrice = 0;
                    salesOrder.GrandTotal = 0;

                    foreach (var item in validOrderItems)
                    {
                        if (!item.ExistSalesOrderItemWhereOrderedWithFeature)
                        {
                            salesOrder.TotalBasePrice += item.TotalBasePrice;
                            salesOrder.TotalDiscount += item.TotalDiscount;
                            salesOrder.TotalSurcharge += item.TotalSurcharge;
                            salesOrder.TotalExVat += item.TotalExVat;
                            salesOrder.TotalVat += item.TotalVat;
                            salesOrder.TotalIrpf += item.TotalIrpf;
                            salesOrder.TotalIncVat += item.TotalIncVat;
                            salesOrder.TotalListPrice += item.TotalExVat;
                            salesOrder.GrandTotal += item.GrandTotal;
                        }
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

                    foreach (OrderAdjustment orderAdjustment in salesOrder.OrderAdjustments)
                    {
                        if (orderAdjustment.GetType().Name.Equals(typeof(DiscountAdjustment).Name))
                        {
                            discount = orderAdjustment.Percentage.HasValue ?
                                            Math.Round(salesOrder.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                            orderAdjustment.Amount ?? 0;

                            salesOrder.TotalDiscount += discount;

                            if (salesOrder.ExistVatRegime)
                            {
                                discountVat = Math.Round(discount * salesOrder.VatRegime.VatRate.Rate / 100, 2);
                            }

                            if (salesOrder.ExistIrpfRegime)
                            {
                                discountIrpf = Math.Round(discount * salesOrder.IrpfRegime.IrpfRate.Rate / 100, 2);
                            }
                        }

                        if (orderAdjustment.GetType().Name.Equals(typeof(SurchargeAdjustment).Name))
                        {
                            surcharge = orderAdjustment.Percentage.HasValue ?
                                                Math.Round(salesOrder.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                                orderAdjustment.Amount ?? 0;

                            salesOrder.TotalSurcharge += surcharge;

                            if (salesOrder.ExistVatRegime)
                            {
                                surchargeVat = Math.Round(surcharge * salesOrder.VatRegime.VatRate.Rate / 100, 2);
                            }

                            if (salesOrder.ExistIrpfRegime)
                            {
                                surchargeIrpf = Math.Round(surcharge * salesOrder.IrpfRegime.IrpfRate.Rate / 100, 2);
                            }
                        }

                        if (orderAdjustment.GetType().Name.Equals(typeof(Fee).Name))
                        {
                            fee = orderAdjustment.Percentage.HasValue ?
                                        Math.Round(salesOrder.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                        orderAdjustment.Amount ?? 0;

                            salesOrder.TotalFee += fee;

                            if (salesOrder.ExistVatRegime)
                            {
                                feeVat = Math.Round(fee * salesOrder.VatRegime.VatRate.Rate / 100, 2);
                            }

                            if (salesOrder.ExistIrpfRegime)
                            {
                                feeIrpf = Math.Round(fee * salesOrder.IrpfRegime.IrpfRate.Rate / 100, 2);
                            }
                        }

                        if (orderAdjustment.GetType().Name.Equals(typeof(ShippingAndHandlingCharge).Name))
                        {
                            shipping = orderAdjustment.Percentage.HasValue ?
                                            Math.Round(salesOrder.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                            orderAdjustment.Amount ?? 0;

                            salesOrder.TotalShippingAndHandling += shipping;

                            if (salesOrder.ExistVatRegime)
                            {
                                shippingVat = Math.Round(shipping * salesOrder.VatRegime.VatRate.Rate / 100, 2);
                            }

                            if (salesOrder.ExistIrpfRegime)
                            {
                                shippingIrpf = Math.Round(shipping * salesOrder.IrpfRegime.IrpfRate.Rate / 100, 2);
                            }
                        }

                        if (orderAdjustment.GetType().Name.Equals(typeof(MiscellaneousCharge).Name))
                        {
                            miscellaneous = orderAdjustment.Percentage.HasValue ?
                                            Math.Round(salesOrder.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                            orderAdjustment.Amount ?? 0;

                            salesOrder.TotalExtraCharge += miscellaneous;

                            if (salesOrder.ExistVatRegime)
                            {
                                miscellaneousVat = Math.Round(miscellaneous * salesOrder.VatRegime.VatRate.Rate / 100, 2);
                            }

                            if (salesOrder.ExistIrpfRegime)
                            {
                                miscellaneousIrpf = Math.Round(miscellaneous * salesOrder.IrpfRegime.IrpfRate.Rate / 100, 2);
                            }
                        }
                    }

                    salesOrder.TotalExtraCharge = fee + shipping + miscellaneous;

                    salesOrder.TotalExVat = salesOrder.TotalExVat - discount + surcharge + fee + shipping + miscellaneous;
                    salesOrder.TotalVat = salesOrder.TotalVat - discountVat + surchargeVat + feeVat + shippingVat + miscellaneousVat;
                    salesOrder.TotalIncVat = salesOrder.TotalIncVat - discount - discountVat + surcharge + surchargeVat + fee + feeVat + shipping + shippingVat + miscellaneous + miscellaneousVat;
                    salesOrder.TotalIrpf = salesOrder.TotalIrpf + discountIrpf - surchargeIrpf - feeIrpf - shippingIrpf - miscellaneousIrpf;
                    salesOrder.GrandTotal = salesOrder.TotalIncVat - salesOrder.TotalIrpf;

                    //// Only take into account items for which there is data at the item level.
                    //// Skip negative sales.
                    decimal totalUnitBasePrice = 0;
                    decimal totalListPrice = 0;

                    foreach (var item1 in validOrderItems)
                    {
                        if (item1.TotalExVat > 0)
                        {
                            totalUnitBasePrice += item1.UnitBasePrice;
                            totalListPrice += item1.UnitPrice;
                        }
                    }
                }

                void SyncPrices(SalesOrderItem salesOrderItem, SalesOrder salesOrder, bool useValueOrdered = false)
                {
                    var sameProductItems = salesOrder.SalesOrderItems
                .Where(v => v.IsValid && v.ExistProduct && v.Product.Equals(salesOrderItem.Product))
                .ToArray();

                    var quantityOrdered = sameProductItems.Sum(w => w.QuantityOrdered);
                    var valueOrdered = useValueOrdered ? sameProductItems.Sum(w => w.TotalBasePrice) : 0;

                    var orderPriceComponents = new PriceComponents(salesOrderItem.Session()).CurrentPriceComponents(salesOrder.OrderDate);
                    var orderItemPriceComponents = Array.Empty<PriceComponent>();
                    if (salesOrderItem.ExistProduct)
                    {
                        orderItemPriceComponents = salesOrderItem.Product.GetPriceComponents(orderPriceComponents);
                    }
                    else if (salesOrderItem.ExistProductFeature)
                    {
                        orderItemPriceComponents = salesOrderItem.ProductFeature.GetPriceComponents(salesOrderItem.SalesOrderItemWhereOrderedWithFeature.Product, orderPriceComponents);
                    }

                    var priceComponents = orderItemPriceComponents.Where(
                        v => PriceComponents.BaseIsApplicable(
                            new PriceComponents.IsApplicable
                            {
                                PriceComponent = v,
                                Customer = salesOrder.BillToCustomer,
                                Product = salesOrderItem.Product,
                                SalesOrder = salesOrder,
                                QuantityOrdered = quantityOrdered,
                                ValueOrdered = valueOrdered,
                            })).ToArray();

                    var unitBasePrice = priceComponents.OfType<BasePrice>().Min(v => v.Price);

                    // Calculate Unit Price (with Discounts and Surcharges)
                    if (salesOrderItem.AssignedUnitPrice.HasValue)
                    {
                        salesOrderItem.UnitBasePrice = unitBasePrice ?? salesOrderItem.AssignedUnitPrice.Value;
                        salesOrderItem.UnitDiscount = 0;
                        salesOrderItem.UnitSurcharge = 0;
                        salesOrderItem.UnitPrice = salesOrderItem.AssignedUnitPrice.Value;
                    }
                    else
                    {
                        if (!unitBasePrice.HasValue)
                        {
                            validation.AddError($"{salesOrderItem} {M.SalesOrderItem.UnitBasePrice} No BasePrice with a Price");
                            return;
                        }

                        salesOrderItem.UnitBasePrice = unitBasePrice.Value;

                        salesOrderItem.UnitDiscount = priceComponents.OfType<DiscountComponent>().Sum(
                            v => v.Percentage.HasValue
                                ? Math.Round(salesOrderItem.UnitBasePrice * v.Percentage.Value / 100, 2)
                                : v.Price ?? 0);

                        salesOrderItem.UnitSurcharge = priceComponents.OfType<SurchargeComponent>().Sum(
                            v => v.Percentage.HasValue
                                ? Math.Round(salesOrderItem.UnitBasePrice * v.Percentage.Value / 100, 2)
                                : v.Price ?? 0);

                        salesOrderItem.UnitPrice = salesOrderItem.UnitBasePrice - salesOrderItem.UnitDiscount + salesOrderItem.UnitSurcharge;

                        foreach (OrderAdjustment orderAdjustment in salesOrderItem.DiscountAdjustments)
                        {
                            salesOrderItem.UnitDiscount += orderAdjustment.Percentage.HasValue
                                ? Math.Round(salesOrderItem.UnitPrice * orderAdjustment.Percentage.Value / 100, 2)
                                : orderAdjustment.Amount ?? 0;
                        }

                        foreach (OrderAdjustment orderAdjustment in salesOrderItem.SurchargeAdjustments)
                        {
                            salesOrderItem.UnitSurcharge += orderAdjustment.Percentage.HasValue
                                ? Math.Round(salesOrderItem.UnitPrice * orderAdjustment.Percentage.Value / 100, 2)
                                : orderAdjustment.Amount ?? 0;
                        }

                        salesOrderItem.UnitPrice = salesOrderItem.UnitBasePrice - salesOrderItem.UnitDiscount + salesOrderItem.UnitSurcharge;
                    }

                    foreach (SalesOrderItem featureItem in salesOrderItem.OrderedWithFeatures)
                    {
                        salesOrderItem.UnitBasePrice += featureItem.UnitBasePrice;
                        salesOrderItem.UnitPrice += featureItem.UnitPrice;
                        salesOrderItem.UnitDiscount += featureItem.UnitDiscount;
                        salesOrderItem.UnitSurcharge += featureItem.UnitSurcharge;
                    }

                    salesOrderItem.UnitVat = salesOrderItem.ExistVatRate ? salesOrderItem.UnitPrice * salesOrderItem.VatRate.Rate / 100 : 0;
                    salesOrderItem.UnitIrpf = salesOrderItem.ExistIrpfRate ? salesOrderItem.UnitPrice * salesOrderItem.IrpfRate.Rate / 100 : 0;

                    // Calculate Totals
                    salesOrderItem.TotalBasePrice = salesOrderItem.UnitBasePrice * salesOrderItem.QuantityOrdered;
                    salesOrderItem.TotalDiscount = salesOrderItem.UnitDiscount * salesOrderItem.QuantityOrdered;
                    salesOrderItem.TotalSurcharge = salesOrderItem.UnitSurcharge * salesOrderItem.QuantityOrdered;
                    salesOrderItem.TotalOrderAdjustment = salesOrderItem.TotalSurcharge - salesOrderItem.TotalDiscount;

                    if (salesOrderItem.TotalBasePrice > 0)
                    {
                        salesOrderItem.TotalDiscountAsPercentage = Math.Round(salesOrderItem.TotalDiscount / salesOrderItem.TotalBasePrice * 100, 2);
                        salesOrderItem.TotalSurchargeAsPercentage = Math.Round(salesOrderItem.TotalSurcharge / salesOrderItem.TotalBasePrice * 100, 2);
                    }
                    else
                    {
                        salesOrderItem.TotalDiscountAsPercentage = 0;
                        salesOrderItem.TotalSurchargeAsPercentage = 0;
                    }

                    salesOrderItem.TotalExVat = salesOrderItem.UnitPrice * salesOrderItem.QuantityOrdered;
                    salesOrderItem.TotalVat = salesOrderItem.UnitVat * salesOrderItem.QuantityOrdered;
                    salesOrderItem.TotalIncVat = salesOrderItem.TotalExVat + salesOrderItem.TotalVat;
                    salesOrderItem.TotalIrpf = salesOrderItem.UnitIrpf * salesOrderItem.QuantityOrdered;
                    salesOrderItem.GrandTotal = salesOrderItem.TotalIncVat - salesOrderItem.TotalIrpf;
                }
            }
        }

        public static void SalesOrderRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("dba2ce35-8659-4e89-8a0b-4aafc95cd5e2")] = new SalesOrderCreationDerivation();
        }
    }
}
