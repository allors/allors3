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

    public class DropShipmentCreateDerivation : DomainDerivation
    {
        public DropShipmentCreateDerivation(M m) : base(m, new Guid("06cc9023-3bf0-4dab-bf4f-46e9dc4bc937")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.DropShipment.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<DropShipment>())
            {
                if (!@this.ExistShipmentState)
                {
                    @this.ShipmentState = new ShipmentStates(@this.Strategy.Session).Created;
                }
            }
        }
    }
}
