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

    public class SalesInvoiceDerivation : DomainDerivation
    {
        public SalesInvoiceDerivation(M m) : base(m, new Guid("5F9E688C-1805-4982-87EC-CE45100BDD30")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.SalesInvoice, m.SalesInvoice.BilledFrom),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.Store),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.BillToCustomer),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.BillToEndCustomer),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.ShipToCustomer),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.ShipToEndCustomer),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.InvoiceDate),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.SalesInvoiceItems),
            new RolePattern(m.RepeatingSalesInvoice, m.RepeatingSalesInvoice.NextExecutionDate) { Steps =  new IPropertyType[] {m.RepeatingSalesInvoice.Source} },
            new RolePattern(m.RepeatingSalesInvoice, m.RepeatingSalesInvoice.FinalExecutionDate) { Steps =  new IPropertyType[] {m.RepeatingSalesInvoice.Source} },
            new RolePattern(m.InvoiceTerm, m.InvoiceTerm.TermValue) { Steps =  new IPropertyType[] {m.InvoiceTerm.InvoiceWhereSalesTerm} },
            new RolePattern(m.CustomerRelationship, m.CustomerRelationship.FromDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesInvoicesWhereBillToCustomer} },
            new RolePattern(m.CustomerRelationship, m.CustomerRelationship.ThroughDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesInvoicesWhereBillToCustomer} },
            new RolePattern(m.CustomerRelationship, m.CustomerRelationship.FromDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesInvoicesWhereShipToCustomer} },
            new RolePattern(m.CustomerRelationship, m.CustomerRelationship.ThroughDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesInvoicesWhereShipToCustomer} },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>())
            {
                if (@this.ExistCurrentVersion
                    && @this.CurrentVersion.ExistBilledFrom
                    && @this.BilledFrom != @this.CurrentVersion.BilledFrom)
                {
                    validation.AddError($"{@this} {this.M.SalesInvoice.BilledFrom} {ErrorMessages.InternalOrganisationChanged}");
                }

                if (@this.ExistBillToCustomer)
                {
                    @this.PreviousBillToCustomer = @this.BillToCustomer;
                }

                if (!@this.ExistStore && @this.ExistBilledFrom)
                {
                    var stores = @this.BilledFrom.StoresWhereInternalOrganisation;
                    @this.Store = stores.FirstOrDefault();
                }

                if (!@this.ExistInvoiceNumber && @this.ExistStore)
                {
                    @this.InvoiceNumber = @this.Store.NextTemporaryInvoiceNumber();
                    @this.SortableInvoiceNumber = NumberFormatter.SortableNumber(null, @this.InvoiceNumber, @this.InvoiceDate.Year.ToString());
                }

                @this.IsRepeatingInvoice = @this.ExistRepeatingSalesInvoiceWhereSource
                        && (!@this.RepeatingSalesInvoiceWhereSource.ExistFinalExecutionDate
                            || @this.RepeatingSalesInvoiceWhereSource.FinalExecutionDate.Value.Date >= @this.Strategy.Transaction.Now().Date);

                @this.PaymentDays = @this.PaymentNetDays;

                if (@this.ExistInvoiceDate)
                {
                    @this.DueDate = @this.InvoiceDate.AddDays(@this.PaymentNetDays);
                }

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

                foreach (SalesInvoiceItem invoiceItem in @this.SalesInvoiceItems)
                {
                    invoiceItem.Sync(@this);
                }

                @this.ResetPrintDocument();
            }
        }
    }
}
