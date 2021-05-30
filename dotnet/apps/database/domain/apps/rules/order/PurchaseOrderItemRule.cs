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
    using Derivations.Rules;
    using Resources;

    public class PurchaseOrderItemRule : Rule
    {
        public PurchaseOrderItemRule(MetaPopulation m) : base(m, new Guid("A59A2EFC-AF5C-4F95-9212-4FD4B0306957")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrderItem.RolePattern(v => v.Part),
                m.PurchaseOrderItem.RolePattern(v => v.SerialisedItem),
                m.PurchaseOrderItem.RolePattern(v => v.SerialNumber),
                m.PurchaseOrderItem.RolePattern(v => v.QuantityOrdered),
                m.PurchaseOrderItem.RolePattern(v => v.DerivationTrigger),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<PurchaseOrderItem>())
            {
                if (@this.ExistInvoiceItemType
                    && (@this.InvoiceItemType.IsPartItem || @this.InvoiceItemType.IsProductItem))
                {
                    validation.AssertExists(@this, @this.Meta.Part);
                    validation.AssertExists(@this, @this.Meta.StoredInFacility);
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

                var purchaseOrder = @this.PurchaseOrderWherePurchaseOrderItem;

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
