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

    public class SalesInvoiceReadyForPostingDerivedBilledFromContactMechanismRule : Rule
    {
        public SalesInvoiceReadyForPostingDerivedBilledFromContactMechanismRule(MetaPopulation m) : base(m, new Guid("9f54a488-1768-4292-99c4-4d16ae86f942")) =>
            this.Patterns = new Pattern[]
        {
            m.SalesInvoice.RolePattern(v => v.BilledFrom),
            m.SalesInvoice.RolePattern(v => v.AssignedBilledFromContactMechanism),
            m.Organisation.RolePattern(v => v.BillingAddress, v => v.SalesInvoicesWhereBilledFrom),
            m.Organisation.RolePattern(v => v.GeneralCorrespondence, v => v.SalesInvoicesWhereBilledFrom),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>().Where(v => v.SalesInvoiceState.IsReadyForPosting))
            {
                @this.DerivedBilledFromContactMechanism = @this.AssignedBilledFromContactMechanism ?? @this.BilledFrom?.BillingAddress ?? @this.BilledFrom?.GeneralCorrespondence;
            }
        }
    }
}
