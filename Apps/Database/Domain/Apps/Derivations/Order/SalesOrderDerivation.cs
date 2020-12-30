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
                new ChangedPattern(this.M.SalesOrder.ValidOrderItems),
                new ChangedPattern(this.M.SalesOrder.DerivedShipToAddress),
                new ChangedPattern(this.M.SalesOrder.DerivedBillToContactMechanism),
                new ChangedPattern(this.M.SalesOrder.CanShip),
                new ChangedPattern(this.M.InvoiceTerm.TermValue) { Steps =  new IPropertyType[] {m.InvoiceTerm.OrderWhereSalesTerm} },
                new ChangedPattern(this.M.InvoiceTerm.TermValue) { Steps =  new IPropertyType[] {m.InvoiceTerm.OrderItemWhereSalesTerm, m.SalesOrderItem.SalesOrderWhereSalesOrderItem } },
                new ChangedPattern(this.M.CustomerRelationship.FromDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesOrdersWhereBillToCustomer} },
                new ChangedPattern(this.M.CustomerRelationship.ThroughDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesOrdersWhereBillToCustomer } },
                new ChangedPattern(this.M.CustomerRelationship.FromDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesOrdersWhereShipToCustomer } },
                new ChangedPattern(this.M.CustomerRelationship.ThroughDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesOrdersWhereShipToCustomer } },
                new ChangedPattern(this.M.Store.AutoGenerateCustomerShipment) { Steps =  new IPropertyType[] {m.Store.InternalOrganisation, m.Organisation.SalesOrdersWhereTakenBy } },
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
                    var year = @this.OrderDate.Year;
                    @this.OrderNumber = @this.Store.NextSalesOrderNumber(year);

                    var fiscalYearsStoreSequenceNumbers = new FiscalYearsStoreSequenceNumbers(@this.Session()).Extent();
                    fiscalYearsStoreSequenceNumbers.Filter.AddEquals(M.FiscalYearStoreSequenceNumbers.FiscalYear, year);
                    var fiscalYearStoreSequenceNumbers = fiscalYearsStoreSequenceNumbers.First;

                    var prefix = fiscalYearStoreSequenceNumbers == null ? @this.Store.SalesOrderNumberPrefix : fiscalYearStoreSequenceNumbers.SalesOrderNumberPrefix;
                    @this.SortableOrderNumber = @this.Session().GetSingleton().SortableNumber(prefix, @this.OrderNumber, year.ToString());
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

                @this.AddSecurityToken(new SecurityTokens(session).DefaultSecurityToken);

                @this.ResetPrintDocument();

                if (@this.CanShip && @this.Store.AutoGenerateCustomerShipment)
                {
                    @this.Ship();
                }
            }
        }
    }
}
