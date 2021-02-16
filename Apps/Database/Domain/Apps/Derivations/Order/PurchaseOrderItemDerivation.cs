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

    public class PurchaseOrderItemDerivation : DomainDerivation
    {
        public PurchaseOrderItemDerivation(M m) : base(m, new Guid("A59A2EFC-AF5C-4F95-9212-4FD4B0306957")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.PurchaseOrderItem.Part),
                new ChangedPattern(m.PurchaseOrderItem.SerialisedItem),
                new ChangedPattern(m.PurchaseOrderItem.SerialNumber),
                new ChangedPattern(m.PurchaseOrderItem.QuantityOrdered),
                new ChangedPattern(m.PurchaseOrderItem.DerivationTrigger),
                new ChangedPattern(m.PurchaseOrder.StoredInFacility) { Steps = new IPropertyType[] {m.PurchaseOrder.PurchaseOrderItems} },
                new ChangedPattern(m.OrderItemBilling.OrderItem) { Steps = new IPropertyType[] { m.OrderItemBilling.OrderItem}, OfType = m.PurchaseOrderItem.Class },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<PurchaseOrderItem>())
            {
                var purchaseOrder = @this.PurchaseOrderWherePurchaseOrderItem;

                if (!@this.ExistStoredInFacility
                    && @this.ExistPurchaseOrderWherePurchaseOrderItem
                    && @this.PurchaseOrderWherePurchaseOrderItem.ExistStoredInFacility)
                {
                    @this.StoredInFacility = @this.PurchaseOrderWherePurchaseOrderItem.StoredInFacility;
                }

                if (@this.ExistPart && @this.Part.InventoryItemKind.IsSerialised)
                {
                    validation.AssertAtLeastOne(@this, this.M.PurchaseOrderItem.SerialisedItem, this.M.PurchaseOrderItem.SerialNumber);
                    validation.AssertExistsAtMostOne(@this, this.M.PurchaseOrderItem.SerialisedItem, this.M.PurchaseOrderItem.SerialNumber);

                    if (@this.QuantityOrdered != 1)
                    {
                        validation.AddError($"{@this} {this.M.PurchaseOrderItem.QuantityOrdered} {ErrorMessages.InvalidQuantity}");
                    }
                }

                if (!@this.ExistPart && @this.QuantityOrdered != 1)
                {
                    validation.AddError($"{@this} {this.M.PurchaseOrderItem.QuantityOrdered} {ErrorMessages.InvalidQuantity}");
                }

                if (@this.ExistPart && @this.Part.InventoryItemKind.IsNonSerialised && @this.QuantityOrdered == 0)
                {
                    validation.AddError($"{@this} {this.M.PurchaseOrderItem.QuantityOrdered} {ErrorMessages.InvalidQuantity}");
                }

                if (@this.IsValid && !@this.ExistOrderItemBillingsWhereOrderItem)
                {
                    @this.CanInvoice = true;
                }
                else
                {
                    @this.CanInvoice = false;
                }

                if (purchaseOrder != null
                    && @this.ExistPart
                    && !purchaseOrder.PurchaseOrderItemsByProduct.Any(v => v.UnifiedProduct.Equals(@this.Part)))
                {
                    purchaseOrder.AddPurchaseOrderItemsByProduct(new PurchaseOrderItemByProductBuilder(transaction).WithUnifiedProduct(@this.Part).Build());
                }
            }
        }
    }
}
