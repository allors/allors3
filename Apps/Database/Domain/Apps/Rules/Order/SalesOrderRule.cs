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

    public class SalesOrderRule : Rule
    {
        public SalesOrderRule(MetaPopulation m) : base(m, new Guid("CC43279A-22B4-499E-9ADA-33364E30FBD4")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrder.RolePattern(v => v.TakenBy),
                m.SalesOrder.RolePattern(v => v.Store),
                m.SalesOrder.RolePattern(v => v.BillToCustomer),
                m.SalesOrder.RolePattern(v => v.BillToEndCustomer),
                m.SalesOrder.RolePattern(v => v.ShipToCustomer),
                m.SalesOrder.RolePattern(v => v.ShipToEndCustomer),
                m.SalesOrder.RolePattern(v => v.PlacingCustomer),
                m.SalesOrder.RolePattern(v => v.OrderDate),
                m.SalesOrder.RolePattern(v => v.SalesOrderItems),
                m.SalesOrder.RolePattern(v => v.ValidOrderItems),
                m.SalesOrder.RolePattern(v => v.DerivedShipToAddress),
                m.SalesOrder.RolePattern(v => v.DerivedBillToContactMechanism),
                m.SalesOrder.RolePattern(v => v.CanShip),
                m.InvoiceTerm.RolePattern(v => v.TermValue, v => v.OrderItemWhereSalesTerm.OrderItem.AsSalesOrderItem.SalesOrderWhereSalesOrderItem),
                m.CustomerRelationship.RolePattern(v => v.FromDate, v => v.Customer.Party.SalesOrdersWhereBillToCustomer),
                m.CustomerRelationship.RolePattern(v => v.ThroughDate, v => v.Customer.Party.SalesOrdersWhereBillToCustomer),
                m.CustomerRelationship.RolePattern(v => v.FromDate, v => v.Customer.Party.SalesOrdersWhereShipToCustomer),
                m.CustomerRelationship.RolePattern(v => v.ThroughDate, v => v.Customer.Party.SalesOrdersWhereShipToCustomer),
                m.Store.RolePattern(v => v.AutoGenerateCustomerShipment, v => v.InternalOrganisation.InternalOrganisation.AsOrganisation.SalesOrdersWhereTakenBy),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SalesOrder>())
            {
                // TODO: Move to versioning
                @this.PreviousBillToCustomer = @this.BillToCustomer;
                @this.PreviousShipToCustomer = @this.ShipToCustomer;

                // TODO: Ticket #5 Github
                @this.ResetPrintDocument();
            }
        }
    }
}
