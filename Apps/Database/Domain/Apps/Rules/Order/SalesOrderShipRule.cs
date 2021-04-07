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

    public class SalesOrderShipRule : Rule
    {
        public SalesOrderShipRule(MetaPopulation m) : base(m, new Guid("563aea87-eb89-4c4c-864e-ccb3d294b785")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.SalesOrder, m.SalesOrder.CanShip),
                new RolePattern(m.Store, m.Store.AutoGenerateCustomerShipment) { Steps =  new IPropertyType[] {m.Store.InternalOrganisation, m.Organisation.SalesOrdersWhereTakenBy } },
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
