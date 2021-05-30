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

    public class SalesInvoiceReadyForPostingDerivedBillToContactMechanismRule : Rule
    {
        public SalesInvoiceReadyForPostingDerivedBillToContactMechanismRule(MetaPopulation m) : base(m, new Guid("799b8588-06f8-46d4-9067-eb21e14484fc")) =>
            this.Patterns = new Pattern[]
        {
            m.SalesInvoice.RolePattern(v => v.AssignedBillToContactMechanism),
            m.Party.RolePattern(v => v.BillingAddress, v => v.SalesInvoicesWhereBillToCustomer),
            m.SalesInvoice.RolePattern(v => v.BillToCustomer),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>().Where(v => v.SalesInvoiceState.IsReadyForPosting))
            {
                @this.DerivedBillToContactMechanism = @this.AssignedBillToContactMechanism ?? @this.BillToCustomer?.BillingAddress;
            }
        }
    }
}
