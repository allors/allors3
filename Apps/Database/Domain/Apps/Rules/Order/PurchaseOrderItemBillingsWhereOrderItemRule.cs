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

    public class PurchaseOrderItemBillingsWhereOrderItemRule : Rule
    {
        public PurchaseOrderItemBillingsWhereOrderItemRule(MetaPopulation m) : base(m, new Guid("2dd5538a-1b0b-4ffb-8049-78ee3032b38a")) =>
            this.Patterns = new Pattern[]
            {
                m.OrderItem.AssociationPattern(v => v.OrderItemBillingsWhereOrderItem, m.PurchaseOrderItem),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<PurchaseOrderItem>())
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
}
