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

    public class SalesOrderProvisionalDeriveVatRegimeRule : Rule
    {
        public SalesOrderProvisionalDeriveVatRegimeRule(MetaPopulation m) : base(m, new Guid("221d0439-b040-48f0-a5b8-07af9e78dd70")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.SalesOrder, m.SalesOrder.SalesOrderState),
                new RolePattern(m.SalesOrder, m.SalesOrder.AssignedVatRegime),
                new RolePattern(m.SalesOrder, m.SalesOrder.AssignedVatClause),
                new RolePattern(m.SalesOrder, m.SalesOrder.OrderDate),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
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

                if (@this.ExistDerivedVatRegime)
                {
                    if (@this.DerivedVatRegime.ExistVatClause)
                    {
                        @this.DerivedVatClause = @this.DerivedVatRegime.VatClause;
                    }
                    else
                    {
                        @this.RemoveDerivedVatClause();
                    }
                }
                else
                {
                    @this.RemoveDerivedVatClause();
                }

                @this.DerivedVatClause = @this.ExistAssignedVatClause ? @this.AssignedVatClause : @this.DerivedVatClause;
            }
        }
    }
}
