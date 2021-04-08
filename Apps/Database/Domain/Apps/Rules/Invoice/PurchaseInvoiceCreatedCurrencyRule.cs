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
                m.PurchaseInvoice.RolePattern(v => v.AssignedCurrency),
                m.PurchaseInvoice.RolePattern(v => v.BilledFrom),
                m.Organisation.RolePattern(v => v.PreferredCurrency, v => v.PurchaseInvoicesWhereBilledTo),

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
