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

    public class CustomerShipmentInvoiceRule : Rule
    {
        public CustomerShipmentInvoiceRule(MetaPopulation m) : base(m, new Guid("7cd3ff20-9b73-41f5-91fa-18c127f73afb")) =>
            this.Patterns = new Pattern[]
            {
                m.CustomerShipment.RolePattern(v => v.ShipmentState),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<CustomerShipment>())
            {

                if (@this.ExistShipmentState && !@this.ShipmentState.Equals(@this.LastShipmentState) &&
                    @this.ShipmentState.Equals(new ShipmentStates(@this.Strategy.Transaction).Shipped))
                {
                    if (Equals(@this.Store.BillingProcess, new BillingProcesses(@this.Strategy.Transaction).BillingForShipmentItems))
                    {
                        @this.Invoice();
                    }
                }
            }
        }
    }
}
