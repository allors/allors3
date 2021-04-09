// <copyright file="PurchaseOrderItemCreatedDerivation.cs" company="Allors bvba">
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

    public class PurchaseOrderItemCreatedDeliveryDateRule : Rule
    {
        public PurchaseOrderItemCreatedDeliveryDateRule(MetaPopulation m) : base(m, new Guid("f2b5b9d4-0496-4237-8722-ad4c5521c721")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrderItem.RolePattern(v => v.PurchaseOrderItemState),
                m.PurchaseOrderItem.RolePattern(v => v.AssignedDeliveryDate),
                m.PurchaseOrder.RolePattern(v => v.DeliveryDate, v => v.PurchaseOrderItems),
                m.PurchaseOrderItem.AssociationPattern(v => v.PurchaseOrderWherePurchaseOrderItem),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<PurchaseOrderItem>().Where(v => v.PurchaseOrderItemState.IsCreated))
            {
                var order = @this.PurchaseOrderWherePurchaseOrderItem;

                @this.DerivedDeliveryDate = @this.AssignedDeliveryDate ?? order?.DeliveryDate;
            }
        }
    }
}
