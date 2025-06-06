// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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
                m.SalesTerm.RolePattern(v => v.TermType, v => v.InvoiceWhereSalesTerm.ObjectType, m.SalesInvoice),
                m.Invoice.RolePattern(v => v.DerivedVatRegime, m.SalesInvoice),
                m.Invoice.RolePattern(v => v.DerivedIrpfRegime, m.SalesInvoice),
                m.Invoice.RolePattern(v => v.ValidInvoiceItems, m.SalesInvoice),
                m.InvoiceItem.RolePattern(v => v.InternalComment, v => v.InvoiceWhereValidInvoiceItem.ObjectType, m.SalesInvoice),
                m.InvoiceItem.RolePattern(v => v.Message, v => v.InvoiceWhereValidInvoiceItem.ObjectType, m.SalesInvoice),
                m.InvoiceItem.RolePattern(v => v.Description, v => v.InvoiceWhereValidInvoiceItem.ObjectType, m.SalesInvoice),
                m.InvoiceItem.RolePattern(v => v.DerivedIrpfRegime, v => v.InvoiceWhereValidInvoiceItem.ObjectType, m.SalesInvoice),
                m.InvoiceItem.RolePattern(v => v.DerivedVatRegime, v => v.InvoiceWhereValidInvoiceItem.ObjectType, m.SalesInvoice),
                m.InvoiceItem.RolePattern(v => v.SalesTerms, v => v.InvoiceWhereValidInvoiceItem.ObjectType, m.SalesInvoice),
                m.SalesTerm.RolePattern(v => v.TermType, v => v.InvoiceItemWhereSalesTerm.ObjectType.InvoiceWhereValidInvoiceItem.ObjectType, m.SalesInvoice),

                m.SalesInvoice.RolePattern(v => v.SalesInvoiceState),
                m.SalesInvoice.RolePattern(v => v.BilledFrom),
                m.SalesInvoice.RolePattern(v => v.BilledFromContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereBilledFromContactPerson.ObjectType),
                m.SalesInvoice.RolePattern(v => v.BillToCustomer),
                m.Party.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereBillToCustomer.ObjectType),
                m.SalesInvoice.RolePattern(v => v.DerivedBillToContactMechanism),
                m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereDerivedBillToContactMechanism.ObjectType),
                m.SalesInvoice.RolePattern(v => v.BillToContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereBillToContactPerson.ObjectType),
                m.SalesInvoice.RolePattern(v => v.BillToEndCustomer),
                m.Party.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereBillToEndCustomer.ObjectType),
                m.SalesInvoice.RolePattern(v => v.DerivedBillToEndCustomerContactMechanism),
                m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereDerivedBillToEndCustomerContactMechanism.ObjectType),
                m.SalesInvoice.RolePattern(v => v.BillToEndCustomerContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereBillToEndCustomerContactPerson.ObjectType),
                m.SalesInvoice.RolePattern(v => v.ShipToCustomer),
                m.Party.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereShipToCustomer.ObjectType),
                m.SalesInvoice.RolePattern(v => v.DerivedShipToAddress),
                m.PostalAddress.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereDerivedShipToAddress.ObjectType),
                m.SalesInvoice.RolePattern(v => v.ShipToContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereShipToContactPerson.ObjectType),
                m.SalesInvoice.RolePattern(v => v.ShipToEndCustomer),
                m.Party.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereShipToEndCustomer.ObjectType),
                m.SalesInvoice.RolePattern(v => v.DerivedShipToEndCustomerAddress),
                m.PostalAddress.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereDerivedShipToEndCustomerAddress.ObjectType),
                m.SalesInvoice.RolePattern(v => v.ShipToEndCustomerContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.SalesInvoicesWhereShipToEndCustomerContactPerson.ObjectType),
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

                m.SalesInvoiceItem.RolePattern(v => v.Product, v => v.SalesInvoiceWhereSalesInvoiceItem.ObjectType),
                m.SalesInvoiceItem.RolePattern(v => v.Part, v => v.SalesInvoiceWhereSalesInvoiceItem.ObjectType),
                m.SalesInvoiceItem.RolePattern(v => v.SerialisedItem, v => v.SalesInvoiceWhereSalesInvoiceItem.ObjectType),
                m.SalesInvoiceItem.RolePattern(v => v.ProductFeatures, v => v.SalesInvoiceWhereSalesInvoiceItem.ObjectType),
                m.SalesInvoiceItem.RolePattern(v => v.InvoiceItemType, v => v.SalesInvoiceWhereSalesInvoiceItem.ObjectType),
                m.SalesInvoiceItem.RolePattern(v => v.SalesInvoiceItemState, v => v.SalesInvoiceWhereSalesInvoiceItem.ObjectType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>())
            {
                @this.DeriveSalesInvoiceSearchString(validation);
            }
        }
    }

    public static class SalesInvoiceSearchStringRuleExtensions
    {
        public static void DeriveSalesInvoiceSearchString(this SalesInvoice @this, IValidation validation)
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
                    string.Join(" ", @this.SalesTerms?.Select(v => v.Description).ToArray()),
                    string.Join(" ", @this.SalesTerms?.Select(v => v.TermType?.Name).ToArray()),
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
                    @this.ExistSalesOrders ? string.Join(" ", @this.SalesOrders?.Select(v => v.OrderNumber ?? string.Empty).ToArray()) : null,
                    @this.ExistShipments ? string.Join(" ", @this.Shipments?.Select(v => v.ShipmentNumber ?? string.Empty).ToArray()) : null,
                    @this.ExistWorkEfforts ? string.Join(" ", @this.WorkEfforts?.Select(v => v.WorkEffortNumber ?? string.Empty).ToArray()) : null,
                    @this.IsRepeatingInvoice ? "repeating" : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.ValidInvoiceItems?.Select(v => v.InternalComment ?? string.Empty).ToArray()) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.ValidInvoiceItems?.Select(v => v.Message ?? string.Empty).ToArray()) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.ValidInvoiceItems?.Select(v => v.Description ?? string.Empty).ToArray()) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.ValidInvoiceItems?.Select(v => v.DerivedIrpfRegime?.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.ValidInvoiceItems?.Select(v => v.DerivedVatRegime?.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.ValidInvoiceItems?.SelectMany(v => v.SalesTerms?.Select(v => v.Description ?? string.Empty)).ToArray()) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.ValidInvoiceItems?.SelectMany(v => v.SalesTerms?.Select(v => v.TermType?.Name ?? string.Empty)).ToArray()) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.SalesInvoiceItems?.Select(v => v.Product?.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.SalesInvoiceItems?.Select(v => v.Part?.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.SalesInvoiceItems?.Select(v => v.SerialisedItem?.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.SalesInvoiceItems?.Select(v => v.InvoiceItemType?.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.SalesInvoiceItems?.SelectMany(v => v.ProductFeatures?.Select(v => v.Description ?? string.Empty)).ToArray()) : null,
                    @this.ExistValidInvoiceItems ? string.Join(" ", @this.SalesInvoiceItems?.Select(v => v.SalesInvoiceItemState?.Name ?? string.Empty).ToArray()) : null,
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
