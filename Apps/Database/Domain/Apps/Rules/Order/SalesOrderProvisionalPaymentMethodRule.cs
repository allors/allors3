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

    public class SalesOrderProvisionalPaymentMethodRule : Rule
    {
        public SalesOrderProvisionalPaymentMethodRule(MetaPopulation m) : base(m, new Guid("bb9b637c-4594-4f9e-927c-bd47236ab515")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrder.RolePattern(v => v.SalesOrderState),
                m.SalesOrder.RolePattern(v => v.TakenBy),
                m.SalesOrder.RolePattern(v => v.Store),
                m.SalesOrder.RolePattern(v => v.AssignedPaymentMethod),
                m.Organisation.RolePattern(v => v.DefaultPaymentMethod, v => v.SalesOrdersWhereTakenBy),
                m.Store.RolePattern(v => v.DefaultCollectionMethod, v => v.SalesOrdersWhereStore),
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
