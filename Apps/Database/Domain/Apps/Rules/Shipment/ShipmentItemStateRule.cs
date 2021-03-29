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

    public class ShipmentItemStateRule : Rule
    {
        public ShipmentItemStateRule(M m) : base(m, new Guid("d376e940-9630-46aa-9d6c-e3dc84ee2ede")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Shipment, m.Shipment.ShipmentState) { Steps = new IPropertyType[] {m.Shipment.ShipmentItems} },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<ShipmentItem>())
            {
                var shipment = @this.ShipmentWhereShipmentItem;

                if (shipment.ShipmentState.IsPicked && !@this.ShipmentItemState.IsPicked)
                {
                    @this.ShipmentItemState = new ShipmentItemStates(@this.Transaction()).Picked;
                }

                if (shipment.ShipmentState.IsPacked && !@this.ShipmentItemState.IsPacked)
                {
                    @this.ShipmentItemState = new ShipmentItemStates(@this.Transaction()).Packed;
                }
            }
        }
    }
}
