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

    public class SalesOrderProvisionalDeriveLocaleRule : Rule
    {
        public SalesOrderProvisionalDeriveLocaleRule(MetaPopulation m) : base(m, new Guid("bed2536f-3dd4-4376-8fe4-6736c4dcc24b")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.SalesOrder, m.SalesOrder.SalesOrderState),
                new RolePattern(m.SalesOrder, m.SalesOrder.Locale),
                new RolePattern(m.SalesOrder, m.SalesOrder.BillToCustomer),
                m.Party.RolePattern(v => v.Locale, v => v.SalesOrdersWhereBillToCustomer),
                new RolePattern(m.Organisation, m.Organisation.Locale) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrdersWhereTakenBy }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
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
