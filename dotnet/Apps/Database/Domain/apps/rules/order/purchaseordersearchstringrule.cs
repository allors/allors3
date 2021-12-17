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

    public class PurchaseOrderSearchStringRule : Rule
    {
        public PurchaseOrderSearchStringRule(MetaPopulation m) : base(m, new Guid("b77504b0-53f4-406b-8179-75b8ebcd82ad")) =>
            this.Patterns = new Pattern[]
            {
                m.Order.RolePattern(v => v.InternalComment, m.PurchaseOrder),
                m.Order.RolePattern(v => v.DerivedCurrency, m.PurchaseOrder),
                m.Order.RolePattern(v => v.Description, m.PurchaseOrder),
                m.Order.RolePattern(v => v.CustomerReference, m.PurchaseOrder),
                m.Order.RolePattern(v => v.OrderNumber, m.PurchaseOrder),
                m.Order.RolePattern(v => v.Message, m.PurchaseOrder),
                m.Order.RolePattern(v => v.OrderKind, m.PurchaseOrder),
                m.Order.RolePattern(v => v.SalesTerms, m.PurchaseOrder),
                m.SalesTerm.RolePattern(v => v.TermType, v => v.OrderWhereSalesTerm.Order, m.PurchaseOrder),
                m.Order.RolePattern(v => v.DerivedVatRegime, m.PurchaseOrder),
                m.Order.RolePattern(v => v.DerivedIrpfRegime, m.PurchaseOrder),
                m.Order.RolePattern(v => v.ValidOrderItems, m.PurchaseOrder),
                m.OrderItem.RolePattern(v => v.InternalComment, v => v.OrderWhereValidOrderItem.Order, m.PurchaseOrder),
                m.OrderItem.RolePattern(v => v.Message, v => v.OrderWhereValidOrderItem.Order, m.PurchaseOrder),
                m.OrderItem.RolePattern(v => v.Description, v => v.OrderWhereValidOrderItem.Order, m.PurchaseOrder),
                m.OrderItem.RolePattern(v => v.CorrespondingPurchaseOrder, v => v.OrderWhereValidOrderItem.Order, m.PurchaseOrder),
                m.OrderItem.RolePattern(v => v.QuoteItem, v => v.OrderWhereValidOrderItem.Order, m.PurchaseOrder),
                m.OrderItem.RolePattern(v => v.DerivedIrpfRegime, v => v.OrderWhereValidOrderItem.Order, m.PurchaseOrder),
                m.OrderItem.RolePattern(v => v.DerivedVatRegime, v => v.OrderWhereValidOrderItem.Order, m.PurchaseOrder),
                m.OrderItem.RolePattern(v => v.SalesTerms, v => v.OrderWhereValidOrderItem.Order, m.PurchaseOrder),
                m.SalesTerm.RolePattern(v => v.TermType, v => v.OrderItemWhereSalesTerm.OrderItem.OrderWhereValidOrderItem.Order, m.PurchaseOrder),

                m.PurchaseOrder.RolePattern(v => v.PurchaseOrderState),
                m.PurchaseOrder.RolePattern(v => v.OrderedBy),
                m.PurchaseOrder.RolePattern(v => v.TakenViaSupplier),
                m.Organisation.RolePattern(v => v.DisplayName, v => v.PurchaseOrdersWhereTakenViaSupplier.PurchaseOrder),
                m.PurchaseOrder.RolePattern(v => v.TakenViaSubcontractor),
                m.Organisation.RolePattern(v => v.DisplayName, v => v.PurchaseOrdersWhereTakenViaSubcontractor.PurchaseOrder),
                m.PurchaseOrder.RolePattern(v => v.DerivedTakenViaContactMechanism),
                m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.PurchaseOrdersWhereDerivedTakenViaContactMechanism.PurchaseOrder),
                m.PurchaseOrder.RolePattern(v => v.TakenViaContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.PurchaseOrdersWhereTakenViaContactPerson.PurchaseOrder),
                m.PurchaseOrder.RolePattern(v => v.DerivedBillToContactMechanism),
                m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.PurchaseOrdersWhereDerivedBillToContactMechanism.PurchaseOrder),
                m.PurchaseOrder.RolePattern(v => v.BillToContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.PurchaseOrdersWhereBillToContactPerson.PurchaseOrder),
                m.PurchaseOrder.RolePattern(v => v.DerivedShipToAddress),
                m.PostalAddress.RolePattern(v => v.DisplayName, v => v.PurchaseOrdersWhereDerivedShipToAddress.PurchaseOrder),
                m.PurchaseOrder.RolePattern(v => v.ShipToContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.PurchaseOrdersWhereShipToContactPerson.PurchaseOrder),

                m.PurchaseOrderItem.RolePattern(v => v.Part, v => v.PurchaseOrderWherePurchaseOrderItem.PurchaseOrder),
                m.PurchaseOrderItem.RolePattern(v => v.SerialisedItem, v => v.PurchaseOrderWherePurchaseOrderItem.PurchaseOrder),
                m.PurchaseOrderItem.RolePattern(v => v.SerialNumber, v => v.PurchaseOrderWherePurchaseOrderItem.PurchaseOrder),
                m.PurchaseOrderItem.RolePattern(v => v.InvoiceItemType, v => v.PurchaseOrderWherePurchaseOrderItem.PurchaseOrder),
                m.PurchaseOrderItem.RolePattern(v => v.PurchaseOrderItemState, v => v.PurchaseOrderWherePurchaseOrderItem.PurchaseOrder),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseOrder>())
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
                    @this.ExistSalesTerms ? string.Join(" ", @this.SalesTerms?.Select(v => v.Description)) : null,
                    @this.ExistSalesTerms ? string.Join(" ", @this.SalesTerms?.Select(v => v.TermType?.Name)) : null,
                    @this.PurchaseOrderState?.Name,
                    @this.OrderedBy?.DisplayName,
                    @this.TakenViaSupplier?.DisplayName,
                    @this.TakenViaSubcontractor?.DisplayName,
                    @this.DerivedTakenViaContactMechanism?.DisplayName,
                    @this.TakenViaContactPerson?.DisplayName,
                    @this.DerivedBillToContactMechanism?.DisplayName,
                    @this.BillToContactPerson?.DisplayName,
                    @this.DerivedShipToAddress?.DisplayName,
                    @this.ShipToContactPerson?.DisplayName,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.ValidOrderItems?.Select(v => v.InternalComment)) : null,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.ValidOrderItems?.Select(v => v.Message)) : null,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.ValidOrderItems?.Select(v => v.Description)) : null,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.ValidOrderItems?.Select(v => v.CorrespondingPurchaseOrder?.OrderNumber)) : null,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.ValidOrderItems?.Select(v => v.QuoteItem?.QuoteWhereQuoteItem?.QuoteNumber)) : null,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.ValidOrderItems?.Select(v => v.DerivedIrpfRegime?.Name)) : null,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.ValidOrderItems?.Select(v => v.DerivedVatRegime?.Name)) : null,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.ValidOrderItems?.Select(v => v.SalesTerms?.Select(v => v.Description))) : null,
                    @this.ExistValidOrderItems ? string.Join(" ", @this.ValidOrderItems?.Select(v => v.SalesTerms?.Select(v => v.TermType?.Name))) : null,
                    @this.ExistPurchaseOrderItems ? string.Join(" ", @this.PurchaseOrderItems?.Select(v => v.Part?.DisplayName)) : null,
                    @this.ExistPurchaseOrderItems ? string.Join(" ", @this.PurchaseOrderItems?.Select(v => v.SerialisedItem?.DisplayName)) : null,
                    @this.ExistPurchaseOrderItems ? string.Join(" ", @this.PurchaseOrderItems?.Select(v => v.SerialNumber)) : null,
                    @this.ExistPurchaseOrderItems ? string.Join(" ", @this.PurchaseOrderItems?.Select(v => v.InvoiceItemType?.Name)) : null,
                    @this.ExistPurchaseOrderItems ? string.Join(" ", @this.PurchaseOrderItems?.Select(v => v.PurchaseOrderItemState?.Name)) : null,
                };

                if (array.Any(s => !string.IsNullOrEmpty(s)))
                {
                    @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
                }
            }
        }
    }
}
