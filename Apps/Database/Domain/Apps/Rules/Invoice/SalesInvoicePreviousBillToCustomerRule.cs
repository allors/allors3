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
    using Resources;

    public class SalesInvoicePreviousBillToCustomerRule : Rule
    {
        public SalesInvoicePreviousBillToCustomerRule(MetaPopulation m) : base(m, new Guid("ff2512b3-d50f-4972-81b7-3cd1ad1bc47b")) =>
            this.Patterns = new Pattern[]
        {
            m.SalesInvoice.RolePattern(v => v.BillToCustomer),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>())
            {
                if (@this.ExistBillToCustomer)
                {
                    @this.PreviousBillToCustomer = @this.BillToCustomer;
                }
            }
        }
    }
}
