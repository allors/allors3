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

    public class SalesOrderProvisionalLocaleRule : Rule
    {
        public SalesOrderProvisionalLocaleRule
            (MetaPopulation m) : base(m, new Guid("bed2536f-3dd4-4376-8fe4-6736c4dcc24b")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrder.RolePattern(v => v.SalesOrderState),
                m.SalesOrder.RolePattern(v => v.Locale),
                m.SalesOrder.RolePattern(v => v.BillToCustomer),
                m.Party.RolePattern(v => v.Locale, v => v.SalesOrdersWhereBillToCustomer),
                m.Organisation.RolePattern(v => v.Locale, v => v.SalesOrdersWhereTakenBy),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrder>().Where(v => v.SalesOrderState.IsProvisional))
            {
                @this.DerivedLocale = @this.Locale ?? @this.BillToCustomer?.Locale ?? @this.TakenBy?.Locale;
            }
        }
    }
}
