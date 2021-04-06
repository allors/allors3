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

    public class PurchaseOrderCreatedDeriveBillToContactMechanismRule : Rule
    {
        public PurchaseOrderCreatedDeriveBillToContactMechanismRule(MetaPopulation m) : base(m, new Guid("afdc6169-7cd1-44ca-a257-84e4d2d3371d")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.AssignedBillToContactMechanism),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.OrderedBy),
                new RolePattern(m.Organisation, m.Organisation.GeneralCorrespondence) { Steps = new IPropertyType[] { m.Organisation.PurchaseOrdersWhereOrderedBy }},
                new RolePattern(m.Organisation, m.Organisation.BillingAddress) { Steps = new IPropertyType[] { m.Organisation.PurchaseOrdersWhereOrderedBy }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseOrder>().Where(v => v.PurchaseOrderState.IsCreated))
            {
                @this.DerivedBillToContactMechanism = @this.AssignedBillToContactMechanism ?? @this.OrderedBy?.BillingAddress ?? @this.OrderedBy?.GeneralCorrespondence;
            }
        }
    }
}
