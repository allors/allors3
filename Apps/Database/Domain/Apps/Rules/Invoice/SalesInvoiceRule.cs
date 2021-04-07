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

    public class SalesInvoiceRule : Rule
    {
        public SalesInvoiceRule(MetaPopulation m) : base(m, new Guid("5F9E688C-1805-4982-87EC-CE45100BDD30")) =>
            this.Patterns = new Pattern[]
        {
            //new RolePattern(m.SalesInvoice, m.SalesInvoice.BilledFrom),
            //new RolePattern(m.SalesInvoice, m.SalesInvoice.Store),
            //new RolePattern(m.SalesInvoice, m.SalesInvoice.BillToCustomer),
            //new RolePattern(m.SalesInvoice, m.SalesInvoice.BillToEndCustomer),
            //new RolePattern(m.SalesInvoice, m.SalesInvoice.ShipToCustomer),
            //new RolePattern(m.SalesInvoice, m.SalesInvoice.ShipToEndCustomer),
            //new RolePattern(m.SalesInvoice, m.SalesInvoice.InvoiceDate),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.SalesInvoiceItems),
            //new RolePattern(m.RepeatingSalesInvoice, m.RepeatingSalesInvoice.NextExecutionDate) { Steps =  new IPropertyType[] {m.RepeatingSalesInvoice.Source} },
            //new RolePattern(m.RepeatingSalesInvoice, m.RepeatingSalesInvoice.FinalExecutionDate) { Steps =  new IPropertyType[] {m.RepeatingSalesInvoice.Source} },
            //new RolePattern(m.InvoiceTerm, m.InvoiceTerm.TermValue) { Steps =  new IPropertyType[] {m.InvoiceTerm.InvoiceWhereSalesTerm} },
            //new RolePattern(m.CustomerRelationship, m.CustomerRelationship.FromDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesInvoicesWhereBillToCustomer} },
            //new RolePattern(m.CustomerRelationship, m.CustomerRelationship.ThroughDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesInvoicesWhereBillToCustomer} },
            //new RolePattern(m.CustomerRelationship, m.CustomerRelationship.FromDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesInvoicesWhereShipToCustomer} },
            //new RolePattern(m.CustomerRelationship, m.CustomerRelationship.ThroughDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesInvoicesWhereShipToCustomer} },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>())
            {
                foreach (SalesInvoiceItem invoiceItem in @this.SalesInvoiceItems)
                {
                    invoiceItem.Sync(@this);
                }

                @this.ResetPrintDocument();
            }
        }
    }
}
