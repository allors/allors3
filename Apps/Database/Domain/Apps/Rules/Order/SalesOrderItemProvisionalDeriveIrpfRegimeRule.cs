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

    public class SalesOrderItemProvisionalDeriveIrpfRegimeRule : Rule
    {
        public SalesOrderItemProvisionalDeriveIrpfRegimeRule(MetaPopulation m) : base(m, new Guid("2d5fad32-da2f-436a-a4fa-04b3a6f1b894")) =>
            this.Patterns = new Pattern[]
            {
                new AssociationPattern(m.SalesOrder.SalesOrderItems),
                new RolePattern(m.SalesOrderItem, m.SalesOrderItem.SalesOrderItemState),
                new RolePattern(m.SalesOrderItem, m.SalesOrderItem.AssignedIrpfRegime),
                new RolePattern(m.SalesOrder, m.SalesOrder.DerivedIrpfRegime) { Steps =  new IPropertyType[] {m.SalesOrder.SalesOrderItems} },
                new RolePattern(m.SalesOrder, m.SalesOrder.OrderDate) { Steps =  new IPropertyType[] {m.SalesOrder.SalesOrderItems} },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrderItem>().Where(v => v.SalesOrderItemState.IsProvisional))
            {
                var salesOrder = @this.SalesOrderWhereSalesOrderItem;

                @this.DerivedIrpfRegime = @this.AssignedIrpfRegime ?? salesOrder?.DerivedIrpfRegime;
                @this.IrpfRate = @this.DerivedIrpfRegime?.IrpfRates.First(v => v.FromDate <= salesOrder.OrderDate && (!v.ExistThroughDate || v.ThroughDate >= salesOrder.OrderDate));
            }
        }
    }
}
