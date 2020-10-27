// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;
    using Resources;

    public class SalesInvoiceDerivation : DomainDerivation
    {
        public SalesInvoiceDerivation(M m) : base(m, new Guid("5F9E688C-1805-4982-87EC-CE45100BDD30")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.SalesInvoice.BilledFrom),
            new ChangedPattern(this.M.SalesInvoice.Store),
            new ChangedPattern(this.M.SalesInvoice.BillToCustomer),
            new ChangedPattern(this.M.SalesInvoice.BillToEndCustomer),
            new ChangedPattern(this.M.SalesInvoice.ShipToCustomer),
            new ChangedPattern(this.M.SalesInvoice.ShipToEndCustomer),
            new ChangedPattern(this.M.SalesInvoice.VatRegime),
            new ChangedPattern(this.M.SalesInvoice.AssignedVatClause),
            new ChangedPattern(this.M.SalesInvoice.InvoiceDate),
            new ChangedPattern(this.M.SalesInvoice.SalesInvoiceItems),
            new ChangedPattern(this.M.RepeatingSalesInvoice.NextExecutionDate) { Steps =  new IPropertyType[] {m.RepeatingSalesInvoice.Source} },
            new ChangedPattern(this.M.RepeatingSalesInvoice.FinalExecutionDate) { Steps =  new IPropertyType[] {m.RepeatingSalesInvoice.Source} },
            new ChangedPattern(this.M.InvoiceTerm.TermValue) { Steps =  new IPropertyType[] {m.InvoiceTerm.InvoiceWhereSalesTerm} },
            new ChangedPattern(this.M.CustomerRelationship.FromDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesInvoicesWhereBillToCustomer} },
            new ChangedPattern(this.M.CustomerRelationship.ThroughDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesInvoicesWhereBillToCustomer} },
            new ChangedPattern(this.M.CustomerRelationship.FromDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesInvoicesWhereShipToCustomer} },
            new ChangedPattern(this.M.CustomerRelationship.ThroughDate) { Steps =  new IPropertyType[] {m.CustomerRelationship.Customer, m.Party.SalesInvoicesWhereShipToCustomer} },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var salesInvoice in matches.Cast<SalesInvoice>())
            {
                if (salesInvoice.ExistBillToCustomer)
                {
                    salesInvoice.PreviousBillToCustomer = salesInvoice.BillToCustomer;
                }

                if (!salesInvoice.ExistStore && salesInvoice.ExistBilledFrom)
                {
                    var stores = new Stores(session).Extent();
                    stores.Filter.AddEquals(this.M.Store.InternalOrganisation, salesInvoice.BilledFrom);
                    salesInvoice.Store = stores.FirstOrDefault();
                }

                if (!salesInvoice.ExistInvoiceNumber && salesInvoice.ExistStore)
                {
                    salesInvoice.InvoiceNumber = salesInvoice.Store.NextTemporaryInvoiceNumber();
                    salesInvoice.SortableInvoiceNumber = salesInvoice.Session().GetSingleton().SortableNumber(null, salesInvoice.InvoiceNumber, salesInvoice.InvoiceDate.Year.ToString());
                }

                if (!salesInvoice.ExistBilledFromContactMechanism && salesInvoice.ExistBilledFrom)
                {
                    salesInvoice.BilledFromContactMechanism = salesInvoice.BilledFrom.ExistBillingAddress ? salesInvoice.BilledFrom.BillingAddress : salesInvoice.BilledFrom.GeneralCorrespondence;
                }

                if (!salesInvoice.ExistBillToContactMechanism && salesInvoice.ExistBillToCustomer)
                {
                    salesInvoice.BillToContactMechanism = salesInvoice.BillToCustomer.BillingAddress;
                }

                if (!salesInvoice.ExistBillToEndCustomerContactMechanism && salesInvoice.ExistBillToEndCustomer)
                {
                    salesInvoice.BillToEndCustomerContactMechanism = salesInvoice.BillToEndCustomer.BillingAddress;
                }

                if (!salesInvoice.ExistShipToEndCustomerAddress && salesInvoice.ExistShipToEndCustomer)
                {
                    salesInvoice.ShipToEndCustomerAddress = salesInvoice.ShipToEndCustomer.ShippingAddress;
                }

                if (!salesInvoice.ExistShipToAddress && salesInvoice.ExistShipToCustomer)
                {
                    salesInvoice.ShipToAddress = salesInvoice.ShipToCustomer.ShippingAddress;
                }

                if (salesInvoice.ExistBillToCustomer && salesInvoice.BillToCustomer.ExistLocale)
                {
                    salesInvoice.Locale = salesInvoice.BillToCustomer.Locale;
                }
                else
                {
                    salesInvoice.Locale = salesInvoice.DefaultLocale;
                }

                if (!salesInvoice.ExistCurrency
                    && (salesInvoice.ExistBilledFrom || salesInvoice.ExistBillToCustomer))
                {
                    if (salesInvoice.ExistBillToCustomer && (salesInvoice.BillToCustomer.ExistPreferredCurrency || salesInvoice.BillToCustomer.ExistLocale))
                    {
                        salesInvoice.Currency = salesInvoice.BillToCustomer.ExistPreferredCurrency ? salesInvoice.BillToCustomer.PreferredCurrency : salesInvoice.BillToCustomer.Locale.Country.Currency;
                    }
                    else
                    {
                        salesInvoice.Currency = salesInvoice.BilledFrom.ExistPreferredCurrency ?
                            salesInvoice.BilledFrom.PreferredCurrency :
                            salesInvoice.DefaultCurrency;
                    }
                }

                salesInvoice.VatRegime ??= salesInvoice.BillToCustomer?.VatRegime;
                salesInvoice.IrpfRegime ??= salesInvoice.BillToCustomer?.IrpfRegime;
                salesInvoice.IsRepeatingInvoice = salesInvoice.ExistRepeatingSalesInvoiceWhereSource
                        && (!salesInvoice.RepeatingSalesInvoiceWhereSource.ExistFinalExecutionDate
                            || salesInvoice.RepeatingSalesInvoiceWhereSource.FinalExecutionDate.Value.Date >= salesInvoice.Strategy.Session.Now().Date);

                foreach (SalesInvoiceItem salesInvoiceItem in salesInvoice.SalesInvoiceItems)
                {
                    foreach (OrderItemBilling orderItemBilling in salesInvoiceItem.OrderItemBillingsWhereInvoiceItem)
                    {
                        if (orderItemBilling.OrderItem is SalesOrderItem salesOrderItem && !salesInvoice.SalesOrders.Contains(salesOrderItem.SalesOrderWhereSalesOrderItem))
                        {
                            salesInvoice.AddSalesOrder(salesOrderItem.SalesOrderWhereSalesOrderItem);
                        }
                    }

                    foreach (WorkEffortBilling workEffortBilling in salesInvoiceItem.WorkEffortBillingsWhereInvoiceItem)
                    {
                        if (!salesInvoice.WorkEfforts.Contains(workEffortBilling.WorkEffort))
                        {
                            salesInvoice.AddWorkEffort(workEffortBilling.WorkEffort);
                        }
                    }

                    foreach (TimeEntryBilling timeEntryBilling in salesInvoiceItem.TimeEntryBillingsWhereInvoiceItem)
                    {
                        if (!salesInvoice.WorkEfforts.Contains(timeEntryBilling.TimeEntry.WorkEffort))
                        {
                            salesInvoice.AddWorkEffort(timeEntryBilling.TimeEntry.WorkEffort);
                        }
                    }
                }

                salesInvoice.PaymentDays = salesInvoice.PaymentNetDays;

                if (salesInvoice.ExistInvoiceDate)
                {
                    salesInvoice.DueDate = salesInvoice.InvoiceDate.AddDays(salesInvoice.PaymentNetDays);
                }

                if (salesInvoice.ExistVatRegime)
                {
                    if (salesInvoice.VatRegime.ExistVatClause)
                    {
                        salesInvoice.DerivedVatClause = salesInvoice.VatRegime.VatClause;
                    }
                    else
                    {
                        salesInvoice.RemoveDerivedVatClause();
                    }
                }
                else
                {
                    salesInvoice.RemoveDerivedVatClause();
                }

                salesInvoice.DerivedVatClause = salesInvoice.ExistAssignedVatClause ? salesInvoice.AssignedVatClause : salesInvoice.DerivedVatClause;

                salesInvoice.RemoveCustomers();
                if (salesInvoice.ExistBillToCustomer && !salesInvoice.Customers.Contains(salesInvoice.BillToCustomer))
                {
                    salesInvoice.AddCustomer(salesInvoice.BillToCustomer);
                }

                if (salesInvoice.ExistShipToCustomer && !salesInvoice.Customers.Contains(salesInvoice.ShipToCustomer))
                {
                    salesInvoice.AddCustomer(salesInvoice.ShipToCustomer);
                }

                if (salesInvoice.ExistBillToCustomer
                    && salesInvoice.ExistBilledFrom
                    && !salesInvoice.BillToCustomer.AppsIsActiveCustomer(salesInvoice.BilledFrom, salesInvoice.InvoiceDate))
                {
                    validation.AddError($"{salesInvoice} {this.M.SalesInvoice.BillToCustomer} {ErrorMessages.PartyIsNotACustomer}");
                }

                if (salesInvoice.ExistShipToCustomer
                    && salesInvoice.ExistBilledFrom
                    && !salesInvoice.ShipToCustomer.AppsIsActiveCustomer(salesInvoice.BilledFrom, salesInvoice.InvoiceDate))
                {
                    validation.AddError($"{salesInvoice} {this.M.SalesInvoice.ShipToCustomer} {ErrorMessages.PartyIsNotACustomer}");
                }

                salesInvoice.PreviousBillToCustomer = salesInvoice.BillToCustomer;
                salesInvoice.PreviousShipToCustomer = salesInvoice.ShipToCustomer;

                foreach (SalesInvoiceItem invoiceItem in salesInvoice.SalesInvoiceItems)
                {
                    invoiceItem.Sync(salesInvoice);
                }

                salesInvoice.ResetPrintDocument();
            }
        }
    }
}
