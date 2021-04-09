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

    public class SalesOrderProvisionalBillToContactMechanismRule : Rule
    {
        public SalesOrderProvisionalBillToContactMechanismRule(MetaPopulation m) : base(m, new Guid("1dba5834-84ff-4fe0-8c86-32ed97db792d")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrder.RolePattern(v => v.SalesOrderState),
                m.SalesOrder.RolePattern(v => v.AssignedBillToContactMechanism),
                m.SalesOrder.RolePattern(v => v.BillToCustomer),
                m.Party.RolePattern(v => v.BillingAddress, v => v.SalesOrdersWhereBillToCustomer),
                m.Party.RolePattern(v => v.ShippingAddress, v => v.SalesOrdersWhereBillToCustomer),
                m.Party.RolePattern(v => v.GeneralCorrespondence, v => v.SalesOrdersWhereBillToCustomer),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrder>().Where(v => v.SalesOrderState.IsProvisional))
            {
                @this.DerivedBillToContactMechanism = @this.AssignedBillToContactMechanism ?? @this.BillToCustomer?.BillingAddress ?? @this.BillToCustomer?.ShippingAddress ?? @this.BillToCustomer?.GeneralCorrespondence;
            }
        }
    }
}
