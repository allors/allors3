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

    public class SalesOrderProvisionalVatRegimeRule : Rule
    {
        public SalesOrderProvisionalVatRegimeRule(MetaPopulation m) : base(m, new Guid("221d0439-b040-48f0-a5b8-07af9e78dd70")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrder.RolePattern(v => v.SalesOrderState),
                m.SalesOrder.RolePattern(v => v.AssignedVatRegime),
                m.SalesOrder.RolePattern(v => v.OrderDate),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrder>().Where(v => v.SalesOrderState.IsProvisional))
            {
                @this.DerivedVatRegime = @this.AssignedVatRegime;

                if (@this.ExistOrderDate)
                {
                    @this.DerivedVatRate = @this.DerivedVatRegime?.VatRates.First(v => v.FromDate <= @this.OrderDate && (!v.ExistThroughDate || v.ThroughDate >= @this.OrderDate));
                }
            }
        }
    }
}
