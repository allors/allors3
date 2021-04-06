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

    public class PurchaseOrderCreatedDeriveVatRegimeRule : Rule
    {
        public PurchaseOrderCreatedDeriveVatRegimeRule(MetaPopulation m) : base(m, new Guid("16c4358a-bbfd-405d-9c54-e7450b0721e6")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.PurchaseOrderState),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.AssignedVatRegime),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.OrderDate),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseOrder>().Where(v => v.PurchaseOrderState.IsCreated))
            {
                @this.DerivedVatRegime = @this.AssignedVatRegime;

                if (@this.ExistOrderDate)
                {
                    @this.DerivedVatRate = @this.DerivedVatRegime?.VatRates.First(v => v.FromDate <= @this.OrderDate && (!v.ExistThroughDate || v.ThroughDate >= @this.OrderDate));
                }
            }
        }
    }
}
