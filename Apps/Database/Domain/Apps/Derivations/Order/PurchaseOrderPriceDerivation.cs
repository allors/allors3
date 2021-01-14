// <copyright file="PurchaseOrderPriceDerivation.cs" company="Allors bvba">
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

    public class PurchaseOrderPriceDerivation : DomainDerivation
    {
        public PurchaseOrderPriceDerivation(M m) : base(m, new Guid("b553564c-45e1-495c-975a-90eb6fc67d5d")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.PurchaseOrder.DerivationTrigger),
                new ChangedPattern(this.M.PurchaseOrder.ValidOrderItems),
                new ChangedPattern(this.M.PurchaseOrder.TakenViaSupplier),
                new ChangedPattern(this.M.PurchaseOrder.OrderAdjustments),
                new ChangedPattern(this.M.PurchaseOrderItem.Part) { Steps =  new IPropertyType[] {m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem } },
                new ChangedPattern(this.M.PurchaseOrderItem.QuantityOrdered) { Steps =  new IPropertyType[] {m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem } },
                new ChangedPattern(this.M.PurchaseOrderItem.AssignedUnitPrice) { Steps =  new IPropertyType[] {m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem } },
                new ChangedPattern(this.M.PurchaseOrderItem.DiscountAdjustments) { Steps =  new IPropertyType[] {m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem } },
                new ChangedPattern(this.M.PurchaseOrderItem.SurchargeAdjustments) { Steps =  new IPropertyType[] {m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseOrder>())
            {
                @this.TotalBasePrice = 0;
                @this.TotalDiscount = 0;
                @this.TotalSurcharge = 0;
                @this.TotalVat = 0;
                @this.TotalIrpf = 0;
                @this.TotalExVat = 0;
                @this.TotalExtraCharge = 0;
                @this.TotalIncVat = 0;
                @this.GrandTotal = 0;

                foreach (PurchaseOrderItem orderItem in @this.ValidOrderItems)
                {
                    @this.TotalBasePrice += orderItem.TotalBasePrice;
                    @this.TotalDiscount += orderItem.TotalDiscount;
                    @this.TotalSurcharge += orderItem.TotalSurcharge;
                    @this.TotalVat += orderItem.TotalVat;
                    @this.TotalIrpf += orderItem.TotalIrpf;
                    @this.TotalExVat += orderItem.TotalExVat;
                    @this.TotalIncVat += orderItem.TotalIncVat;
                    @this.GrandTotal += orderItem.GrandTotal;
                }
            }
        }
    }
}
