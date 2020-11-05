// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class SalesOrderCanShipDerivation : DomainDerivation
    {
        public SalesOrderCanShipDerivation(M m) : base(m, new Guid("3f3129c8-2a62-4d2f-8652-cf2d503539a5")) =>
            this.Patterns = new Pattern[]
        {
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<SalesOrder>())
            {
                var validOrderItems = @this.SalesOrderItems.Where(v => v.IsValid).ToArray();

                if (@this.SalesOrderState.Equals(new SalesOrderStates(@this.Strategy.Session).InProcess))
                {
                    var somethingToShip = false;
                    var allItemsAvailable = true;

                    foreach (var salesOrderItem1 in validOrderItems)
                    {
                        if (!@this.PartiallyShip && salesOrderItem1.QuantityRequestsShipping != salesOrderItem1.QuantityOrdered)
                        {
                            allItemsAvailable = false;
                            break;
                        }

                        if (@this.PartiallyShip && salesOrderItem1.QuantityRequestsShipping > 0)
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
