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

    public class PurchaseOrderCreatedTakenViaContactMechanismRule : Rule
    {
        public PurchaseOrderCreatedTakenViaContactMechanismRule(MetaPopulation m) : base(m, new Guid("f4a7358e-8f6a-4178-bccb-d27aa3a35918")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrder.RolePattern(v => v.PurchaseOrderState),
                m.PurchaseOrder.RolePattern(v => v.AssignedTakenViaContactMechanism),
                m.PurchaseOrder.RolePattern(v => v.TakenViaSupplier),
                m.Organisation.RolePattern(v => v.OrderAddress, v => v.PurchaseOrdersWhereTakenViaSupplier),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseOrder>().Where(v => v.PurchaseOrderState.IsCreated))
            {
                @this.DerivedTakenViaContactMechanism = @this.AssignedTakenViaContactMechanism ?? @this.TakenViaSupplier?.OrderAddress;
            }
        }
    }
}
