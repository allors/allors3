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

    public class PurchaseOrderCreatedBillToContactMechanismRule : Rule
    {
        public PurchaseOrderCreatedBillToContactMechanismRule(MetaPopulation m) : base(m, new Guid("afdc6169-7cd1-44ca-a257-84e4d2d3371d")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrder.RolePattern(v => v.AssignedBillToContactMechanism),
                m.PurchaseOrder.RolePattern(v => v.OrderedBy),
                m.Organisation.RolePattern(v => v.GeneralCorrespondence, v => v.PurchaseOrdersWhereOrderedBy),
                m.Organisation.RolePattern(v => v.BillingAddress, v => v.PurchaseOrdersWhereOrderedBy),

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
