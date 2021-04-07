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

    public class SalesOrderProvisionalDeriveCurrencyRule : Rule
    {
        public SalesOrderProvisionalDeriveCurrencyRule(MetaPopulation m) : base(m, new Guid("d61da60f-f6aa-4103-a8cc-eefe74c0a27e")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.SalesOrder, m.SalesOrder.SalesOrderState),
                new RolePattern(m.SalesOrder, m.SalesOrder.AssignedCurrency),
                new RolePattern(m.SalesOrder, m.SalesOrder.BillToCustomer),
                new RolePattern(m.SalesOrder, m.SalesOrder.TakenBy),
                new RolePattern(m.Party, m.Party.PreferredCurrency) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToCustomer }},
                new RolePattern(m.Organisation, m.Organisation.PreferredCurrency) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrdersWhereTakenBy }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrder>().Where(v => v.SalesOrderState.IsProvisional))
            {
                @this.DerivedCurrency = @this.AssignedCurrency ?? @this.BillToCustomer?.PreferredCurrency ?? @this.BillToCustomer?.Locale?.Country?.Currency ?? @this.TakenBy?.PreferredCurrency;
            }
        }
    }
}
