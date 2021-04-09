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

    public class PurchaseOrderCreatedShipToAddressRule : Rule
    {
        public PurchaseOrderCreatedShipToAddressRule(MetaPopulation m) : base(m, new Guid("1b414d7b-ad1f-412c-8856-03642c105bec")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrder.RolePattern(v => v.PurchaseOrderState),
                m.PurchaseOrder.RolePattern(v => v.AssignedShipToAddress),
                m.PurchaseOrder.RolePattern(v => v.OrderedBy),
                m.Organisation.RolePattern(v => v.ShippingAddress, v => v.PurchaseOrdersWhereOrderedBy),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseOrder>().Where(v => v.PurchaseOrderState.IsCreated))
            {
                @this.DerivedShipToAddress = @this.AssignedShipToAddress ?? @this.OrderedBy?.ShippingAddress;
            }
        }
    }
}
