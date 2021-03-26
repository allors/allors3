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
                new RolePattern(m.SalesOrder, m.SalesOrder.TakenBy),
                new RolePattern(m.SalesOrder, m.SalesOrder.Store),
                new RolePattern(m.SalesOrder, m.SalesOrder.BillToCustomer),
                new RolePattern(m.SalesOrder, m.SalesOrder.BillToEndCustomer),
                new RolePattern(m.SalesOrder, m.SalesOrder.ShipToCustomer),
                new RolePattern(m.SalesOrder, m.SalesOrder.ShipToEndCustomer),
                new RolePattern(m.SalesOrder, m.SalesOrder.PlacingCustomer),
                new RolePattern(m.SalesOrder, m.SalesOrder.OrderDate),
                new RolePattern(m.SalesOrder, m.SalesOrder.SalesOrderItems),
                new RolePattern(m.SalesOrder, m.SalesOrder.ValidOrderItems),
                new RolePattern(m.SalesOrder, m.SalesOrder.DerivedShipToAddress),
                new RolePattern(m.SalesOrder, m.SalesOrder.DerivedBillToContactMechanism),
                new RolePattern(m.SalesOrder, m.SalesOrder.CanShip),
                new RolePattern(m.InvoiceTerm, m.InvoiceTerm.TermValue) { Steps =  new IPropertyType[] {m.InvoiceTerm.OrderWhereSalesTerm} },
                new RolePattern(m.InvoiceTerm, m.InvoiceTerm.TermValue) { Steps =  new IPropertyType[] {m.InvoiceTerm.OrderItemWhereSalesTerm, m.SalesOrderItem.SalesOrderWhereSalesOrderItem } },
                new RolePattern(m.CustomerRelationship, m.CustomerRelationship.FromDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesOrdersWhereBillToCustomer} },
                new RolePattern(m.CustomerRelationship, m.CustomerRelationship.ThroughDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesOrdersWhereBillToCustomer } },
                new RolePattern(m.CustomerRelationship, m.CustomerRelationship.FromDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesOrdersWhereShipToCustomer } },
                new RolePattern(m.CustomerRelationship, m.CustomerRelationship.ThroughDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesOrdersWhereShipToCustomer } },
                new RolePattern(m.Store, m.Store.AutoGenerateCustomerShipment) { Steps =  new IPropertyType[] {m.Store.InternalOrganisation, m.Organisation.SalesOrdersWhereTakenBy } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

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
                    var year = @this.OrderDate.Year;
                    @this.OrderNumber = @this.Store.NextSalesOrderNumber(year);

                    var fiscalYearStoreSequenceNumbers = @this.Store.FiscalYearsStoreSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);
                    var prefix = @this.TakenBy.InvoiceSequence.IsEnforcedSequence ? @this.Store.SalesOrderNumberPrefix : fiscalYearStoreSequenceNumbers.SalesOrderNumberPrefix;
                    @this.SortableOrderNumber = @this.Transaction().GetSingleton().SortableNumber(prefix, @this.OrderNumber, year.ToString());
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

                // TODO: Move to versioning
                @this.PreviousBillToCustomer = @this.BillToCustomer;
                @this.PreviousShipToCustomer = @this.ShipToCustomer;

                foreach (SalesOrderItem salesOrderItem in @this.SalesOrderItems)
                {
                    salesOrderItem.Sync(@this);
                }

                @this.ResetPrintDocument();

                if (@this.CanShip && @this.Store.AutoGenerateCustomerShipment)
                {
                    @this.Ship();
                }
            }
        }
    }
}
