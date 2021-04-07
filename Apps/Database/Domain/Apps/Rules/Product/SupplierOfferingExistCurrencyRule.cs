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

    public class SupplierOfferingExistCurrencyRule : Rule
    {
        public SupplierOfferingExistCurrencyRule(MetaPopulation m) : base(m, new Guid("c80df6c4-cdaf-49bd-af52-200e1dea2a9b")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.SupplierOffering, m.SupplierOffering.Price),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var m = cycle.Transaction.Database.Context().M;
            foreach (var @this in matches.Cast<SupplierOffering>())
            {
                if (!@this.ExistCurrency)
                {
                    @this.Currency = @this.Transaction().GetSingleton().Settings.PreferredCurrency;
                }
            }
        }
    }
}
