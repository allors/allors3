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

    public class SalesOrderSearchStringRule : Rule
    {
        public SalesOrderSearchStringRule(MetaPopulation m) : base(m, new Guid("de0a690c-4f77-4642-b707-8b1087b3098e")) =>
            this.Patterns = new Pattern[]
            {
                m.Order.RolePattern(v => v.InternalComment, m.SalesOrder),
                m.Order.RolePattern(v => v.DerivedCurrency, m.SalesOrder),
                m.Order.RolePattern(v => v.Description, m.SalesOrder),
                m.Order.RolePattern(v => v.CustomerReference, m.SalesOrder),
                m.Order.RolePattern(v => v.OrderNumber, m.SalesOrder),
                m.Order.RolePattern(v => v.Message, m.SalesOrder),
                m.Order.RolePattern(v => v.OrderKind, m.SalesOrder),
                m.Order.RolePattern(v => v.SalesTerms, m.SalesOrder),
                m.SalesTerm.RolePattern(v => v.TermType, v => v.OrderWhereSalesTerm.ObjectType, m.SalesOrder),
                m.Order.RolePattern(v => v.DerivedVatRegime, m.SalesOrder),
                m.Order.RolePattern(v => v.DerivedIrpfRegime, m.SalesOrder),
                m.Order.RolePattern(v => v.ValidOrderItems, m.SalesOrder),
                m.OrderItem.RolePattern(v => v.InternalComment, v => v.OrderWhereValidOrderItem.ObjectType, m.SalesOrder),
                m.OrderItem.RolePattern(v => v.Message, v => v.OrderWhereValidOrderItem.ObjectType, m.SalesOrder),
                m.OrderItem.RolePattern(v => v.Description, v => v.OrderWhereValidOrderItem.ObjectType, m.SalesOrder),
                m.OrderItem.RolePattern(v => v.CorrespondingPurchaseOrder, v => v.OrderWhereValidOrderItem.ObjectType, m.SalesOrder),
                m.OrderItem.RolePattern(v => v.QuoteItem, v => v.OrderWhereValidOrderItem.ObjectType, m.SalesOrder),
                m.OrderItem.RolePattern(v => v.DerivedIrpfRegime, v => v.OrderWhereValidOrderItem.ObjectType, m.SalesOrder),
                m.OrderItem.RolePattern(v => v.DerivedVatRegime, v => v.OrderWhereValidOrderItem.ObjectType, m.SalesOrder),
                m.OrderItem.RolePattern(v => v.SalesTerms, v => v.OrderWhereValidOrderItem.ObjectType, m.SalesOrder),
                m.SalesTerm.RolePattern(v => v.TermType, v => v.OrderItemWhereSalesTerm.ObjectType.OrderWhereValidOrderItem.ObjectType, m.SalesOrder),

                m.SalesOrder.RolePattern(v => v.SalesOrderState),
                m.SalesOrder.RolePattern(v => v.TakenBy),
                m.SalesOrder.RolePattern(v => v.TakenByContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.SalesOrdersWhereTakenByContactPerson.ObjectType),
                m.SalesOrder.RolePattern(v => v.BillToCustomer),
                m.Party.RolePattern(v => v.DisplayName, v => v.SalesOrdersWhereBillToCustomer.ObjectType),
                m.SalesOrder.RolePattern(v => v.DerivedBillToContactMechanism),
                m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.SalesOrdersWhereDerivedBillToContactMechanism.ObjectType),
                m.SalesOrder.RolePattern(v => v.BillToContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.SalesOrdersWhereBillToContactPerson.ObjectType),
                m.SalesOrder.RolePattern(v => v.BillToEndCustomer),
                m.Party.RolePattern(v => v.DisplayName, v => v.SalesOrdersWhereBillToEndCustomer.ObjectType),
                m.SalesOrder.RolePattern(v => v.DerivedBillToEndCustomerContactMechanism),
                m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.SalesOrdersWhereDerivedBillToEndCustomerContactMechanism.ObjectType),
                m.SalesOrder.RolePattern(v => v.BillToEndCustomerContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.SalesOrdersWhereBillToEndCustomerContactPerson.ObjectType),
                m.SalesOrder.RolePattern(v => v.DerivedShipFromAddress),
                m.PostalAddress.RolePattern(v => v.DisplayName, v => v.SalesOrdersWhereDerivedShipFromAddress.ObjectType),
                m.SalesOrder.RolePattern(v => v.ShipToCustomer),
                m.Party.RolePattern(v => v.DisplayName, v => v.SalesOrdersWhereShipToCustomer.ObjectType),
                m.SalesOrder.RolePattern(v => v.DerivedShipToAddress),
                m.PostalAddress.RolePattern(v => v.DisplayName, v => v.SalesOrdersWhereDerivedShipToAddress.ObjectType),
                m.SalesOrder.RolePattern(v => v.ShipToContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.SalesOrdersWhereShipToContactPerson.ObjectType),
                m.SalesOrder.RolePattern(v => v.ShipToEndCustomer),
                m.Party.RolePattern(v => v.DisplayName, v => v.SalesOrdersWhereShipToEndCustomer.ObjectType),
                m.SalesOrder.RolePattern(v => v.DerivedShipToEndCustomerAddress),
                m.PostalAddress.RolePattern(v => v.DisplayName, v => v.SalesOrdersWhereDerivedShipToEndCustomerAddress.ObjectType),
                m.SalesOrder.RolePattern(v => v.ShipToEndCustomerContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.SalesOrdersWhereShipToEndCustomerContactPerson.ObjectType),
                m.SalesOrder.RolePattern(v => v.PlacingCustomer),
                m.Party.RolePattern(v => v.DisplayName, v => v.SalesOrdersWherePlacingCustomer.ObjectType),
                m.SalesOrder.RolePattern(v => v.DerivedPlacingCustomerContactMechanism),
                m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.SalesOrdersWhereDerivedPlacingCustomerContactMechanism.ObjectType),
                m.SalesOrder.RolePattern(v => v.PlacingCustomerContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.SalesOrdersWherePlacingCustomerContactPerson.ObjectType),
                m.SalesOrder.RolePattern(v => v.DerivedShipmentMethod),
                m.SalesOrder.RolePattern(v => v.DerivedPaymentMethod),
                m.SalesOrder.RolePattern(v => v.DerivedVatClause),
                m.SalesOrder.RolePattern(v => v.SalesChannel),
                m.SalesOrder.RolePattern(v => v.Store),
                m.SalesOrder.RolePattern(v => v.Quote),

                m.SalesOrderItem.RolePattern(v => v.DerivedShipToAddress, v => v.SalesOrderWhereSalesOrderItem.ObjectType),
                m.PostalAddress.RolePattern(v => v.DisplayName, v => v.SalesOrderItemsWhereDerivedShipFromAddress.ObjectType.SalesOrderWhereSalesOrderItem.ObjectType),
                m.SalesOrderItem.RolePattern(v => v.DerivedShipToParty, v => v.SalesOrderWhereSalesOrderItem.ObjectType),
                m.Party.RolePattern(v => v.DisplayName, v => v.SalesOrderItemsWhereDerivedShipToParty.ObjectType.SalesOrderWhereSalesOrderItem.ObjectType),
                m.SalesOrderItem.RolePattern(v => v.Product, v => v.SalesOrderWhereSalesOrderItem.ObjectType),
                m.SalesOrderItem.RolePattern(v => v.SerialisedItem, v => v.SalesOrderWhereSalesOrderItem.ObjectType),
                m.SalesOrderItem.RolePattern(v => v.OrderedWithFeatures, v => v.SalesOrderWhereSalesOrderItem.ObjectType),
                m.SalesOrderItem.RolePattern(v => v.InvoiceItemType, v => v.SalesOrderWhereSalesOrderItem.ObjectType),
                m.SalesOrderItem.RolePattern(v => v.SalesOrderItemState, v => v.SalesOrderWhereSalesOrderItem.ObjectType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesOrder>())
            {
                @this.DeriveSalesOrderSearchString(validation);
            }
        }
    }

    public static class SalesOrderSearchStringRuleExtensions
    {
        public static void DeriveSalesOrderSearchString(this SalesOrder @this, IValidation validation)
        {
            var array = new string[] {
                    @this.InternalComment,
                    @this.Description,
                    @this.CustomerReference,
                    @this.OrderNumber,
                    @this.Message,
                    @this.OrderKind?.Description,
                    @this.DerivedCurrency?.Abbreviation,
                    @this.DerivedCurrency?.Name,
                    @this.DerivedVatRegime?.Name,
                    @this.DerivedIrpfRegime?.Name,
                    @this.ExistSalesTerms ? string.Join(" ", @this.SalesTerms?.Select(v => v.Description ?? string.Empty).ToArray()) : null,
                    @this.ExistSalesTerms ? string.Join(" ", @this.SalesTerms?.Select(v => v.TermType?.Name ?? string.Empty).ToArray()) : null,
                    @this.SalesOrderState?.Name,
                    @this.TakenBy?.DisplayName,
                    @this.TakenByContactPerson?.DisplayName,
                    @this.BillToCustomer?.DisplayName,
                    @this.DerivedBillToContactMechanism?.DisplayName,
                    @this.BillToContactPerson?.DisplayName,
                    @this.BillToEndCustomer?.DisplayName,
                    @this.DerivedBillToEndCustomerContactMechanism?.DisplayName,
                    @this.BillToEndCustomerContactPerson?.DisplayName,
                    @this.DerivedShipFromAddress?.DisplayName,
                    @this.ShipToCustomer?.DisplayName,
                    @this.DerivedShipToAddress?.DisplayName,
                    @this.ShipToContactPerson?.DisplayName,
                    @this.ShipToEndCustomer?.DisplayName,
                    @this.DerivedShipToEndCustomerAddress?.DisplayName,
                    @this.ShipToEndCustomerContactPerson?.DisplayName,
                    @this.PlacingCustomer?.DisplayName,
                    @this.DerivedPlacingCustomerContactMechanism?.DisplayName,
                    @this.PlacingCustomerContactPerson?.DisplayName,
                    @this.DerivedShipmentMethod?.Name,
                    @this.DerivedPaymentMethod?.Description,
                    @this.DerivedVatClause?.Name,
                    @this.SalesChannel?.Name,
                    @this.Store?.Name,
                    @this.Quote?.QuoteNumber,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.ValidOrderItems?.Select(v => v.InternalComment ?? string.Empty).ToArray()) : null,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.ValidOrderItems?.Select(v => v.Message ?? string.Empty).ToArray()) : null,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.ValidOrderItems?.Select(v => v.Description ?? string.Empty).ToArray()) : null,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.ValidOrderItems?.Select(v => v.CorrespondingPurchaseOrder?.OrderNumber ?? string.Empty).ToArray()) : null,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.ValidOrderItems?.Select(v => v.QuoteItem?.QuoteWhereQuoteItem?.QuoteNumber ?? string.Empty).ToArray()) : null,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.ValidOrderItems?.Select(v => v.DerivedIrpfRegime?.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.ValidOrderItems?.Select(v => v.DerivedVatRegime?.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.ValidOrderItems?.SelectMany(v => v.SalesTerms?.Select(v => v.Description ?? string.Empty)).ToArray()) : null,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.ValidOrderItems?.SelectMany(v => v.SalesTerms?.Select(v => v.TermType?.Name ?? string.Empty)).ToArray()) : null,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.SalesOrderItems?.Select(v => v.DerivedShipToAddress?.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.SalesOrderItems?.Select(v => v.DerivedShipToParty?.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.SalesOrderItems?.Select(v => v.Product?.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.SalesOrderItems?.Select(v => v.SerialisedItem?.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.SalesOrderItems?.SelectMany(v => v.OrderedWithFeatures?.Select(v => v.Description ?? string.Empty)).ToArray()) : null,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.SalesOrderItems?.Select(v => v.InvoiceItemType?.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.SalesOrderItems?.Select(v => v.SalesOrderItemState?.Name ?? string.Empty).ToArray()) : null,
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
