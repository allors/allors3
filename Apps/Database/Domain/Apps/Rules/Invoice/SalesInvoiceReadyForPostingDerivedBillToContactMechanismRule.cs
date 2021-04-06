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

    public class SalesInvoiceReadyForPostingDerivedBillToContactMechanismRule : Rule
    {
        public SalesInvoiceReadyForPostingDerivedBillToContactMechanismRule(MetaPopulation m) : base(m, new Guid("799b8588-06f8-46d4-9067-eb21e14484fc")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.SalesInvoice, m.SalesInvoice.AssignedBillToContactMechanism),
            new RolePattern(m.Party, m.Party.BillingAddress) { Steps = new IPropertyType[] { this.M.Party.SalesInvoicesWhereBillToCustomer }},
            new RolePattern(m.SalesInvoice, m.SalesInvoice.BillToCustomer),
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
