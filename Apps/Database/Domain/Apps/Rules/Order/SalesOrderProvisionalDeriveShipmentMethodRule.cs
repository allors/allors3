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

    public class SalesOrderProvisionalDeriveShipmentMethodRule : Rule
    {
        public SalesOrderProvisionalDeriveShipmentMethodRule(MetaPopulation m) : base(m, new Guid("c22988df-9bf2-44a8-8974-bc798b7c318c")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.SalesOrder, m.SalesOrder.SalesOrderState),
                new RolePattern(m.SalesOrder, m.SalesOrder.AssignedShipmentMethod),
                new RolePattern(m.SalesOrder, m.SalesOrder.ShipToCustomer),
                new RolePattern(m.SalesOrder, m.SalesOrder.Store),
                new RolePattern(m.Party, m.Party.DefaultShipmentMethod) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereShipToCustomer }},
                new RolePattern(m.Store, m.Store.DefaultShipmentMethod) { Steps = new IPropertyType[] { this.M.Store.SalesOrdersWhereStore }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
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
