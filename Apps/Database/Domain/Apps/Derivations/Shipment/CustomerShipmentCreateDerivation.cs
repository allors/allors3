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

    public class CustomerShipmentCreateDerivation : DomainDerivation
    {
        public CustomerShipmentCreateDerivation(M m) : base(m, new Guid("1f2ccc0a-167b-4f95-a27b-66a974bb28bc")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.CustomerShipment.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<CustomerShipment>())
            {
                if (!@this.ExistShipmentState)
                {
                    @this.ShipmentState = new ShipmentStates(@this.Strategy.Session).Created;
                }

                if (!@this.ExistReleasedManually)
                {
                    @this.ReleasedManually = false;
                }

                if (!@this.ExistHeldManually)
                {
                    @this.HeldManually = false;
                }

                if (!@this.ExistWithoutCharges)
                {
                    @this.WithoutCharges = false;
                }

                if (!@this.ExistStore)
                {
                    @this.Store = @this.Strategy.Session.Extent<Store>().First;
                }

                if (!@this.ExistEstimatedShipDate)
                {
                    @this.EstimatedShipDate = @this.Session().Now().Date;
                }

                if (!@this.ExistCarrier && @this.ExistStore)
                {
                    @this.Carrier = @this.Store.DefaultCarrier;
                }
            }
        }
    }
}
