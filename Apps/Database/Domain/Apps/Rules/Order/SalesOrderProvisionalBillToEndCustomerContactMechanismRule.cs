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

    public class SalesOrderProvisionalBillToEndCustomerContactMechanismRule : Rule
    {
        public SalesOrderProvisionalBillToEndCustomerContactMechanismRule(MetaPopulation m) : base(m, new Guid("30d5d5ce-526b-4eac-8950-001e5c92cb37")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrder.RolePattern(v => v.SalesOrderState),
                m.SalesOrder.RolePattern(v => v.AssignedBillToEndCustomerContactMechanism),
                m.SalesOrder.RolePattern(v => v.BillToEndCustomer),
                m.Party.RolePattern(v => v.BillingAddress, v => v.SalesOrdersWhereBillToEndCustomer),
                m.Party.RolePattern(v => v.ShippingAddress, v => v.SalesOrdersWhereBillToEndCustomer),
                m.Party.RolePattern(v => v.GeneralCorrespondence, v => v.SalesOrdersWhereBillToEndCustomer),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrder>().Where(v => v.SalesOrderState.IsProvisional))
            {
                @this.DerivedBillToEndCustomerContactMechanism = @this.AssignedBillToEndCustomerContactMechanism ?? @this.BillToEndCustomer?.BillingAddress ?? @this.BillToEndCustomer?.ShippingAddress ?? @this.BillToEndCustomer?.GeneralCorrespondence;
            }
        }
    }
}
