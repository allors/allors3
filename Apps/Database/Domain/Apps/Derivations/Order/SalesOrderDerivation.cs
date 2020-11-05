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
            new ChangedPattern(this.M.SalesOrder.TakenBy),
            new ChangedPattern(this.M.SalesOrder.Store),
            new ChangedPattern(this.M.SalesOrder.BillToCustomer),
            new ChangedPattern(this.M.SalesOrder.BillToEndCustomer),
            new ChangedPattern(this.M.SalesOrder.ShipToCustomer),
            new ChangedPattern(this.M.SalesOrder.ShipToEndCustomer),
            new ChangedPattern(this.M.SalesOrder.VatRegime),
            new ChangedPattern(this.M.SalesOrder.AssignedVatClause),
            new ChangedPattern(this.M.SalesOrder.OrderDate),
            new ChangedPattern(this.M.SalesOrder.SalesOrderItems),
            new ChangedPattern(this.M.InvoiceTerm.TermValue) { Steps =  new IPropertyType[] {m.InvoiceTerm.OrderWhereSalesTerm} },
            new ChangedPattern(this.M.InvoiceTerm.TermValue) { Steps =  new IPropertyType[] {m.InvoiceTerm.OrderItemWhereSalesTerm, m.SalesOrderItem.SalesOrderWhereSalesOrderItem } },
            new ChangedPattern(this.M.CustomerRelationship.FromDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesOrdersWhereBillToCustomer} },
            new ChangedPattern(this.M.CustomerRelationship.ThroughDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesOrdersWhereBillToCustomer } },
            new ChangedPattern(this.M.CustomerRelationship.FromDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesOrdersWhereShipToCustomer } },
            new ChangedPattern(this.M.CustomerRelationship.ThroughDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesOrdersWhereShipToCustomer } },
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

                // TODO: Move to versioning
                @this.PreviousBillToCustomer = @this.BillToCustomer;
                @this.PreviousShipToCustomer = @this.ShipToCustomer;

                var singleton = session.GetSingleton();

                @this.AddSecurityToken(new SecurityTokens(session).DefaultSecurityToken);

                @this.ResetPrintDocument();

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
            }
        }
    }
}
