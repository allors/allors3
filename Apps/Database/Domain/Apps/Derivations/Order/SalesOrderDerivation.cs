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
                new ChangedPattern(this.M.SalesOrder.OrderDate),
                new ChangedPattern(this.M.SalesOrder.SalesOrderItems),
                new ChangedPattern(this.M.SalesOrder.DerivedShipToAddress),
                new ChangedPattern(this.M.SalesOrder.DerivedBillToContactMechanism),
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

                @this.ValidOrderItems = @this.SalesOrderItems.Where(v => v.IsValid).ToArray();

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
                    var shipToEU = @this.ValidOrderItems?.Cast<SalesOrderItem>().Any(v => Equals(true, v.DerivedShipToAddress?.Country?.EuMemberState));
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
