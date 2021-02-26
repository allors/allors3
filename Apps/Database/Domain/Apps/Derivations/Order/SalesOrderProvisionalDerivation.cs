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

    public class SalesOrderProvisionalDerivation : DomainDerivation
    {
        public SalesOrderProvisionalDerivation(M m) : base(m, new Guid("bb9b637c-4594-4f9e-927c-bd47236ab515")) =>
            this.Patterns = new Pattern[]
            {
                new AssociationPattern(this.M.SalesOrder.SalesOrderState),
                new AssociationPattern(this.M.SalesOrder.TakenBy),
                new AssociationPattern(this.M.SalesOrder.Store),
                new AssociationPattern(this.M.SalesOrder.BillToCustomer),
                new AssociationPattern(this.M.SalesOrder.BillToEndCustomer),
                new AssociationPattern(this.M.SalesOrder.ShipToCustomer),
                new AssociationPattern(this.M.SalesOrder.ShipToEndCustomer),
                new AssociationPattern(this.M.SalesOrder.AssignedIrpfRegime),
                new AssociationPattern(this.M.SalesOrder.AssignedVatRegime),
                new AssociationPattern(this.M.SalesOrder.AssignedVatClause),
                new AssociationPattern(this.M.SalesOrder.AssignedCurrency),
                new AssociationPattern(this.M.SalesOrder.AssignedTakenByContactMechanism),
                new AssociationPattern(this.M.SalesOrder.AssignedBillToContactMechanism),
                new AssociationPattern(this.M.SalesOrder.AssignedBillToEndCustomerContactMechanism),
                new AssociationPattern(this.M.SalesOrder.AssignedShipToEndCustomerAddress),
                new AssociationPattern(this.M.SalesOrder.AssignedShipFromAddress),
                new AssociationPattern(this.M.SalesOrder.AssignedShipToAddress),
                new AssociationPattern(this.M.SalesOrder.AssignedShipmentMethod),
                new AssociationPattern(this.M.SalesOrder.AssignedPaymentMethod),
                new AssociationPattern(this.M.SalesOrder.Locale),
                new AssociationPattern(this.M.Party.Locale) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToCustomer }},
                new AssociationPattern(this.M.Organisation.Locale) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrdersWhereTakenBy }},
                new AssociationPattern(this.M.Party.PreferredCurrency) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToCustomer }},
                new AssociationPattern(this.M.Party.VatRegime) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToCustomer }},
                new AssociationPattern(this.M.Party.IrpfRegime) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToCustomer }},
                new AssociationPattern(this.M.Organisation.PreferredCurrency) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrdersWhereTakenBy }},
                new AssociationPattern(this.M.Organisation.OrderAddress) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrdersWhereTakenBy }},
                new AssociationPattern(this.M.Organisation.ShippingAddress) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrdersWhereTakenBy }},
                new AssociationPattern(this.M.Organisation.BillingAddress) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrdersWhereTakenBy }},
                new AssociationPattern(this.M.Organisation.GeneralCorrespondence) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrdersWhereTakenBy }},
                new AssociationPattern(this.M.Organisation.DefaultPaymentMethod) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrdersWhereTakenBy }},
                new AssociationPattern(this.M.Party.BillingAddress) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToCustomer }},
                new AssociationPattern(this.M.Party.ShippingAddress) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToCustomer }},
                new AssociationPattern(this.M.Party.GeneralCorrespondence) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToCustomer }},
                new AssociationPattern(this.M.Party.BillingAddress) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToEndCustomer }},
                new AssociationPattern(this.M.Party.ShippingAddress) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToEndCustomer }},
                new AssociationPattern(this.M.Party.GeneralCorrespondence) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToEndCustomer }},
                new AssociationPattern(this.M.Party.ShippingAddress) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereShipToEndCustomer }},
                new AssociationPattern(this.M.Party.GeneralCorrespondence) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereShipToEndCustomer }},
                new AssociationPattern(this.M.Party.DefaultShipmentMethod) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereShipToCustomer }},
                new AssociationPattern(this.M.Store.DefaultShipmentMethod) { Steps = new IPropertyType[] { this.M.Store.SalesOrdersWhereStore }},
                new AssociationPattern(this.M.Store.DefaultCollectionMethod) { Steps = new IPropertyType[] { this.M.Store.SalesOrdersWhereStore }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrder>().Where(v => v.SalesOrderState.IsProvisional))
            {
                @this.DerivedLocale = @this.Locale ?? @this.BillToCustomer?.Locale ?? @this.TakenBy?.Locale;
                @this.DerivedVatRegime = @this.AssignedVatRegime ?? @this.BillToCustomer?.VatRegime;
                @this.DerivedIrpfRegime = @this.AssignedIrpfRegime ?? @this.BillToCustomer?.IrpfRegime;
                @this.DerivedCurrency = @this.AssignedCurrency ?? @this.BillToCustomer?.PreferredCurrency ?? @this.BillToCustomer?.Locale?.Country?.Currency ?? @this.TakenBy?.PreferredCurrency;
                @this.DerivedTakenByContactMechanism = @this.AssignedTakenByContactMechanism ?? @this.TakenBy?.OrderAddress ?? @this.TakenBy?.BillingAddress ?? @this.TakenBy?.GeneralCorrespondence;
                @this.DerivedBillToContactMechanism = @this.AssignedBillToContactMechanism ?? @this.BillToCustomer?.BillingAddress ?? @this.BillToCustomer?.ShippingAddress ?? @this.BillToCustomer?.GeneralCorrespondence;
                @this.DerivedBillToEndCustomerContactMechanism = @this.AssignedBillToEndCustomerContactMechanism ?? @this.BillToEndCustomer?.BillingAddress ?? @this.BillToEndCustomer?.ShippingAddress ?? @this.BillToEndCustomer?.GeneralCorrespondence;
                @this.DerivedShipToEndCustomerAddress = @this.AssignedShipToEndCustomerAddress ?? @this.ShipToEndCustomer?.ShippingAddress ?? @this.ShipToEndCustomer?.GeneralCorrespondence as PostalAddress;
                @this.DerivedShipFromAddress = @this.AssignedShipFromAddress ?? @this.TakenBy?.ShippingAddress;
                @this.DerivedShipToAddress = @this.AssignedShipToAddress ?? @this.ShipToCustomer?.ShippingAddress;
                @this.DerivedShipmentMethod = @this.AssignedShipmentMethod ?? @this.ShipToCustomer?.DefaultShipmentMethod ?? @this.Store?.DefaultShipmentMethod;
                @this.DerivedPaymentMethod = @this.AssignedPaymentMethod ?? @this.TakenBy?.DefaultPaymentMethod ?? @this.Store?.DefaultCollectionMethod;
            }
        }
    }
}
