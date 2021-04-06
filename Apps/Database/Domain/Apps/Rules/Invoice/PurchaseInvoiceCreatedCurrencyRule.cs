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

    public class PurchaseInvoiceCreatedCurrencyRule : Rule
    {
        public PurchaseInvoiceCreatedCurrencyRule(MetaPopulation m) : base(m, new Guid("488995ea-3b77-4c84-8679-fe0a6f071feb")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PurchaseInvoice, m.PurchaseInvoice.AssignedCurrency),
                new RolePattern(m.PurchaseInvoice, m.PurchaseInvoice.BilledFrom),
                new RolePattern(m.Organisation, m.Organisation.PreferredCurrency) { Steps = new IPropertyType[] { m.Organisation.PurchaseInvoicesWhereBilledTo }},

            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseInvoice>().Where(v => v.PurchaseInvoiceState.IsCreated))
            {
                @this.DerivedCurrency = @this.AssignedCurrency ?? @this.BilledTo?.PreferredCurrency;
            }
        }
    }
}
