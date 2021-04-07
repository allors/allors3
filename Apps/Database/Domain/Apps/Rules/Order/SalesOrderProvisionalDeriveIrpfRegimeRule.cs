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

    public class SalesOrderProvisionalDeriveIrpfRegimeRule : Rule
    {
        public SalesOrderProvisionalDeriveIrpfRegimeRule(MetaPopulation m) : base(m, new Guid("f4017b40-d052-4760-a414-0a20b83ec9cc")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.SalesOrder, m.SalesOrder.SalesOrderState),
                new RolePattern(m.SalesOrder, m.SalesOrder.AssignedIrpfRegime),
                new RolePattern(m.SalesOrder, m.SalesOrder.OrderDate),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrder>().Where(v => v.SalesOrderState.IsProvisional))
            {
                @this.DerivedIrpfRegime = @this.AssignedIrpfRegime;
                
                if (@this.ExistOrderDate)
                {
                    @this.DerivedIrpfRate = @this.DerivedIrpfRegime?.IrpfRates.First(v => v.FromDate <= @this.OrderDate && (!v.ExistThroughDate || v.ThroughDate >= @this.OrderDate));
                }
            }
        }
    }
}
