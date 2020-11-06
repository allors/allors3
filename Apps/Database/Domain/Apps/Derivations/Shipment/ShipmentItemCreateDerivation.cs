// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public class ShipmentItemCreateDerivation : DomainDerivation
    {
        public ShipmentItemCreateDerivation(M m) : base(m, new Guid("ef0c60ec-a60b-45ae-b2c0-d01594634cf5")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.ShipmentItem.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<ShipmentItem>())
            {
                if (!@this.ExistShipmentItemState)
                {
                    @this.ShipmentItemState = new ShipmentItemStates(@this.Strategy.Session).Created;
                }
            }
        }
    }
}
