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

    public class PurchaseShipmentCreateDerivation : DomainDerivation
    {
        public PurchaseShipmentCreateDerivation(M m) : base(m, new Guid("ddfe71a6-b706-4280-a05e-7efe1e0e58ca")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.PurchaseShipment.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<PurchaseShipment>())
            {
                if (!@this.ExistShipToParty)
                {
                    var internalOrganisations = new Organisations(@this.Strategy.Session).InternalOrganisations();
                    if (internalOrganisations.Length == 1)
                    {
                        @this.ShipToParty = internalOrganisations.First();
                    }
                }

                if (!@this.ExistShipmentState)
                {
                    @this.ShipmentState = new ShipmentStates(@this.Strategy.Session).Created;
                }

                if (!@this.ExistEstimatedArrivalDate)
                {
                    @this.EstimatedArrivalDate = @this.Session().Now().Date;
                }
            }
        }
    }
}
