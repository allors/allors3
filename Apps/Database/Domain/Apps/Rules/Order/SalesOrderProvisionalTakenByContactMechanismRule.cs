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

    public class SalesOrderProvisionalTakenByContactMechanismRule : Rule
    {
        public SalesOrderProvisionalTakenByContactMechanismRule(MetaPopulation m) : base(m, new Guid("2fe58115-5005-4012-850f-2a13743076c8")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrder.RolePattern(v => v.SalesOrderState),
                m.SalesOrder.RolePattern(v => v.TakenBy),
                m.SalesOrder.RolePattern(v => v.AssignedTakenByContactMechanism),
                m.Organisation.RolePattern(v => v.OrderAddress, v => v.SalesOrdersWhereTakenBy),
                m.Organisation.RolePattern(v => v.BillingAddress, v => v.SalesOrdersWhereTakenBy),
                m.Organisation.RolePattern(v => v.GeneralCorrespondence, v => v.SalesOrdersWhereTakenBy),
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
