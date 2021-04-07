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

    public class SalesOrderProvisionalDerivePaymentMethodRule : Rule
    {
        public SalesOrderProvisionalDerivePaymentMethodRule(MetaPopulation m) : base(m, new Guid("bb9b637c-4594-4f9e-927c-bd47236ab515")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.SalesOrder, m.SalesOrder.SalesOrderState),
                new RolePattern(m.SalesOrder, m.SalesOrder.TakenBy),
                new RolePattern(m.SalesOrder, m.SalesOrder.Store),
                new RolePattern(m.SalesOrder, m.SalesOrder.AssignedPaymentMethod),
                new RolePattern(m.Organisation, m.Organisation.DefaultPaymentMethod) { Steps = new IPropertyType[] { this.M.Organisation.SalesOrdersWhereTakenBy }},
                new RolePattern(m.Store, m.Store.DefaultCollectionMethod) { Steps = new IPropertyType[] { this.M.Store.SalesOrdersWhereStore }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrder>().Where(v => v.SalesOrderState.IsProvisional))
            {
                @this.DerivedPaymentMethod = @this.AssignedPaymentMethod ?? @this.TakenBy?.DefaultPaymentMethod ?? @this.Store?.DefaultCollectionMethod;
            }
        }
    }
}
