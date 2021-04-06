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

    public class SalesOrderItemProvisionalDeriveDeliveryDateRule : Rule
    {
        public SalesOrderItemProvisionalDeriveDeliveryDateRule(MetaPopulation m) : base(m, new Guid("bb7a16f3-ae6c-49e3-adb4-a978e93f106d")) =>
            this.Patterns = new Pattern[]
            {
                new AssociationPattern(m.SalesOrder.SalesOrderItems),
                new RolePattern(m.SalesOrderItem, m.SalesOrderItem.SalesOrderItemState),
                new RolePattern(m.SalesOrderItem, m.SalesOrderItem.AssignedDeliveryDate),
                new RolePattern(m.SalesOrder, m.SalesOrder.DeliveryDate) { Steps =  new IPropertyType[] {m.SalesOrder.SalesOrderItems} },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrderItem>().Where(v => v.SalesOrderItemState.IsProvisional))
            {
                var salesOrder = @this.SalesOrderWhereSalesOrderItem;

                @this.DerivedDeliveryDate = @this.AssignedDeliveryDate ?? salesOrder?.DeliveryDate;
            }
        }
    }
}
