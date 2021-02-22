// <copyright file="Domain.cs" company="Allors bvba">
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

    public class SalesOrderItemProvisionalDerivation : DomainDerivation
    {
        public SalesOrderItemProvisionalDerivation(M m) : base(m, new Guid("2d5fad32-da2f-436a-a4fa-04b3a6f1b894")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.SalesOrderItem.SalesOrderItemState),
                new ChangedPattern(m.SalesOrderItem.AssignedShipFromAddress),
                new ChangedPattern(m.SalesOrderItem.AssignedShipToAddress),
                new ChangedPattern(m.SalesOrderItem.AssignedShipToParty),
                new ChangedPattern(m.SalesOrderItem.AssignedDeliveryDate),
                new ChangedPattern(m.SalesOrderItem.AssignedVatRegime),
                new ChangedPattern(m.SalesOrderItem.AssignedIrpfRegime),
                new ChangedPattern(this.M.SalesOrder.SalesOrderItems) { Steps =  new IPropertyType[] {m.SalesOrder.SalesOrderItems} },
                new ChangedPattern(this.M.SalesOrder.DerivedShipFromAddress) { Steps =  new IPropertyType[] {m.SalesOrder.SalesOrderItems} },
                new ChangedPattern(this.M.SalesOrder.DerivedShipToAddress) { Steps =  new IPropertyType[] {m.SalesOrder.SalesOrderItems} },
                new ChangedPattern(this.M.SalesOrder.ShipToCustomer) { Steps =  new IPropertyType[] {m.SalesOrder.SalesOrderItems} },
                new ChangedPattern(this.M.SalesOrder.DeliveryDate) { Steps =  new IPropertyType[] {m.SalesOrder.SalesOrderItems} },
                new ChangedPattern(this.M.SalesOrder.DerivedVatRegime) { Steps =  new IPropertyType[] {m.SalesOrder.SalesOrderItems} },
                new ChangedPattern(this.M.SalesOrder.DerivedIrpfRegime) { Steps =  new IPropertyType[] {m.SalesOrder.SalesOrderItems} },
                new ChangedPattern(this.M.Organisation.ShippingAddress) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrderItemsWhereAssignedShipToParty  }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrderItem>().Where(v => v.SalesOrderItemState.IsProvisional))
            {
                var salesOrder = @this.SalesOrderWhereSalesOrderItem;

                @this.DerivedShipFromAddress = @this.AssignedShipFromAddress ?? salesOrder?.DerivedShipFromAddress;
                @this.DerivedShipToAddress = @this.AssignedShipToAddress ?? @this.AssignedShipToParty?.ShippingAddress ?? salesOrder?.DerivedShipToAddress;
                @this.DerivedShipToParty = @this.AssignedShipToParty ?? salesOrder?.ShipToCustomer;
                @this.DerivedDeliveryDate = @this.AssignedDeliveryDate ?? salesOrder?.DeliveryDate;
                @this.DerivedVatRegime = @this.AssignedVatRegime ?? salesOrder?.DerivedVatRegime;
                @this.VatRate = @this.DerivedVatRegime?.VatRate;
                @this.DerivedIrpfRegime = @this.AssignedIrpfRegime ?? salesOrder?.DerivedIrpfRegime;
                @this.IrpfRate = @this.DerivedIrpfRegime?.IrpfRate;
            }
        }
    }
}
