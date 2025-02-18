// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class SalesOrderProvisionalShipmentMethodRule : Rule
    {
        public SalesOrderProvisionalShipmentMethodRule(MetaPopulation m) : base(m, new Guid("c22988df-9bf2-44a8-8974-bc798b7c318c")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrder.RolePattern(v => v.SalesOrderState),
                m.SalesOrder.RolePattern(v => v.AssignedShipmentMethod),
                m.SalesOrder.RolePattern(v => v.ShipToCustomer),
                m.SalesOrder.RolePattern(v => v.Store),
                m.Party.RolePattern(v => v.DefaultShipmentMethod, v => v.SalesOrdersWhereShipToCustomer),
                m.Store.RolePattern(v => v.DefaultShipmentMethod, v => v.SalesOrdersWhereStore),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrder>().Where(v => v.SalesOrderState.IsProvisional))
            {
                @this.DerivedShipmentMethod = @this.AssignedShipmentMethod ?? @this.ShipToCustomer?.DefaultShipmentMethod ?? @this.Store?.DefaultShipmentMethod;
            }
        }
    }
}
