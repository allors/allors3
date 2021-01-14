// <copyright file="PurchaseOrderItemPriceDerivation.cs" company="Allors bvba">
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

    public class PurchaseOrderItemPriceDerivation : DomainDerivation
    {
        public PurchaseOrderItemPriceDerivation(M m) : base(m, new Guid("28cabee9-ccef-44e5-a702-eed61b6b019e")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.PurchaseOrderItem.PurchaseOrderItemState),
                new ChangedPattern(m.PurchaseOrderItem.IsReceivable),
                new ChangedPattern(m.PurchaseOrderItem.QuantityOrdered),
                new ChangedPattern(m.PurchaseOrderItem.AssignedUnitPrice),
                new ChangedPattern(m.PurchaseOrderItem.AssignedVatRegime),
                new ChangedPattern(m.PurchaseOrderItem.AssignedIrpfRegime),
                new ChangedPattern(m.PurchaseOrder.PurchaseOrderItems) { Steps = new IPropertyType[] {m.PurchaseOrder.PurchaseOrderItems} },
                new ChangedPattern(m.PurchaseOrder.DerivedVatRegime) { Steps = new IPropertyType[] {m.PurchaseOrder.PurchaseOrderItems} },
                new ChangedPattern(m.PurchaseOrder.DerivedIrpfRegime ) { Steps = new IPropertyType[] {m.PurchaseOrder.PurchaseOrderItems} },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<PurchaseOrderItem>())
            {
                var purchaseOrder = @this.PurchaseOrderWherePurchaseOrderItem;

                if (@this.IsValid)
                {
                    @this.UnitBasePrice = 0;
                    @this.UnitDiscount = 0;
                    @this.UnitSurcharge = 0;

                    if (@this.AssignedUnitPrice.HasValue)
                    {
                        @this.UnitBasePrice = @this.AssignedUnitPrice.Value;
                        @this.UnitPrice = @this.AssignedUnitPrice.Value;
                    }
                    else
                    {
                        var order = @this.PurchaseOrderWherePurchaseOrderItem;
                        if (order != null)
                        {
                            @this.UnitBasePrice = new SupplierOfferings(@this.Strategy.Session).PurchasePrice(order.TakenViaSupplier, order.OrderDate, @this.Part);
                        }
                    }

                    @this.TotalBasePrice = @this.UnitBasePrice * @this.QuantityOrdered;
                    @this.TotalDiscount = @this.UnitDiscount * @this.QuantityOrdered;
                    @this.TotalSurcharge = @this.UnitSurcharge * @this.QuantityOrdered;
                    @this.UnitPrice = @this.UnitBasePrice - @this.UnitDiscount + @this.UnitSurcharge;

                    @this.UnitVat = @this.ExistVatRate ? @this.UnitPrice * @this.VatRate.Rate / 100 : 0;
                    @this.UnitIrpf = @this.ExistIrpfRate ? @this.UnitPrice * @this.IrpfRate.Rate / 100 : 0;
                    @this.TotalVat = @this.UnitVat * @this.QuantityOrdered;
                    @this.TotalExVat = @this.UnitPrice * @this.QuantityOrdered;
                    @this.TotalIrpf = @this.UnitIrpf * @this.QuantityOrdered;
                    @this.TotalIncVat = @this.TotalExVat + @this.TotalVat;
                    @this.GrandTotal = @this.TotalIncVat - @this.TotalIrpf;
                }
            }
        }
    }
}
