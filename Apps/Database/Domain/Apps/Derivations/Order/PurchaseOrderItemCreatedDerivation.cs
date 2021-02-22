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

    public class PurchaseOrderItemCreatedDerivation : DomainDerivation
    {
        public PurchaseOrderItemCreatedDerivation(M m) : base(m, new Guid("7559bffd-7685-4023-bef7-9f5ff96b6f41")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.PurchaseOrderItem.PurchaseOrderItemState),
                new ChangedPattern(m.PurchaseOrderItem.AssignedDeliveryDate),
                new ChangedPattern(m.PurchaseOrderItem.AssignedVatRegime),
                new ChangedPattern(m.PurchaseOrderItem.AssignedIrpfRegime),
                new ChangedPattern(this.M.PurchaseOrder.PurchaseOrderItems) { Steps =  new IPropertyType[] {m.PurchaseOrder.PurchaseOrderItems } },
                new ChangedPattern(this.M.PurchaseOrder.DeliveryDate) { Steps =  new IPropertyType[] {m.PurchaseOrder.PurchaseOrderItems } },
                new ChangedPattern(this.M.PurchaseOrder.DerivedVatRegime) { Steps =  new IPropertyType[] {m.PurchaseOrder.PurchaseOrderItems } },
                new ChangedPattern(this.M.PurchaseOrder.DerivedIrpfRegime) { Steps =  new IPropertyType[] {m.PurchaseOrder.PurchaseOrderItems } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<PurchaseOrderItem>().Where(v => v.PurchaseOrderItemState.IsCreated))
            {
                var order = @this.PurchaseOrderWherePurchaseOrderItem;

                @this.DerivedDeliveryDate = @this.AssignedDeliveryDate ?? order?.DeliveryDate;
                @this.DerivedVatRegime = @this.AssignedVatRegime ?? order?.DerivedVatRegime;
                @this.VatRate = @this.DerivedVatRegime?.VatRate;
                @this.DerivedIrpfRegime = @this.AssignedIrpfRegime ?? order?.DerivedIrpfRegime;
                @this.IrpfRate = @this.DerivedIrpfRegime?.IrpfRate;
            }
        }
    }
}
