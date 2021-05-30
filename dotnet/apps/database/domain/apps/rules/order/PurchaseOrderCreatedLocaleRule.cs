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

    public class PurchaseOrderCreatedLocaleRule : Rule
    {
        public PurchaseOrderCreatedLocaleRule(MetaPopulation m) : base(m, new Guid("d121d029-74ca-4980-8862-0e66a3418227")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrder.RolePattern(v => v.PurchaseOrderState),
                m.PurchaseOrder.RolePattern(v => v.Locale),
                m.PurchaseOrder.RolePattern(v => v.OrderedBy),
                m.Organisation.RolePattern(v => v.Locale, v => v.PurchaseOrdersWhereOrderedBy),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseOrder>().Where(v => v.PurchaseOrderState.IsCreated))
            {
                @this.DerivedLocale = @this.Locale ?? @this.OrderedBy?.Locale;
            }
        }
    }
}
