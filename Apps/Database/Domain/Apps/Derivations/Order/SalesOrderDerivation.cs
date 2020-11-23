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
                new ChangedPattern(this.M.SalesOrder.PlacingCustomer),
                new ChangedPattern(this.M.SalesOrder.AssignedVatRegime),
                new ChangedPattern(this.M.SalesOrder.AssignedVatClause),
                new ChangedPattern(this.M.SalesOrder.OrderDate),
                new ChangedPattern(this.M.SalesOrder.SalesOrderItems),
                new ChangedPattern(this.M.SalesOrder.AssignedCurrency),
                new ChangedPattern(this.M.SalesOrder.Locale),
                new ChangedPattern(this.M.Party.Locale) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToCustomer }},
                new ChangedPattern(this.M.Organisation.Locale) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrdersWhereTakenBy }},
                new ChangedPattern(this.M.Party.PreferredCurrency) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToCustomer }},
                new ChangedPattern(this.M.Organisation.PreferredCurrency) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrdersWhereTakenBy }},
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
                if (@this.ExistCurrentVersion
                    && @this.CurrentVersion.ExistTakenBy
                    && @this.TakenBy != @this.CurrentVersion.TakenBy)
                {
                    validation.AddError($"{@this} {this.M.SalesOrder.TakenBy} {ErrorMessages.InternalOrganisationChanged}");
                }

                // SalesOrder Derivations and Validations
                @this.BillToCustomer ??= @this.ShipToCustomer;
                @this.ShipToCustomer ??= @this.BillToCustomer;
                @this.Customers = new[] { @this.BillToCustomer, @this.ShipToCustomer, @this.PlacingCustomer };

                if (@this.SalesOrderState.IsProvisional)
                {
                    @this.DerivedLocale = @this.Locale ?? @this.BillToCustomer?.Locale ?? @this.TakenBy?.Locale;
                    @this.DerivedVatRegime = @this.AssignedVatRegime ?? @this.BillToCustomer?.VatRegime;
                    @this.DerivedIrpfRegime = @this.AssignedIrpfRegime ?? @this.BillToCustomer?.IrpfRegime;
                    @this.DerivedCurrency = @this.AssignedCurrency ?? @this.BillToCustomer?.PreferredCurrency ?? @this.BillToCustomer?.Locale?.Country?.Currency ?? @this.TakenBy?.PreferredCurrency;
                    @this.DerivedTakenByContactMechanism = @this.AssignedTakenByContactMechanism ?? @this.TakenBy?.OrderAddress ?? @this.TakenBy?.BillingAddress ?? @this.TakenBy?.GeneralCorrespondence;
                    @this.DerivedBillToContactMechanism = @this.AssignedBillToContactMechanism ?? @this.BillToCustomer?.BillingAddress ?? @this.BillToCustomer?.ShippingAddress ?? @this.BillToCustomer?.GeneralCorrespondence;
                    @this.DerivedBillToEndCustomerContactMechanism = @this.AssignedBillToEndCustomerContactMechanism ?? @this.BillToEndCustomer?.BillingAddress ?? @this.BillToEndCustomer?.ShippingAddress ?? @this.BillToCustomer?.GeneralCorrespondence;
                    @this.DerivedShipToEndCustomerAddress = @this.AssignedShipToEndCustomerAddress ?? @this.ShipToEndCustomer?.ShippingAddress ?? @this.ShipToCustomer?.GeneralCorrespondence as PostalAddress;
                    @this.DerivedShipFromAddress = @this.AssignedShipFromAddress ?? @this.TakenBy?.ShippingAddress;
                    @this.DerivedShipToAddress = @this.AssignedShipToAddress ?? @this.ShipToCustomer?.ShippingAddress;
                    @this.DerivedShipmentMethod =  @this.AssignedShipmentMethod ?? @this.ShipToCustomer?.DefaultShipmentMethod ?? @this.Store.DefaultShipmentMethod;
                    @this.DerivedPaymentMethod = @this.AssignedPaymentMethod ?? @this.ShipToCustomer?.PartyFinancialRelationshipsWhereFinancialParty?.FirstOrDefault(v => object.Equals(v.InternalOrganisation, @this.TakenBy))?.DefaultPaymentMethod ?? @this.Store.DefaultCollectionMethod;
                }

                if (!@this.ExistOrderNumber && @this.ExistStore)
                {
                    @this.OrderNumber = @this.Store.NextSalesOrderNumber(@this.OrderDate.Year);
                    @this.SortableOrderNumber = NumberFormatter.SortableNumber(@this.Store.SalesOrderNumberPrefix, @this.OrderNumber, @this.OrderDate.Year.ToString());
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
                    validation.AssertExists(@this, @this.Meta.DerivedShipToAddress);
                    validation.AssertExists(@this, @this.Meta.DerivedBillToContactMechanism);
                }

                // SalesOrderItem Derivations and Validations
                foreach (SalesOrderItem salesOrderItem in @this.SalesOrderItems)
                {
                    var salesOrderItemDerivedRoles = salesOrderItem;

                    salesOrderItem.DerivedShipFromAddress = salesOrderItem.AssignedShipFromAddress ?? @this.DerivedShipFromAddress;
                    salesOrderItemDerivedRoles.ShipToAddress = salesOrderItem.AssignedShipToAddress ?? salesOrderItem.AssignedShipToParty?.ShippingAddress ?? @this.DerivedShipToAddress;
                    salesOrderItemDerivedRoles.ShipToParty = salesOrderItem.AssignedShipToParty ?? @this.ShipToCustomer;
                    salesOrderItemDerivedRoles.DeliveryDate = salesOrderItem.AssignedDeliveryDate ?? @this.DeliveryDate;
                    salesOrderItemDerivedRoles.DerivedVatRegime = salesOrderItem.AssignedVatRegime ?? @this.DerivedVatRegime;
                    salesOrderItemDerivedRoles.VatRate = salesOrderItem.DerivedVatRegime?.VatRate;
                    salesOrderItemDerivedRoles.DerivedIrpfRegime = salesOrderItem.AssignedIrpfRegime ?? @this.DerivedIrpfRegime;
                    salesOrderItemDerivedRoles.IrpfRate = salesOrderItem.DerivedIrpfRegime?.IrpfRate;

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

                if (@this.ExistDerivedVatRegime && @this.DerivedVatRegime.ExistVatClause)
                {
                    @this.DerivedVatClause = @this.DerivedVatRegime.VatClause;
                }
                else
                {
                    string TakenbyCountry = null;

                    if (@this.TakenBy.PartyContactMechanisms?.FirstOrDefault(v => v.ContactPurposes.Any(p => Equals(p, new ContactMechanismPurposes(session).RegisteredOffice)))?.ContactMechanism is PostalAddress registeredOffice)
                    {
                        TakenbyCountry = registeredOffice.Country.IsoCode;
                    }

                    var OutsideEUCustomer = @this.BillToCustomer?.VatRegime?.Equals(new VatRegimes(session).Export);
                    var shipFromBelgium = @this.ValidOrderItems?.Cast<SalesOrderItem>().All(v => Equals("BE", v.DerivedShipFromAddress?.Country?.IsoCode));
                    var shipToEU = @this.ValidOrderItems?.Cast<SalesOrderItem>().Any(v => Equals(true, v.ShipToAddress?.Country?.EuMemberState));
                    var sellerResponsibleForTransport = @this.SalesTerms.Any(v => Equals(v.TermType, new IncoTermTypes(session).Cif) || Equals(v.TermType, new IncoTermTypes(session).Cfr));
                    var buyerResponsibleForTransport = @this.SalesTerms.Any(v => Equals(v.TermType, new IncoTermTypes(session).Exw));

                    if (Equals(@this.DerivedVatRegime, new VatRegimes(session).ServiceB2B))
                    {
                        @this.DerivedVatClause = new VatClauses(session).ServiceB2B;
                    }
                    else if (Equals(@this.DerivedVatRegime, new VatRegimes(session).IntraCommunautair))
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
