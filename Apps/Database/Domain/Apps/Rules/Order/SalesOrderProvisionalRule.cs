// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class SalesOrderProvisionalRule : Rule
    {
        public SalesOrderProvisionalRule(MetaPopulation m) : base(m, new Guid("bb9b637c-4594-4f9e-927c-bd47236ab515")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.SalesOrder, m.SalesOrder.SalesOrderState),
                new RolePattern(m.SalesOrder, m.SalesOrder.TakenBy),
                new RolePattern(m.SalesOrder, m.SalesOrder.Store),
                new RolePattern(m.SalesOrder, m.SalesOrder.BillToCustomer),
                new RolePattern(m.SalesOrder, m.SalesOrder.BillToEndCustomer),
                new RolePattern(m.SalesOrder, m.SalesOrder.ShipToCustomer),
                new RolePattern(m.SalesOrder, m.SalesOrder.ShipToEndCustomer),
                new RolePattern(m.SalesOrder, m.SalesOrder.AssignedIrpfRegime),
                new RolePattern(m.SalesOrder, m.SalesOrder.AssignedVatRegime),
                new RolePattern(m.SalesOrder, m.SalesOrder.AssignedVatClause),
                new RolePattern(m.SalesOrder, m.SalesOrder.AssignedCurrency),
                new RolePattern(m.SalesOrder, m.SalesOrder.AssignedTakenByContactMechanism),
                new RolePattern(m.SalesOrder, m.SalesOrder.AssignedBillToContactMechanism),
                new RolePattern(m.SalesOrder, m.SalesOrder.AssignedBillToEndCustomerContactMechanism),
                new RolePattern(m.SalesOrder, m.SalesOrder.AssignedShipToEndCustomerAddress),
                new RolePattern(m.SalesOrder, m.SalesOrder.AssignedShipFromAddress),
                new RolePattern(m.SalesOrder, m.SalesOrder.AssignedShipToAddress),
                new RolePattern(m.SalesOrder, m.SalesOrder.AssignedShipmentMethod),
                new RolePattern(m.SalesOrder, m.SalesOrder.AssignedPaymentMethod),
                new RolePattern(m.SalesOrder, m.SalesOrder.Locale),
                new RolePattern(m.SalesOrder, m.SalesOrder.OrderDate),
                new RolePattern(m.Party, m.Party.Locale) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToCustomer }},
                new RolePattern(m.Organisation, m.Organisation.Locale) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrdersWhereTakenBy }},
                new RolePattern(m.Party, m.Party.PreferredCurrency) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToCustomer }},
                new RolePattern(m.Organisation, m.Organisation.PreferredCurrency) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrdersWhereTakenBy }},
                new RolePattern(m.Organisation, m.Organisation.OrderAddress) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrdersWhereTakenBy }},
                new RolePattern(m.Organisation, m.Organisation.ShippingAddress) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrdersWhereTakenBy }},
                new RolePattern(m.Organisation, m.Organisation.BillingAddress) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrdersWhereTakenBy }},
                new RolePattern(m.Organisation, m.Organisation.GeneralCorrespondence) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrdersWhereTakenBy }},
                new RolePattern(m.Organisation, m.Organisation.DefaultPaymentMethod) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrdersWhereTakenBy }},
                new RolePattern(m.Party, m.Party.BillingAddress) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToCustomer }},
                new RolePattern(m.Party, m.Party.ShippingAddress) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToCustomer }},
                new RolePattern(m.Party, m.Party.GeneralCorrespondence) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToCustomer }},
                new RolePattern(m.Party, m.Party.BillingAddress) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToEndCustomer }},
                new RolePattern(m.Party, m.Party.ShippingAddress) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToEndCustomer }},
                new RolePattern(m.Party, m.Party.GeneralCorrespondence) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToEndCustomer }},
                new RolePattern(m.Party, m.Party.ShippingAddress) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereShipToEndCustomer }},
                new RolePattern(m.Party, m.Party.GeneralCorrespondence) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereShipToEndCustomer }},
                new RolePattern(m.Party, m.Party.DefaultShipmentMethod) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereShipToCustomer }},
                new RolePattern(m.Store, m.Store.DefaultShipmentMethod) { Steps = new IPropertyType[] { this.M.Store.SalesOrdersWhereStore }},
                new RolePattern(m.Store, m.Store.DefaultCollectionMethod) { Steps = new IPropertyType[] { this.M.Store.SalesOrdersWhereStore }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrder>().Where(v => v.SalesOrderState.IsProvisional))
            {
                @this.DerivedLocale = @this.Locale ?? @this.BillToCustomer?.Locale ?? @this.TakenBy?.Locale;
                @this.DerivedVatRegime = @this.AssignedVatRegime;
                @this.DerivedIrpfRegime = @this.AssignedIrpfRegime;
                @this.DerivedCurrency = @this.AssignedCurrency ?? @this.BillToCustomer?.PreferredCurrency ?? @this.BillToCustomer?.Locale?.Country?.Currency ?? @this.TakenBy?.PreferredCurrency;
                @this.DerivedTakenByContactMechanism = @this.AssignedTakenByContactMechanism ?? @this.TakenBy?.OrderAddress ?? @this.TakenBy?.BillingAddress ?? @this.TakenBy?.GeneralCorrespondence;
                @this.DerivedBillToContactMechanism = @this.AssignedBillToContactMechanism ?? @this.BillToCustomer?.BillingAddress ?? @this.BillToCustomer?.ShippingAddress ?? @this.BillToCustomer?.GeneralCorrespondence;
                @this.DerivedBillToEndCustomerContactMechanism = @this.AssignedBillToEndCustomerContactMechanism ?? @this.BillToEndCustomer?.BillingAddress ?? @this.BillToEndCustomer?.ShippingAddress ?? @this.BillToEndCustomer?.GeneralCorrespondence;
                @this.DerivedShipToEndCustomerAddress = @this.AssignedShipToEndCustomerAddress ?? @this.ShipToEndCustomer?.ShippingAddress ?? @this.ShipToEndCustomer?.GeneralCorrespondence as PostalAddress;
                @this.DerivedShipFromAddress = @this.AssignedShipFromAddress ?? @this.TakenBy?.ShippingAddress;
                @this.DerivedShipToAddress = @this.AssignedShipToAddress ?? @this.ShipToCustomer?.ShippingAddress;
                @this.DerivedShipmentMethod = @this.AssignedShipmentMethod ?? @this.ShipToCustomer?.DefaultShipmentMethod ?? @this.Store?.DefaultShipmentMethod;
                @this.DerivedPaymentMethod = @this.AssignedPaymentMethod ?? @this.TakenBy?.DefaultPaymentMethod ?? @this.Store?.DefaultCollectionMethod;

                if (@this.ExistOrderDate)
                {
                    @this.DerivedVatRate = @this.DerivedVatRegime?.VatRates.First(v => v.FromDate <= @this.OrderDate && (!v.ExistThroughDate || v.ThroughDate >= @this.OrderDate));
                    @this.DerivedIrpfRate = @this.DerivedIrpfRegime?.IrpfRates.First(v => v.FromDate <= @this.OrderDate && (!v.ExistThroughDate || v.ThroughDate >= @this.OrderDate));
                }

                if (@this.ExistDerivedVatRegime)
                {
                    if (@this.DerivedVatRegime.ExistVatClause)
                    {
                        @this.DerivedVatClause = @this.DerivedVatRegime.VatClause;
                    }
                    else
                    {
                        @this.RemoveDerivedVatClause();
                    }
                }
                else
                {
                    @this.RemoveDerivedVatClause();
                }

                @this.DerivedVatClause = @this.ExistAssignedVatClause ? @this.AssignedVatClause : @this.DerivedVatClause;
            }
        }
    }
}
