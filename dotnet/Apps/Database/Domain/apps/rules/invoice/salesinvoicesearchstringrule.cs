// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class SalesInvoiceSearchStringRule : Rule
    {
        public SalesInvoiceSearchStringRule(MetaPopulation m) : base(m, new Guid("80ac3b41-a764-4124-acb6-eff0f1566c90")) =>
            this.Patterns = new Pattern[]
            {
                m.Invoice.RolePattern(v => v.InternalComment, m.SalesInvoice),
                m.Invoice.RolePattern(v => v.DerivedCurrency, m.SalesInvoice),
                m.Invoice.RolePattern(v => v.Description, m.SalesInvoice),
                m.Invoice.RolePattern(v => v.CustomerReference, m.SalesInvoice),
                m.Invoice.RolePattern(v => v.InvoiceNumber, m.SalesInvoice),
                m.Invoice.RolePattern(v => v.Message, m.SalesInvoice),
                m.Invoice.RolePattern(v => v.SalesTerms, m.SalesInvoice),
                m.SalesTerm.RolePattern(v => v.TermType, v => v.InvoiceWhereSalesTerm.Invoice, m.SalesInvoice),
                m.Invoice.RolePattern(v => v.DerivedVatRegime, m.SalesInvoice),
                m.Invoice.RolePattern(v => v.DerivedIrpfRegime, m.SalesInvoice),
                m.Invoice.RolePattern(v => v.ValidInvoiceItems, m.SalesInvoice),
                m.InvoiceItem.RolePattern(v => v.InternalComment, v => v.InvoiceWhereValidInvoiceItem.Invoice, m.SalesInvoice),
                m.InvoiceItem.RolePattern(v => v.Message, v => v.InvoiceWhereValidInvoiceItem.Invoice, m.SalesInvoice),
                m.InvoiceItem.RolePattern(v => v.Description, v => v.InvoiceWhereValidInvoiceItem.Invoice, m.SalesInvoice),
                m.InvoiceItem.RolePattern(v => v.DerivedIrpfRegime, v => v.InvoiceWhereValidInvoiceItem.Invoice, m.SalesInvoice),
                m.InvoiceItem.RolePattern(v => v.DerivedVatRegime, v => v.InvoiceWhereValidInvoiceItem.Invoice, m.SalesInvoice),
                m.InvoiceItem.RolePattern(v => v.SalesTerms, v => v.InvoiceWhereValidInvoiceItem.Invoice, m.SalesInvoice),
                m.SalesTerm.RolePattern(v => v.TermType, v => v.InvoiceItemWhereSalesTerm.InvoiceItem.InvoiceWhereValidInvoiceItem.Invoice, m.SalesInvoice),

                m.SalesInvoice.RolePattern(v => v.SalesInvoiceState),
                m.SalesInvoice.RolePattern(v => v.BilledFrom),
                m.SalesInvoice.RolePattern(v => v.BilledFromContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereBilledFromContactPerson.SalesInvoice),
                m.SalesInvoice.RolePattern(v => v.BillToCustomer),
                m.Party.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereBillToCustomer.SalesInvoice),
                m.SalesInvoice.RolePattern(v => v.DerivedBillToContactMechanism),
                m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereDerivedBillToContactMechanism.SalesInvoice),
                m.SalesInvoice.RolePattern(v => v.BillToContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereBillToContactPerson.SalesInvoice),
                m.SalesInvoice.RolePattern(v => v.BillToEndCustomer),
                m.Party.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereBillToEndCustomer.SalesInvoice),
                m.SalesInvoice.RolePattern(v => v.DerivedBillToEndCustomerContactMechanism),
                m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereDerivedBillToEndCustomerContactMechanism.SalesInvoice),
                m.SalesInvoice.RolePattern(v => v.BillToEndCustomerContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereBillToEndCustomerContactPerson.SalesInvoice),
                m.SalesInvoice.RolePattern(v => v.ShipToCustomer),
                m.Party.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereShipToCustomer.SalesInvoice),
                m.SalesInvoice.RolePattern(v => v.DerivedShipToAddress),
                m.PostalAddress.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereDerivedShipToAddress.SalesInvoice),
                m.SalesInvoice.RolePattern(v => v.ShipToContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereShipToContactPerson.SalesInvoice),
                m.SalesInvoice.RolePattern(v => v.ShipToEndCustomer),
                m.Party.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereShipToEndCustomer.SalesInvoice),
                m.SalesInvoice.RolePattern(v => v.DerivedShipToEndCustomerAddress),
                m.PostalAddress.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereDerivedShipToEndCustomerAddress.SalesInvoice),
                m.SalesInvoice.RolePattern(v => v.ShipToEndCustomerContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereShipToEndCustomerContactPerson.SalesInvoice),
                m.SalesInvoice.RolePattern(v => v.SalesInvoiceType),
                m.SalesInvoice.RolePattern(v => v.DerivedPaymentMethod),
                m.SalesInvoice.RolePattern(v => v.DerivedVatClause),
                m.SalesInvoice.RolePattern(v => v.SalesChannel),
                m.SalesInvoice.RolePattern(v => v.Store),
                m.SalesInvoice.RolePattern(v => v.CreditedFromInvoice),
                m.SalesInvoice.RolePattern(v => v.PurchaseInvoice),
                m.SalesInvoice.RolePattern(v => v.SalesOrders),
                m.SalesInvoice.RolePattern(v => v.Shipments),
                m.SalesInvoice.RolePattern(v => v.WorkEfforts),
                m.SalesInvoice.RolePattern(v => v.IsRepeatingInvoice),

                m.SalesInvoiceItem.RolePattern(v => v.Product, v => v.SalesInvoiceWhereSalesInvoiceItem.SalesInvoice),
                m.SalesInvoiceItem.RolePattern(v => v.Part, v => v.SalesInvoiceWhereSalesInvoiceItem.SalesInvoice),
                m.SalesInvoiceItem.RolePattern(v => v.SerialisedItem, v => v.SalesInvoiceWhereSalesInvoiceItem.SalesInvoice),
                m.SalesInvoiceItem.RolePattern(v => v.ProductFeatures, v => v.SalesInvoiceWhereSalesInvoiceItem.SalesInvoice),
                m.SalesInvoiceItem.RolePattern(v => v.InvoiceItemType, v => v.SalesInvoiceWhereSalesInvoiceItem.SalesInvoice),
                m.SalesInvoiceItem.RolePattern(v => v.SalesInvoiceItemState, v => v.SalesInvoiceWhereSalesInvoiceItem.SalesInvoice),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SalesInvoice>())
            {
                var array = new string[] {
                    @this.InternalComment,
                    @this.Description,
                    @this.CustomerReference,
                    @this.InvoiceNumber,
                    @this.Message,
                    @this.DerivedCurrency?.Abbreviation,
                    @this.DerivedCurrency?.Name,
                    @this.DerivedVatRegime?.Name,
                    @this.DerivedIrpfRegime?.Name,
                    string.Join(" ", @this.SalesTerms?.Select(v => v.Description)),
                    string.Join(" ", @this.SalesTerms?.Select(v => v.TermType?.Name)),
                    @this.SalesInvoiceState?.Name,
                    @this.BilledFrom?.DisplayName,
                    @this.BilledFromContactPerson?.DisplayName,
                    @this.BillToCustomer?.DisplayName,
                    @this.DerivedBillToContactMechanism?.DisplayName,
                    @this.BillToContactPerson?.DisplayName,
                    @this.BillToEndCustomer?.DisplayName,
                    @this.DerivedBillToEndCustomerContactMechanism?.DisplayName,
                    @this.BillToEndCustomerContactPerson?.DisplayName,
                    @this.ShipToCustomer?.DisplayName,
                    @this.DerivedShipToAddress?.DisplayName,
                    @this.ShipToContactPerson?.DisplayName,
                    @this.ShipToEndCustomer?.DisplayName,
                    @this.DerivedShipToEndCustomerAddress?.DisplayName,
                    @this.ShipToEndCustomerContactPerson?.DisplayName,
                    @this.SalesInvoiceType?.Name,
                    @this.DerivedPaymentMethod?.Description,
                    @this.DerivedVatClause?.Name,
                    @this.SalesChannel?.Name,
                    @this.Store?.Name,
                    @this.CreditedFromInvoice?.InvoiceNumber,
                    @this.PurchaseInvoice?.InvoiceNumber,
                    @this.ExistSalesOrders ? string.Join(" ", @this.SalesOrders?.Select(v => v.OrderNumber)) : null,
                    @this.ExistShipments ? string.Join(" ", @this.Shipments?.Select(v => v.ShipmentNumber)) : null,
                    @this.ExistWorkEfforts ? string.Join(" ", @this.WorkEfforts?.Select(v => v.WorkEffortNumber)) : null,
                    @this.IsRepeatingInvoice ? "repeating" : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.ValidInvoiceItems?.Select(v => v.InternalComment)) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.ValidInvoiceItems?.Select(v => v.Message)) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.ValidInvoiceItems?.Select(v => v.Description)) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.ValidInvoiceItems?.Select(v => v.DerivedIrpfRegime?.Name)) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.ValidInvoiceItems?.Select(v => v.DerivedVatRegime?.Name)) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.ValidInvoiceItems?.Select(v => v.SalesTerms?.Select(v => v.Description))) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.ValidInvoiceItems?.Select(v => v.SalesTerms?.Select(v => v.TermType?.Name))) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.SalesInvoiceItems?.Select(v => v.Product?.DisplayName)) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.SalesInvoiceItems?.Select(v => v.Part?.DisplayName)) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.SalesInvoiceItems?.Select(v => v.SerialisedItem?.DisplayName)) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.SalesInvoiceItems?.Select(v => v.InvoiceItemType?.Name)) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.SalesInvoiceItems?.Select(v => v.ProductFeatures?.Select(v => v.Description))) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.SalesInvoiceItems?.Select(v => v.SalesInvoiceItemState?.Name)) : null,
                };

                if (array.Any(s => !string.IsNullOrEmpty(s)))
                {
                    @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
                }
            }
        }
    }
}
