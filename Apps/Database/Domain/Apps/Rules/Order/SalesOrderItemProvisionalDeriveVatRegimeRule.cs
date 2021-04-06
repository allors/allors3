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

    public class SalesOrderItemProvisionalDeriveVatRegimeRule : Rule
    {
        public SalesOrderItemProvisionalDeriveVatRegimeRule(MetaPopulation m) : base(m, new Guid("69ce53b7-28ce-405b-83ae-1bb800d53048")) =>
            this.Patterns = new Pattern[]
            {
                new AssociationPattern(m.SalesOrder.SalesOrderItems),
                new RolePattern(m.SalesOrderItem, m.SalesOrderItem.SalesOrderItemState),
                new RolePattern(m.SalesOrderItem, m.SalesOrderItem.AssignedVatRegime),
                new RolePattern(m.SalesOrder, m.SalesOrder.DerivedVatRegime) { Steps =  new IPropertyType[] {m.SalesOrder.SalesOrderItems} },
                new RolePattern(m.SalesOrder, m.SalesOrder.OrderDate) { Steps =  new IPropertyType[] {m.SalesOrder.SalesOrderItems} },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrderItem>().Where(v => v.SalesOrderItemState.IsProvisional))
            {
                var salesOrder = @this.SalesOrderWhereSalesOrderItem;

                @this.DerivedVatRegime = @this.AssignedVatRegime ?? salesOrder?.DerivedVatRegime;
                @this.VatRate = @this.DerivedVatRegime?.VatRates.First(v => v.FromDate <= salesOrder.OrderDate && (!v.ExistThroughDate || v.ThroughDate >= salesOrder.OrderDate));
            }
        }
    }
}
