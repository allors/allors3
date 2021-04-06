// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class PurchaseOrderCreatedDeriveLocaleRule : Rule
    {
        public PurchaseOrderCreatedDeriveLocaleRule(MetaPopulation m) : base(m, new Guid("d121d029-74ca-4980-8862-0e66a3418227")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.PurchaseOrderState),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.Locale),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.OrderedBy),
                new RolePattern(m.Organisation, m.Organisation.Locale) { Steps = new IPropertyType[] { m.Organisation.PurchaseOrdersWhereOrderedBy }},
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
