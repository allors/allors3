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

    public class PurchaseOrderItemCreatedIrpfRateRule : Rule
    {
        public PurchaseOrderItemCreatedIrpfRateRule(MetaPopulation m) : base(m, new Guid("7559bffd-7685-4023-bef7-9f5ff96b6f41")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrderItem.RolePattern(v => v.PurchaseOrderItemState),
                m.PurchaseOrderItem.RolePattern(v => v.AssignedIrpfRegime),
                m.PurchaseOrder.RolePattern(v => v.DerivedIrpfRegime, v => v.PurchaseOrderItems),
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

                @this.DerivedIrpfRegime = @this.AssignedIrpfRegime ?? order?.DerivedIrpfRegime;
                @this.IrpfRate = @this.DerivedIrpfRegime?.IrpfRates.First(v => v.FromDate <= order.OrderDate && (!v.ExistThroughDate || v.ThroughDate >= order.OrderDate));
            }
        }
    }
}
