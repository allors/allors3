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

    public class PurchaseOrderItemBillingsWhereOrderItemRule : Rule
    {
        public PurchaseOrderItemBillingsWhereOrderItemRule(MetaPopulation m) : base(m, new Guid("2dd5538a-1b0b-4ffb-8049-78ee3032b38a")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrderItem.RolePattern(v => v.PurchaseOrderItemState),
                m.OrderItem.AssociationPattern(v => v.OrderItemBillingsWhereOrderItem, m.PurchaseOrderItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<PurchaseOrderItem>())
            {
                @this.DerivePurchaseOrderItemBillingsWhereOrderItem(validation);
            }
        }
    }

    public static class PurchaseOrderItemBillingsWhereOrderItemRuleExtensions
    {
        public static void DerivePurchaseOrderItemBillingsWhereOrderItem(this PurchaseOrderItem @this, IValidation validation)
        {
            if (@this.IsValid && !@this.ExistOrderItemBillingsWhereOrderItem)
            {
                @this.CanInvoice = true;
            }
            else
            {
                @this.CanInvoice = false;
            }
        }
    }
}
