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

    public class SalesOrderProvisionalDeriveTakenByContactMechanismRule : Rule
    {
        public SalesOrderProvisionalDeriveTakenByContactMechanismRule(MetaPopulation m) : base(m, new Guid("2fe58115-5005-4012-850f-2a13743076c8")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.SalesOrder, m.SalesOrder.SalesOrderState),
                new RolePattern(m.SalesOrder, m.SalesOrder.TakenBy),
                new RolePattern(m.SalesOrder, m.SalesOrder.AssignedTakenByContactMechanism),
                new RolePattern(m.Organisation, m.Organisation.OrderAddress) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrdersWhereTakenBy }},
                new RolePattern(m.Organisation, m.Organisation.BillingAddress) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrdersWhereTakenBy }},
                new RolePattern(m.Organisation, m.Organisation.GeneralCorrespondence) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrdersWhereTakenBy }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrder>().Where(v => v.SalesOrderState.IsProvisional))
            {
                @this.DerivedTakenByContactMechanism = @this.AssignedTakenByContactMechanism ?? @this.TakenBy?.OrderAddress ?? @this.TakenBy?.BillingAddress ?? @this.TakenBy?.GeneralCorrespondence;
            }
        }
    }
}
