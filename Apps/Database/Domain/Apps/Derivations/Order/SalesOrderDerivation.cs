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

    public class SalesOrderDerivation : DomainDerivation
    {
        public SalesOrderDerivation(M m) : base(m, new Guid("CC43279A-22B4-499E-9ADA-33364E30FBD4")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.SalesOrder.Class),
                new ChangedPattern(this.M.SalesOrder.SalesOrderState),
                new ChangedPattern(this.M.SalesOrder.SalesOrderItems)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<SalesOrder>())
            {
                // SalesOrder Derivations and Validations
                @this.BillToCustomer ??= @this.ShipToCustomer;
                @this.ShipToCustomer ??= @this.BillToCustomer;
                @this.Customers = new[] { @this.BillToCustomer, @this.ShipToCustomer, @this.PlacingCustomer };
                @this.Locale ??= @this.BillToCustomer?.Locale ?? @this.Strategy.Session.GetSingleton().DefaultLocale;
                @this.VatRegime ??= @this.BillToCustomer?.VatRegime;
                @this.IrpfRegime ??= @this.BillToCustomer?.IrpfRegime;
                @this.Currency ??= @this.BillToCustomer?.PreferredCurrency ?? @this.BillToCustomer?.Locale?.Country?.Currency ?? @this.TakenBy?.PreferredCurrency;
                @this.TakenByContactMechanism ??= @this.TakenBy?.OrderAddress ?? @this.TakenBy?.BillingAddress ?? @this.TakenBy?.GeneralCorrespondence;
                @this.BillToContactMechanism ??= @this.BillToCustomer?.BillingAddress ?? @this.BillToCustomer?.ShippingAddress ?? @this.BillToCustomer?.GeneralCorrespondence;
                @this.BillToEndCustomerContactMechanism ??= @this.BillToEndCustomer?.BillingAddress ?? @this.BillToEndCustomer?.ShippingAddress ?? @this.BillToCustomer?.GeneralCorrespondence;
                @this.ShipToEndCustomerAddress ??= @this.ShipToEndCustomer?.ShippingAddress ?? @this.ShipToCustomer?.GeneralCorrespondence as PostalAddress;
                @this.ShipFromAddress ??= @this.TakenBy?.ShippingAddress;
                @this.ShipToAddress ??= @this.ShipToCustomer?.ShippingAddress;
                @this.ShipmentMethod ??= @this.ShipToCustomer?.DefaultShipmentMethod ?? @this.Store.DefaultShipmentMethod;
                @this.PaymentMethod ??= @this.ShipToCustomer?.PartyFinancialRelationshipsWhereFinancialParty?.FirstOrDefault(v => object.Equals(v.InternalOrganisation, @this.TakenBy))?.DefaultPaymentMethod ?? @this.Store.DefaultCollectionMethod;

                if (!@this.ExistOrderNumber && @this.ExistStore)
                {
                    @this.OrderNumber = @this.Store.NextSalesOrderNumber(@this.OrderDate.Year);
                    @this.SortableOrderNumber = @this.Session().GetSingleton().SortableNumber(@this.Store.SalesOrderNumberPrefix, @this.OrderNumber, @this.OrderDate.Year.ToString());
                }

                if (@this.BillToCustomer?.AppsIsActiveCustomer(@this.TakenBy, @this.OrderDate) == false)
                {
                    validation.AddError($"{@this} {this.M.SalesOrder.BillToCustomer} {ErrorMessages.PartyIsNotACustomer}");
                }

                if (@this.ShipToCustomer?.AppsIsActiveCustomer(@this.TakenBy, @this.OrderDate) == false)
                {
                    validation.AddError($"{@this} {this.M.SalesOrder.ShipToCustomer} {ErrorMessages.PartyIsNotACustomer}");
                }

                if (@this.SalesOrderState.IsInProcess)
                {
                    validation.AssertExists(@this, @this.Meta.ShipToAddress);
                    validation.AssertExists(@this, @this.Meta.BillToContactMechanism);
                }

                // SalesOrderItem Derivations and Validations
                foreach (SalesOrderItem salesOrderItem in @this.SalesOrderItems)
                {
                    var salesOrderItemDerivedRoles = salesOrderItem;

                    salesOrderItem.ShipFromAddress ??= @this.ShipFromAddress;
                    salesOrderItemDerivedRoles.ShipToAddress = salesOrderItem.AssignedShipToAddress ?? salesOrderItem.AssignedShipToParty?.ShippingAddress ?? @this.ShipToAddress;
                    salesOrderItemDerivedRoles.ShipToParty = salesOrderItem.AssignedShipToParty ?? @this.ShipToCustomer;
                    salesOrderItemDerivedRoles.DeliveryDate = salesOrderItem.AssignedDeliveryDate ?? @this.DeliveryDate;
                    salesOrderItemDerivedRoles.VatRegime = salesOrderItem.AssignedVatRegime ?? @this.VatRegime;
                    salesOrderItemDerivedRoles.VatRate = salesOrderItem.VatRegime?.VatRate;
                    salesOrderItemDerivedRoles.IrpfRegime = salesOrderItem.AssignedIrpfRegime ?? @this.IrpfRegime;
                    salesOrderItemDerivedRoles.IrpfRate = salesOrderItem.IrpfRegime?.IrpfRate;

                    // TODO: Use versioning
                    if (salesOrderItem.ExistPreviousProduct && !salesOrderItem.PreviousProduct.Equals(salesOrderItem.Product))
                    {
                        validation.AddError($"{salesOrderItem} {this.M.SalesOrderItem.Product} {ErrorMessages.SalesOrderItemProductChangeNotAllowed}");
                    }
                    else
                    {
                        salesOrderItemDerivedRoles.PreviousProduct = salesOrderItem.Product;
                    }

                    if (salesOrderItem.ExistSalesOrderItemWhereOrderedWithFeature)
                    {
                        validation.AssertExists(salesOrderItem, this.M.SalesOrderItem.ProductFeature);
                        validation.AssertNotExists(salesOrderItem, this.M.SalesOrderItem.Product);
                    }
                    else
                    {
                        validation.AssertNotExists(salesOrderItem, this.M.SalesOrderItem.ProductFeature);
                    }

                    if (salesOrderItem.ExistProduct && salesOrderItem.ExistQuantityOrdered && salesOrderItem.QuantityOrdered < salesOrderItem.QuantityShipped)
                    {
                        validation.AddError($"{salesOrderItem} {this.M.SalesOrderItem.QuantityOrdered} {ErrorMessages.SalesOrderItemLessThanAlreadeyShipped}");
                    }

                    var isSubTotalItem = salesOrderItem.ExistInvoiceItemType && (salesOrderItem.InvoiceItemType.IsProductItem || salesOrderItem.InvoiceItemType.IsPartItem);
                    if (isSubTotalItem)
                    {
                        if (salesOrderItem.QuantityOrdered == 0)
                        {
                            validation.AddError($"{salesOrderItem} {this.M.SalesOrderItem.QuantityOrdered} QuantityOrdered is Required");
                        }
                    }
                    else
                    {
                        if (salesOrderItem.AssignedUnitPrice == 0)
                        {
                            validation.AddError($"{salesOrderItem} {this.M.SalesOrderItem.AssignedUnitPrice} Price is Required");
                        }
                    }

                    validation.AssertExistsAtMostOne(salesOrderItem, this.M.SalesOrderItem.Product, this.M.SalesOrderItem.ProductFeature);
                    validation.AssertExistsAtMostOne(salesOrderItem, this.M.SalesOrderItem.SerialisedItem, this.M.SalesOrderItem.ProductFeature);
                    validation.AssertExistsAtMostOne(salesOrderItem, this.M.SalesOrderItem.ReservedFromSerialisedInventoryItem, this.M.SalesOrderItem.ReservedFromNonSerialisedInventoryItem);
                    validation.AssertExistsAtMostOne(salesOrderItem, this.M.SalesOrderItem.AssignedUnitPrice, this.M.SalesOrderItem.DiscountAdjustments, this.M.SalesOrderItem.SurchargeAdjustments);
                }

                var validOrderItems = @this.SalesOrderItems.Where(v => v.IsValid).ToArray();
                @this.ValidOrderItems = validOrderItems;

                if (@this.ExistVatRegime && @this.VatRegime.ExistVatClause)
                {
                    @this.DerivedVatClause = @this.VatRegime.VatClause;
                }
                else
                {
                    string TakenbyCountry = null;

                    if (@this.TakenBy.PartyContactMechanisms?.FirstOrDefault(v => v.ContactPurposes.Any(p => Equals(p, new ContactMechanismPurposes(session).RegisteredOffice)))?.ContactMechanism is PostalAddress registeredOffice)
                    {
                        TakenbyCountry = registeredOffice.Country.IsoCode;
                    }

                    var OutsideEUCustomer = @this.BillToCustomer?.VatRegime?.Equals(new VatRegimes(session).Export);
                    var shipFromBelgium = @this.ValidOrderItems?.Cast<SalesOrderItem>().All(v => Equals("BE", v.ShipFromAddress?.Country?.IsoCode));
                    var shipToEU = @this.ValidOrderItems?.Cast<SalesOrderItem>().Any(v => Equals(true, v.ShipToAddress?.Country?.EuMemberState));
                    var sellerResponsibleForTransport = @this.SalesTerms.Any(v => Equals(v.TermType, new IncoTermTypes(session).Cif) || Equals(v.TermType, new IncoTermTypes(session).Cfr));
                    var buyerResponsibleForTransport = @this.SalesTerms.Any(v => Equals(v.TermType, new IncoTermTypes(session).Exw));

                    if (Equals(@this.VatRegime, new VatRegimes(session).ServiceB2B))
                    {
                        @this.DerivedVatClause = new VatClauses(session).ServiceB2B;
                    }
                    else if (Equals(@this.VatRegime, new VatRegimes(session).IntraCommunautair))
                    {
                        @this.DerivedVatClause = new VatClauses(session).Intracommunautair;
                    }
                    else if (TakenbyCountry == "BE"
                             && OutsideEUCustomer.HasValue && OutsideEUCustomer.Value
                             && shipFromBelgium.HasValue && shipFromBelgium.Value
                             && shipToEU.HasValue && shipToEU.Value == false)
                    {
                        if (sellerResponsibleForTransport)
                        {
                            // You sell goods to a customer out of the EU and the goods are being sold and transported from Belgium to another country out of the EU and you transport the goods and importer is the customer
                            @this.DerivedVatClause = new VatClauses(session).BeArt39Par1Item1;
                        }
                        else if (buyerResponsibleForTransport)
                        {
                            // You sell goods to a customer out of the EU and the goods are being sold and transported from Belgium to another country out of the EU  and the customer does the transport of the goods and importer is the customer
                            @this.DerivedVatClause = new VatClauses(session).BeArt39Par1Item2;
                        }
                    }
                }

                @this.DerivedVatClause = @this.ExistAssignedVatClause ? @this.AssignedVatClause : @this.DerivedVatClause;

                var salesOrderShipmentStates = new SalesOrderShipmentStates(@this.Strategy.Session);
                var salesOrderPaymentStates = new SalesOrderPaymentStates(@this.Strategy.Session);
                var salesOrderInvoiceStates = new SalesOrderInvoiceStates(@this.Strategy.Session);

                var salesOrderItemShipmentStates = new SalesOrderItemShipmentStates(session);
                var salesOrderItemPaymentStates = new SalesOrderItemPaymentStates(session);
                var salesOrderItemInvoiceStates = new SalesOrderItemInvoiceStates(session);

                // SalesOrder Shipment State
                if (validOrderItems.Any())
                {
                    if (validOrderItems.All(v => v.SalesOrderItemShipmentState.Shipped))
                    {
                        @this.SalesOrderShipmentState = salesOrderShipmentStates.Shipped;
                    }
                    else if (validOrderItems.All(v => v.SalesOrderItemShipmentState.NotShipped))
                    {
                        @this.SalesOrderShipmentState = salesOrderShipmentStates.NotShipped;
                    }
                    else if (validOrderItems.Any(v => v.SalesOrderItemShipmentState.InProgress))
                    {
                        @this.SalesOrderShipmentState = salesOrderShipmentStates.InProgress;
                    }
                    else
                    {
                        @this.SalesOrderShipmentState = salesOrderShipmentStates.PartiallyShipped;
                    }

                    // SalesOrder Payment State
                    if (validOrderItems.All(v => v.SalesOrderItemPaymentState.Paid))
                    {
                        @this.SalesOrderPaymentState = salesOrderPaymentStates.Paid;
                    }
                    else if (validOrderItems.All(v => v.SalesOrderItemPaymentState.NotPaid))
                    {
                        @this.SalesOrderPaymentState = salesOrderPaymentStates.NotPaid;
                    }
                    else
                    {
                        @this.SalesOrderPaymentState = salesOrderPaymentStates.PartiallyPaid;
                    }

                    // SalesOrder Invoice State
                    if (validOrderItems.All(v => v.SalesOrderItemInvoiceState.Invoiced))
                    {
                        @this.SalesOrderInvoiceState = salesOrderInvoiceStates.Invoiced;
                    }
                    else if (validOrderItems.All(v => v.SalesOrderItemInvoiceState.NotInvoiced))
                    {
                        @this.SalesOrderInvoiceState = salesOrderInvoiceStates.NotInvoiced;
                    }
                    else
                    {
                        @this.SalesOrderInvoiceState = salesOrderInvoiceStates.PartiallyInvoiced;
                    }

                    // SalesOrder OrderState
                    if (@this.SalesOrderShipmentState.Shipped && @this.SalesOrderInvoiceState.Invoiced)
                    {
                        @this.SalesOrderState = new SalesOrderStates(@this.Strategy.Session).Completed;
                    }

                    if (@this.SalesOrderState.IsCompleted && @this.SalesOrderPaymentState.Paid)
                    {
                        @this.SalesOrderState = new SalesOrderStates(@this.Strategy.Session).Finished;
                    }
                }

                // TODO: Move to versioning
                @this.PreviousBillToCustomer = @this.BillToCustomer;
                @this.PreviousShipToCustomer = @this.ShipToCustomer;

                var singleton = session.GetSingleton();

                @this.AddSecurityToken(new SecurityTokens(session).DefaultSecurityToken);

                @this.ResetPrintDocument();

                // CanShip
                if (@this.SalesOrderState.Equals(new SalesOrderStates(@this.Strategy.Session).InProcess))
                {
                    var somethingToShip = false;
                    var allItemsAvailable = true;

                    foreach (var salesOrderItem1 in validOrderItems)
                    {
                        if (!@this.PartiallyShip && salesOrderItem1.QuantityRequestsShipping != salesOrderItem1.QuantityOrdered)
                        {
                            allItemsAvailable = false;
                            break;
                        }

                        if (@this.PartiallyShip && salesOrderItem1.QuantityRequestsShipping > 0)
                        {
                            somethingToShip = true;
                        }
                    }

                    @this.CanShip = (!@this.PartiallyShip && allItemsAvailable) || somethingToShip;
                }
                else
                {
                    @this.CanShip = false;
                }

                // CanInvoice
                if (@this.SalesOrderState.IsInProcess && object.Equals(@this.Store.BillingProcess, new BillingProcesses(@this.Strategy.Session).BillingForOrderItems))
                {
                    @this.CanInvoice = false;

                    foreach (var orderItem2 in validOrderItems)
                    {
                        var amountAlreadyInvoiced1 = orderItem2.OrderItemBillingsWhereOrderItem.Sum(v => v.Amount);

                        var leftToInvoice1 = (orderItem2.QuantityOrdered * orderItem2.UnitPrice) - amountAlreadyInvoiced1;

                        if (leftToInvoice1 > 0)
                        {
                            @this.CanInvoice = true;
                        }
                    }
                }
                else
                {
                    @this.CanInvoice = false;
                }

                if (@this.SalesOrderState.Equals(new SalesOrderStates(@this.Strategy.Session).InProcess) &&
                    Equals(@this.Store.BillingProcess, new BillingProcesses(@this.Strategy.Session).BillingForShipmentItems))
                {
                    @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Invoice));
                }

                if (@this.CanShip && @this.Store.AutoGenerateCustomerShipment)
                {
                    @this.Ship();
                }

                if (@this.SalesOrderState.IsInProcess
                    && (!@this.ExistLastSalesOrderState || !@this.LastSalesOrderState.IsInProcess)
                    && @this.TakenBy.SerialisedItemSoldOns.Contains(new SerialisedItemSoldOns(@this.Session()).SalesOrderAccept))
                {
                    foreach (SalesOrderItem item in @this.ValidOrderItems.Where(v => ((SalesOrderItem)v).ExistSerialisedItem))
                    {
                        if (item.ExistNextSerialisedItemAvailability)
                        {
                            item.SerialisedItem.SerialisedItemAvailability = item.NextSerialisedItemAvailability;

                            if (item.NextSerialisedItemAvailability.Equals(new SerialisedItemAvailabilities(@this.Session()).Sold))
                            {
                                item.SerialisedItem.OwnedBy = @this.ShipToCustomer;
                                item.SerialisedItem.Ownership = new Ownerships(@this.Session()).ThirdParty;
                            }
                        }

                        item.SerialisedItem.AvailableForSale = false;
                    }
                }

                Sync(validOrderItems, @this);

                if (@this.CanShip)
                {
                    @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Ship));
                }
                else
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Ship));
                }

                if (@this.CanInvoice)
                {
                    @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Invoice));
                }
                else
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Invoice));
                }

                if (!@this.SalesOrderInvoiceState.NotInvoiced || !@this.SalesOrderShipmentState.NotShipped)
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.SetReadyForPosting));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Post));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Reopen));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Approve));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Hold));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Continue));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Accept));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Revise));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Complete));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Reject));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Cancel));

                    var deniablePermissionByOperandTypeId = new Dictionary<OperandType, Permission>();

                    foreach (Permission permission in @this.Session().Extent<Permission>())
                    {
                        if (permission.ClassPointer == @this.Strategy.Class.Id && permission.Operation == Operations.Write)
                        {
                            deniablePermissionByOperandTypeId.Add(permission.OperandType, permission);
                        }
                    }

                    foreach (var keyValuePair in deniablePermissionByOperandTypeId)
                    {
                        @this.AddDeniedPermission(keyValuePair.Value);
                    }
                }

                var deletePermission = new Permissions(@this.Strategy.Session).Get(@this.Meta.ObjectType, @this.Meta.Delete);
                if (@this.IsDeletable)
                {
                    @this.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    @this.AddDeniedPermission(deletePermission);
                }

                if (@this.HasChangedStates())
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

                var orderPriceComponents = salesOrder.TakenBy?.PriceComponentsWherePricedBy
                    .Where(v => v.FromDate <= salesOrder.OrderDate && (!v.ExistThroughDate || v.ThroughDate >= salesOrder.OrderDate))
                    .ToArray();

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
                    v => PriceComponents.AppsIsApplicable(
                        new PriceComponents.IsApplicable
                        {
                            PriceComponent = v,
                            Customer = salesOrder.BillToCustomer,
                            Product = salesOrderItem.Product,
                            SalesOrder = salesOrder,
                            QuantityOrdered = quantityOrdered,
                            ValueOrdered = valueOrdered,
                        })).ToArray();

                var unitBasePrice = priceComponents.OfType<BasePrice>()
                    .Where(v => salesOrderItem.ExistProduct
                                && salesOrderItem.OrderedWithFeatures.Count > 0
                                && v.ExistProduct
                                && v.ExistProductFeature
                                && v.Product.Equals(salesOrderItem.Product)
                                && salesOrderItem.OrderedWithFeatures.Contains(v.ProductFeature))
                    .Min(v => v.Price);

                if (unitBasePrice == null)
                {
                    unitBasePrice = priceComponents.OfType<BasePrice>().Min(v => v.Price);
                }


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
                        validation.AddError($"{salesOrderItem} {this.M.SalesOrderItem.UnitBasePrice} No BasePrice with a Price");
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
}
