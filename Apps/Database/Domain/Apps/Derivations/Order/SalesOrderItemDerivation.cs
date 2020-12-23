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

    public class SalesOrderItemDerivation : DomainDerivation
    {
        public SalesOrderItemDerivation(M m) : base(m, new Guid("FEF4E104-A0F0-4D83-A248-A1A606D93E41")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.SalesOrderItem.Product),
                new ChangedPattern(m.SalesOrderItem.ProductFeature),
                new ChangedPattern(m.SalesOrderItem.InvoiceItemType),
                new ChangedPattern(m.SalesOrderItem.SerialisedItem),
                new ChangedPattern(m.SalesOrderItem.NextSerialisedItemAvailability),
                new ChangedPattern(m.SalesOrderItem.QuantityOrdered),
                new ChangedPattern(m.SalesOrderItem.AssignedUnitPrice),
                new ChangedPattern(m.SalesOrderItem.SalesOrderItemState),
                new ChangedPattern(m.SalesOrderItem.SalesOrderItemShipmentState),
                new ChangedPattern(m.SalesOrderItem.ReservedFromNonSerialisedInventoryItem),
                new ChangedPattern(m.SalesOrderItem.ReservedFromSerialisedInventoryItem),
                new ChangedPattern(m.SalesOrderItem.DiscountAdjustments),
                new ChangedPattern(m.SalesOrderItem.SurchargeAdjustments),
                new ChangedPattern(m.SalesOrderItem.SalesOrderItemInventoryAssignments),
                new ChangedPattern(m.SerialisedInventoryItem.Quantity) { Steps = new IPropertyType[] {m.SerialisedInventoryItem.SerialisedItem, m.SerialisedItem.SalesOrderItemsWhereSerialisedItem }},
                new ChangedPattern(m.InventoryItemTransaction.Part) { Steps = new IPropertyType[] {m.InventoryItemTransaction.Part, m.UnifiedGood.SalesOrderItemsWhereProduct }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<SalesOrderItem>())
            {
                var salesOrder = @this.SalesOrderWhereSalesOrderItem;

                if (@this.ExistProduct && !@this.ExistInvoiceItemType)
                {
                    @this.InvoiceItemType = new InvoiceItemTypes(@this.Session()).ProductItem;
                }

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

                // TODO: Use versioning
                if (@this.ExistPreviousProduct && !@this.PreviousProduct.Equals(@this.Product))
                {
                    validation.AddError($"{@this} {this.M.SalesOrderItem.Product} {ErrorMessages.SalesOrderItemProductChangeNotAllowed}");
                }
                else
                {
                    @this.PreviousProduct = @this.Product;
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
                    validation.AddError($"{@this} {@this.Meta.QuantityOrdered} {ErrorMessages.SerializedItemQuantity}");
                }

                if (@this.IsValid && @this.Part != null && salesOrder?.TakenBy != null)
                {
                    if (@this.Part.InventoryItemKind.IsSerialised)
                    {
                        if (!@this.ExistReservedFromSerialisedInventoryItem)
                        {
                            if (@this.ExistSerialisedItem)
                            {
                                if (@this.SerialisedItem.ExistSerialisedInventoryItemsWhereSerialisedItem)
                                {
                                    @this.ReservedFromSerialisedInventoryItem = @this.SerialisedItem.SerialisedInventoryItemsWhereSerialisedItem.FirstOrDefault(v => v.Quantity == 1);
                                }
                            }
                            else
                            {
                                var inventoryItems = @this.Part.InventoryItemsWherePart;
                                inventoryItems.Filter.AddEquals(this.M.InventoryItem.Facility, salesOrder.OriginFacility);
                                @this.ReservedFromSerialisedInventoryItem = inventoryItems.FirstOrDefault() as SerialisedInventoryItem;
                            }
                        }
                    }
                    else
                    {
                        if (!@this.ExistReservedFromNonSerialisedInventoryItem)
                        {
                            var inventoryItems = @this.Part.InventoryItemsWherePart;
                            inventoryItems.Filter.AddEquals(this.M.InventoryItem.Facility, salesOrder.OriginFacility);
                            @this.ReservedFromNonSerialisedInventoryItem = inventoryItems.FirstOrDefault() as NonSerialisedInventoryItem;
                        }
                    }
                }

                @this.PreviousReservedFromNonSerialisedInventoryItem = @this.ReservedFromNonSerialisedInventoryItem;
                @this.PreviousQuantity = @this.QuantityOrdered;
                @this.PreviousProduct = @this.Product;
            }
        }
    }
}
