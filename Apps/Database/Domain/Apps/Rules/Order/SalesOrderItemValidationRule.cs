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

    public class SalesOrderItemValidationRule : Rule
    {
        public SalesOrderItemValidationRule(MetaPopulation m) : base(m, new Guid("aacada18-bce7-44e3-92fa-50d0a9b0790b")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.SalesOrderItem, m.SalesOrderItem.Product),
                new RolePattern(m.SalesOrderItem, m.SalesOrderItem.ProductFeature),
                new RolePattern(m.SalesOrderItem, m.SalesOrderItem.InvoiceItemType),
                new RolePattern(m.SalesOrderItem, m.SalesOrderItem.SerialisedItem),
                new RolePattern(m.SalesOrderItem, m.SalesOrderItem.NextSerialisedItemAvailability),
                new RolePattern(m.SalesOrderItem, m.SalesOrderItem.QuantityOrdered),
                new RolePattern(m.SalesOrderItem, m.SalesOrderItem.AssignedUnitPrice),
                new RolePattern(m.SalesOrderItem, m.SalesOrderItem.SalesOrderItemState),
                new RolePattern(m.SalesOrderItem, m.SalesOrderItem.ReservedFromNonSerialisedInventoryItem),
                new RolePattern(m.SalesOrderItem, m.SalesOrderItem.ReservedFromSerialisedInventoryItem),
                new RolePattern(m.SalesOrderItem, m.SalesOrderItem.DiscountAdjustments),
                new RolePattern(m.SalesOrderItem, m.SalesOrderItem.SurchargeAdjustments),
                new RolePattern(m.SalesOrderItem, m.SalesOrderItem.SalesOrderItemInventoryAssignments),
                new RolePattern(m.SerialisedInventoryItem, m.SerialisedInventoryItem.Quantity) { Steps = new IPropertyType[] { m.SerialisedInventoryItem.SerialisedItem, m.SerialisedItem.SalesOrderItemsWhereSerialisedItem }},
                new AssociationPattern(m.InventoryItemTransaction.Part) { Steps = new IPropertyType[] { m.UnifiedGood.SalesOrderItemsWhereProduct }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrderItem>())
            {

                if (@this.ExistSerialisedItem && !@this.ExistNextSerialisedItemAvailability)
                {
                    validation.AssertExists(@this, @this.Meta.NextSerialisedItemAvailability);
                }

                if (@this.Part != null && @this.Part.InventoryItemKind.IsSerialised && @this.QuantityOrdered != 1)
                {
                    validation.AddError($"{@this} {this.M.SalesOrderItem.QuantityOrdered} {ErrorMessages.InvalidQuantity}");
                }

                if (@this.Part != null && @this.Part.InventoryItemKind.IsNonSerialised && @this.QuantityOrdered == 0)
                {
                    validation.AddError($"{@this} {this.M.SalesOrderItem.QuantityOrdered} {ErrorMessages.InvalidQuantity}");
                }

                if (@this.ExistInvoiceItemType && @this.InvoiceItemType.MaxQuantity.HasValue && @this.QuantityOrdered > @this.InvoiceItemType.MaxQuantity.Value)
                {
                    validation.AddError($"{@this} {this.M.SalesOrderItem.QuantityOrdered} {ErrorMessages.InvalidQuantity}");
                }

                if (@this.ExistSalesOrderItemWhereOrderedWithFeature)
                {
                    validation.AssertExists(@this, this.M.SalesOrderItem.ProductFeature);
                    validation.AssertNotExists(@this, this.M.SalesOrderItem.Product);
                }
                else
                {
                    validation.AssertNotExists(@this, this.M.SalesOrderItem.ProductFeature);
                }

                if (@this.ExistProduct && @this.ExistQuantityOrdered && @this.QuantityOrdered < @this.QuantityShipped)
                {
                    validation.AddError($"{@this} {this.M.SalesOrderItem.QuantityOrdered} {ErrorMessages.SalesOrderItemLessThanAlreadeyShipped}");
                }

                var isSubTotalItem = @this.ExistInvoiceItemType && (@this.InvoiceItemType.IsProductItem || @this.InvoiceItemType.IsPartItem);

                if (isSubTotalItem)
                {
                    if (@this.QuantityOrdered == 0)
                    {
                        validation.AddError($"{@this} {this.M.SalesOrderItem.QuantityOrdered} QuantityOrdered is Required");
                    }
                }
                else
                {
                    if (@this.AssignedUnitPrice == 0)
                    {
                        validation.AddError($"{@this} {this.M.SalesOrderItem.AssignedUnitPrice} Price is Required");
                    }
                }

                validation.AssertExistsAtMostOne(@this, this.M.SalesOrderItem.Product, this.M.SalesOrderItem.ProductFeature);
                validation.AssertExistsAtMostOne(@this, this.M.SalesOrderItem.SerialisedItem, this.M.SalesOrderItem.ProductFeature);
                validation.AssertExistsAtMostOne(@this, this.M.SalesOrderItem.ReservedFromSerialisedInventoryItem, this.M.SalesOrderItem.ReservedFromNonSerialisedInventoryItem);
                validation.AssertExistsAtMostOne(@this, this.M.SalesOrderItem.AssignedUnitPrice, this.M.SalesOrderItem.DiscountAdjustments, this.M.SalesOrderItem.SurchargeAdjustments);

                if (@this.SalesOrderItemState.IsInProcess
                     && @this.ExistPreviousReservedFromNonSerialisedInventoryItem
                    && !Equals(@this.ReservedFromNonSerialisedInventoryItem, @this.PreviousReservedFromNonSerialisedInventoryItem))
                {
                    validation.AddError($"{@this} {@this.Meta.ReservedFromNonSerialisedInventoryItem} {ErrorMessages.ReservedFromNonSerialisedInventoryItem}");
                }

                if (@this.ExistSerialisedItem && @this.QuantityOrdered != 1)
                {
                    validation.AddError($"{@this}, {@this.Meta.QuantityOrdered}, {ErrorMessages.SerializedItemQuantity}");
                }
            }
        }
    }
}
