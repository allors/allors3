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

    public class SalesOrderCanShipRule : Rule
    {
        public SalesOrderCanShipRule(M m) : base(m, new Guid("3f3129c8-2a62-4d2f-8652-cf2d503539a5")) =>
            this.Patterns = new Pattern[]
        {
            // Do not listen for changes in Store.BillingProcess.

            new RolePattern(m.SalesOrder, m.SalesOrder.SalesOrderState),
            new RolePattern(m.SalesOrder, m.SalesOrder.PartiallyShip),
            new RolePattern(m.SalesOrderItem, m.SalesOrderItem.SalesOrderItemState) { Steps =  new IPropertyType[] {m.SalesOrderItem.SalesOrderWhereSalesOrderItem } },
            new RolePattern(m.SalesOrderItem, m.SalesOrderItem.QuantityRequestsShipping) { Steps =  new IPropertyType[] {m.SalesOrderItem.SalesOrderWhereSalesOrderItem } },
            new RolePattern(m.SalesOrderItem, m.SalesOrderItem.QuantityOrdered) { Steps =  new IPropertyType[] {m.SalesOrderItem.SalesOrderWhereSalesOrderItem } },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrder>())
            {
                var validOrderItems = @this.SalesOrderItems.Where(v => v.IsValid).ToArray();

                if (@this.SalesOrderState.Equals(new SalesOrderStates(@this.Strategy.Transaction).InProcess))
                {
                    var somethingToShip = false;
                    var allItemsAvailable = true;

                    foreach (var salesOrderItem in validOrderItems)
                    {
                        if (!@this.PartiallyShip && salesOrderItem.QuantityRequestsShipping != salesOrderItem.QuantityOrdered)
                        {
                            allItemsAvailable = false;
                            break;
                        }

                        if (@this.PartiallyShip && salesOrderItem.QuantityRequestsShipping > 0)
                        {
                            somethingToShip = true;
                        }
                    }

                    @this.CanShip = (!@this.PartiallyShip && allItemsAvailable) || somethingToShip;
                }
                else
                {
                    @this.CanShip = false;
                }
            }
        }
    }
}
