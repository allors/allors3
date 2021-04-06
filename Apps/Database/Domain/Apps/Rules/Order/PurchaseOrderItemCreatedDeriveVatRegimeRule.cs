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

    public class PurchaseOrderItemCreatedDeriveVatRegimeRule : Rule
    {
        public PurchaseOrderItemCreatedDeriveVatRegimeRule(MetaPopulation m) : base(m, new Guid("4eb0b3b1-479d-46c0-9e4d-5761e07bceb8")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PurchaseOrderItem, m.PurchaseOrderItem.PurchaseOrderItemState),
                new RolePattern(m.PurchaseOrderItem, m.PurchaseOrderItem.AssignedVatRegime),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.DerivedVatRegime) { Steps =  new IPropertyType[] {m.PurchaseOrder.PurchaseOrderItems } },
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.OrderDate) { Steps =  new IPropertyType[] {m.PurchaseOrder.PurchaseOrderItems } },
                new AssociationPattern(m.PurchaseOrder.PurchaseOrderItems),
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
