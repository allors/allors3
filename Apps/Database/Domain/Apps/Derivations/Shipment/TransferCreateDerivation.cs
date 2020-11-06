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

    public class TransferCreateDerivation : DomainDerivation
    {
        public TransferCreateDerivation(M m) : base(m, new Guid("962ad2a8-fe25-4607-9f38-540278d33104")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.ShipmentPackage.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<Transfer>())
            {
                if (!@this.ExistShipmentState)
                {
                    @this.ShipmentState = new ShipmentStates(@this.Strategy.Session).Created;
                }
            }
        }
    }
}
