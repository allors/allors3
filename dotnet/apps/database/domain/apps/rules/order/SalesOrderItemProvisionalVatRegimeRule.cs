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
    using Derivations.Rules;

    public class SalesOrderItemProvisionalVatRegimeRule : Rule
    {
        public SalesOrderItemProvisionalVatRegimeRule(MetaPopulation m) : base(m, new Guid("69ce53b7-28ce-405b-83ae-1bb800d53048")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrderItem.AssociationPattern(v => v.SalesOrderWhereSalesOrderItem),
                m.SalesOrderItem.RolePattern(v => v.SalesOrderItemState),
                m.SalesOrderItem.RolePattern(v => v.AssignedVatRegime),
                m.SalesOrder.RolePattern(v => v.DerivedVatRegime, v => v.SalesOrderItems),
                m.SalesOrder.RolePattern(v => v.OrderDate, v => v.SalesOrderItems),
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
