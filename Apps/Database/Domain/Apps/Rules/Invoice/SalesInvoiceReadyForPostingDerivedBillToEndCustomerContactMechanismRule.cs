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

    public class SalesInvoiceReadyForPostingDerivedBillToEndCustomerContactMechanismRule : Rule
    {
        public SalesInvoiceReadyForPostingDerivedBillToEndCustomerContactMechanismRule(MetaPopulation m) : base(m, new Guid("80a71b59-7c73-4e43-bb58-d127ffdc6d76")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.SalesInvoice, m.SalesInvoice.AssignedBillToEndCustomerContactMechanism),
            new RolePattern(m.Party, m.Party.BillingAddress) { Steps = new IPropertyType[] { this.M.Party.SalesInvoicesWhereBillToEndCustomer }},
            new RolePattern(m.SalesInvoice, m.SalesInvoice.BillToEndCustomer),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>().Where(v => v.SalesInvoiceState.IsReadyForPosting))
            {
                @this.DerivedBillToEndCustomerContactMechanism = @this.AssignedBillToEndCustomerContactMechanism ?? @this.BillToEndCustomer?.BillingAddress;
            }
        }
    }
}
