// <copyright file="PurchaseOrderItemCreatedDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class PurchaseOrderItemCreatedVatRegimeRule : Rule
    {
        public PurchaseOrderItemCreatedVatRegimeRule(MetaPopulation m) : base(m, new Guid("4eb0b3b1-479d-46c0-9e4d-5761e07bceb8")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrderItem.RolePattern(v => v.PurchaseOrderItemState),
                m.PurchaseOrderItem.RolePattern(v => v.AssignedVatRegime),
                m.PurchaseOrder.RolePattern(v => v.DerivedVatRegime, v => v.PurchaseOrderItems),
                m.PurchaseOrder.RolePattern(v => v.OrderDate, v => v.PurchaseOrderItems),
                m.PurchaseOrderItem.AssociationPattern(v => v.PurchaseOrderWherePurchaseOrderItem),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<PurchaseOrderItem>().Where(v => v.PurchaseOrderItemState.IsCreated))
            {
                var order = @this.PurchaseOrderWherePurchaseOrderItem;

                @this.DerivedVatRegime = @this.AssignedVatRegime ?? order?.DerivedVatRegime;
                @this.VatRate = @this.DerivedVatRegime?.VatRates.First(v => v.FromDate <= order.OrderDate && (!v.ExistThroughDate || v.ThroughDate >= order.OrderDate));
            }
        }
    }
}
