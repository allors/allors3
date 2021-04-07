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

    public class SalesOrderProvisionalDeriveBillToEndCustomerContactMechanismRule : Rule
    {
        public SalesOrderProvisionalDeriveBillToEndCustomerContactMechanismRule(MetaPopulation m) : base(m, new Guid("30d5d5ce-526b-4eac-8950-001e5c92cb37")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.SalesOrder, m.SalesOrder.SalesOrderState),
                new RolePattern(m.SalesOrder, m.SalesOrder.AssignedBillToEndCustomerContactMechanism),
                new RolePattern(m.SalesOrder, m.SalesOrder.BillToEndCustomer),
                new RolePattern(m.Party, m.Party.BillingAddress) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToEndCustomer }},
                new RolePattern(m.Party, m.Party.ShippingAddress) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToEndCustomer }},
                new RolePattern(m.Party, m.Party.GeneralCorrespondence) { Steps = new IPropertyType[] { this.M.Party.SalesOrdersWhereBillToEndCustomer }},
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
