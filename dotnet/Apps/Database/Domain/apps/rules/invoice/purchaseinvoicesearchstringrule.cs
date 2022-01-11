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

    public class PurchaseInvoiceSearchStringRule : Rule
    {
        public PurchaseInvoiceSearchStringRule(MetaPopulation m) : base(m, new Guid("e380a5f7-9a6b-44ef-b153-ffda9e6a0597")) =>
            this.Patterns = new Pattern[]
            {
                m.Invoice.RolePattern(v => v.InternalComment, m.PurchaseInvoice),
                m.Invoice.RolePattern(v => v.DerivedCurrency, m.PurchaseInvoice),
                m.Invoice.RolePattern(v => v.Description, m.PurchaseInvoice),
                m.Invoice.RolePattern(v => v.CustomerReference, m.PurchaseInvoice),
                m.Invoice.RolePattern(v => v.InvoiceNumber, m.PurchaseInvoice),
                m.Invoice.RolePattern(v => v.Message, m.PurchaseInvoice),
                m.Invoice.RolePattern(v => v.SalesTerms, m.PurchaseInvoice),
                m.SalesTerm.RolePattern(v => v.TermType, v => v.InvoiceWhereSalesTerm.Invoice, m.PurchaseInvoice),
                m.Invoice.RolePattern(v => v.DerivedVatRegime, m.PurchaseInvoice),
                m.Invoice.RolePattern(v => v.DerivedIrpfRegime, m.PurchaseInvoice),
                m.Invoice.RolePattern(v => v.ValidInvoiceItems, m.PurchaseInvoice),
                m.InvoiceItem.RolePattern(v => v.InternalComment, v => v.InvoiceWhereValidInvoiceItem.Invoice, m.PurchaseInvoice),
                m.InvoiceItem.RolePattern(v => v.Message, v => v.InvoiceWhereValidInvoiceItem.Invoice, m.PurchaseInvoice),
                m.InvoiceItem.RolePattern(v => v.Description, v => v.InvoiceWhereValidInvoiceItem.Invoice, m.PurchaseInvoice),
                m.InvoiceItem.RolePattern(v => v.DerivedIrpfRegime, v => v.InvoiceWhereValidInvoiceItem.Invoice, m.PurchaseInvoice),
                m.InvoiceItem.RolePattern(v => v.DerivedVatRegime, v => v.InvoiceWhereValidInvoiceItem.Invoice, m.PurchaseInvoice),
                m.InvoiceItem.RolePattern(v => v.SalesTerms, v => v.InvoiceWhereValidInvoiceItem.Invoice, m.PurchaseInvoice),
                m.SalesTerm.RolePattern(v => v.TermType, v => v.InvoiceItemWhereSalesTerm.InvoiceItem.InvoiceWhereValidInvoiceItem.Invoice, m.PurchaseInvoice),

                m.PurchaseInvoice.RolePattern(v => v.PurchaseInvoiceState),
                m.PurchaseInvoice.RolePattern(v => v.BilledFrom),
                m.PurchaseInvoice.RolePattern(v => v.DerivedBilledFromContactMechanism),
                m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.PurchaseInvoicesWhereDerivedBilledFromContactMechanism.PurchaseInvoice),
                m.PurchaseInvoice.RolePattern(v => v.BilledFromContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.PurchaseInvoicesWhereBilledFromContactPerson.PurchaseInvoice),
                m.PurchaseInvoice.RolePattern(v => v.BilledTo),
                m.PurchaseInvoice.RolePattern(v => v.BilledToContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.PurchaseInvoicesWhereBilledToContactPerson.PurchaseInvoice),
                m.PurchaseInvoice.RolePattern(v => v.BillToEndCustomer),
                m.Party.RolePattern(v => v.DisplayName, v => v.PurchaseInvoicesWhereBillToEndCustomer.PurchaseInvoice),
                m.PurchaseInvoice.RolePattern(v => v.DerivedBillToEndCustomerContactMechanism),
                m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.PurchaseInvoicesWhereDerivedBillToEndCustomerContactMechanism.PurchaseInvoice),
                m.PurchaseInvoice.RolePattern(v => v.BillToEndCustomerContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.PurchaseInvoicesWhereBillToEndCustomerContactPerson.PurchaseInvoice),
                m.PurchaseInvoice.RolePattern(v => v.ShipToCustomer),
                m.Party.RolePattern(v => v.DisplayName, v => v.PurchaseInvoicesWhereShipToCustomer.PurchaseInvoice),
                m.PurchaseInvoice.RolePattern(v => v.DerivedShipToCustomerAddress),
                m.PostalAddress.RolePattern(v => v.DisplayName, v => v.PurchaseInvoicesWhereDerivedShipToCustomerAddress.PurchaseInvoice),
                m.PurchaseInvoice.RolePattern(v => v.ShipToCustomerContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.PurchaseInvoicesWhereShipToCustomerContactPerson.PurchaseInvoice),
                m.PurchaseInvoice.RolePattern(v => v.ShipToEndCustomer),
                m.Party.RolePattern(v => v.DisplayName, v => v.PurchaseInvoicesWhereShipToEndCustomer.PurchaseInvoice),
                m.PurchaseInvoice.RolePattern(v => v.DerivedShipToEndCustomerAddress),
                m.PostalAddress.RolePattern(v => v.DisplayName, v => v.PurchaseInvoicesWhereDerivedShipToEndCustomerAddress.PurchaseInvoice),
                m.PurchaseInvoice.RolePattern(v => v.ShipToEndCustomerContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.PurchaseInvoicesWhereShipToEndCustomerContactPerson.PurchaseInvoice),
                m.PurchaseInvoice.RolePattern(v => v.PurchaseInvoiceType),
                m.PurchaseInvoice.RolePattern(v => v.DerivedBillToCustomerPaymentMethod),
                m.PurchaseInvoice.RolePattern(v => v.PurchaseOrders),

                m.PurchaseInvoiceItem.RolePattern(v => v.Part, v => v.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoice),
                m.PurchaseInvoiceItem.RolePattern(v => v.SerialisedItem, v => v.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoice),
                m.PurchaseInvoiceItem.RolePattern(v => v.InvoiceItemType, v => v.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoice),
                m.PurchaseInvoiceItem.RolePattern(v => v.PurchaseInvoiceItemState, v => v.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoice),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseInvoice>())
            {
                @this.DerivePurchaseInvoiceSearchString(validation);
            }
        }
    }

    public static class PurchaseInvoiceSearchStringRuleExtensions
    {
        public static void DerivePurchaseInvoiceSearchString(this PurchaseInvoice @this, IValidation validation)
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
                    @this.ExistSalesTerms ? string.Join(" ", @this.SalesTerms?.Select(v => v.Description ?? string.Empty).ToArray()) : null,
                    @this.ExistSalesTerms ? string.Join(" ", @this.SalesTerms?.Select(v => v.TermType?.Name ?? string.Empty).ToArray()) : null,
                    @this.PurchaseInvoiceState?.Name,
                    @this.BilledFrom?.DisplayName,
                    @this.DerivedBilledFromContactMechanism?.DisplayName,
                    @this.BilledFromContactPerson?.DisplayName,
                    @this.BilledTo?.DisplayName,
                    @this.BilledToContactPerson?.DisplayName,
                    @this.BillToEndCustomer?.DisplayName,
                    @this.DerivedBillToEndCustomerContactMechanism?.DisplayName,
                    @this.BillToEndCustomerContactPerson?.DisplayName,
                    @this.ShipToCustomer?.DisplayName,
                    @this.DerivedShipToCustomerAddress?.DisplayName,
                    @this.ShipToCustomerContactPerson?.DisplayName,
                    @this.ShipToEndCustomer?.DisplayName,
                    @this.DerivedShipToEndCustomerAddress?.DisplayName,
                    @this.ShipToEndCustomerContactPerson?.DisplayName,
                    @this.PurchaseInvoiceType?.Name,
                    @this.DerivedBillToCustomerPaymentMethod?.Description,
                    @this.ExistPurchaseOrders ? string.Join(" ", @this.PurchaseOrders?.Select(v => v.OrderNumber ?? string.Empty).ToArray()) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.ValidInvoiceItems?.Select(v => v.InternalComment ?? string.Empty).ToArray()) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.ValidInvoiceItems?.Select(v => v.Message ?? string.Empty).ToArray()) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.ValidInvoiceItems?.Select(v => v.Description ?? string.Empty).ToArray()) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.ValidInvoiceItems?.Select(v => v.DerivedIrpfRegime?.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.ValidInvoiceItems?.Select(v => v.DerivedVatRegime?.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.ValidInvoiceItems?.SelectMany(v => v.SalesTerms?.Select(v => v.Description ?? string.Empty)).ToArray()) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.ValidInvoiceItems?.SelectMany(v => v.SalesTerms?.Select(v => v.TermType?.Name ?? string.Empty)).ToArray()) : null,
                    @this.ExistPurchaseInvoiceItems ? string.Join(" ", @this.PurchaseInvoiceItems?.Select(v => v.Part?.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistPurchaseInvoiceItems ? string.Join(" ", @this.PurchaseInvoiceItems?.Select(v => v.SerialisedItem?.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistPurchaseInvoiceItems ? string.Join(" ", @this.PurchaseInvoiceItems?.Select(v => v.InvoiceItemType?.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistPurchaseInvoiceItems ? string.Join(" ", @this.PurchaseInvoiceItems?.Select(v => v.PurchaseInvoiceItemState?.Name ?? string.Empty).ToArray()) : null,
                };

            if (array.Any(s => !string.IsNullOrEmpty(s)))
            {
                @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
            else
            {
                @this.RemoveSearchString();
            }
        }
    }
}
