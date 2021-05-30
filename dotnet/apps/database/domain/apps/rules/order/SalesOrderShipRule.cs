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

    public class SalesOrderShipRule : Rule
    {
        public SalesOrderShipRule(MetaPopulation m) : base(m, new Guid("563aea87-eb89-4c4c-864e-ccb3d294b785")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrder.RolePattern(v => v.CanShip),
                m.Store.RolePattern(v => v.AutoGenerateCustomerShipment, v => v.InternalOrganisation.InternalOrganisation.AsOrganisation.SalesOrdersWhereTakenBy),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SalesOrder>())
            {
                if (@this.CanShip && @this.Store.AutoGenerateCustomerShipment)
                {
                    @this.Ship();
                }
            }
        }
    }
}
