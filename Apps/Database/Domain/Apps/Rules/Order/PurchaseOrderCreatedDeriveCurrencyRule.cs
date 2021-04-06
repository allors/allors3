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

    public class PurchaseOrderCreatedDeriveCurrencyRule : Rule
    {
        public PurchaseOrderCreatedDeriveCurrencyRule(MetaPopulation m) : base(m, new Guid("99e76e43-51ec-4a57-b8c6-438ad3dc9f57")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.PurchaseOrderState),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.AssignedCurrency),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.OrderedBy),
                new RolePattern(m.Organisation, m.Organisation.PreferredCurrency) { Steps = new IPropertyType[] { m.Organisation.PurchaseOrdersWhereOrderedBy }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseOrder>().Where(v => v.PurchaseOrderState.IsCreated))
            {
                @this.DerivedCurrency = @this.AssignedCurrency ?? @this.OrderedBy?.PreferredCurrency;
            }
        }
    }
}
