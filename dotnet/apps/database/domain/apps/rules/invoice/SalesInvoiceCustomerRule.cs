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
    using Resources;

    public class SalesInvoiceCustomerRule : Rule
    {
        public SalesInvoiceCustomerRule(MetaPopulation m) : base(m, new Guid("3d76cf7a-7f7a-438a-b3a4-921bdad42765")) =>
            this.Patterns = new Pattern[]
        {
            m.SalesInvoice.RolePattern(v => v.BilledFrom),
            m.SalesInvoice.RolePattern(v => v.BillToCustomer),
            m.SalesInvoice.RolePattern(v => v.BillToEndCustomer),
            m.SalesInvoice.RolePattern(v => v.ShipToCustomer),
            m.SalesInvoice.RolePattern(v => v.ShipToEndCustomer),
            m.SalesInvoice.RolePattern(v => v.InvoiceDate),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>())
            {
                @this.RemoveCustomers();
                if (@this.ExistBillToCustomer && !@this.Customers.Contains(@this.BillToCustomer))
                {
                    @this.AddCustomer(@this.BillToCustomer);
                }

                if (@this.ExistShipToCustomer && !@this.Customers.Contains(@this.ShipToCustomer))
                {
                    @this.AddCustomer(@this.ShipToCustomer);
                }

                if (@this.ExistBillToCustomer
                    && @this.ExistBilledFrom
                    && !@this.BillToCustomer.AppsIsActiveCustomer(@this.BilledFrom, @this.InvoiceDate))
                {
                    validation.AddError($"{@this} {this.M.SalesInvoice.BillToCustomer} {ErrorMessages.PartyIsNotACustomer}");
                }

                if (@this.ExistShipToCustomer
                    && @this.ExistBilledFrom
                    && !@this.ShipToCustomer.AppsIsActiveCustomer(@this.BilledFrom, @this.InvoiceDate))
                {
                    validation.AddError($"{@this} {this.M.SalesInvoice.ShipToCustomer} {ErrorMessages.PartyIsNotACustomer}");
                }

                @this.PreviousBillToCustomer = @this.BillToCustomer;
                @this.PreviousShipToCustomer = @this.ShipToCustomer;
            }
        }
    }
}
