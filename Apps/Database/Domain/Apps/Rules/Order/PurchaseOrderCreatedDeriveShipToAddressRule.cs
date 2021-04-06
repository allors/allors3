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

    public class PurchaseOrderCreatedDeriveShipToAddressRule : Rule
    {
        public PurchaseOrderCreatedDeriveShipToAddressRule(MetaPopulation m) : base(m, new Guid("1b414d7b-ad1f-412c-8856-03642c105bec")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.PurchaseOrderState),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.AssignedShipToAddress),
                new RolePattern(m.PurchaseOrder, m.PurchaseOrder.OrderedBy),
                new RolePattern(m.Organisation, m.Organisation.ShippingAddress) { Steps = new IPropertyType[] { m.Organisation.PurchaseOrdersWhereOrderedBy }},
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
