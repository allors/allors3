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

    public class PurchaseReturnCreateDerivation : DomainDerivation
    {
        public PurchaseReturnCreateDerivation(M m) : base(m, new Guid("6effabb9-3869-45ee-b6c6-401c989d6b26")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.PurchaseReturn.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<PurchaseReturn>())
            {
                if (!@this.ExistShipmentState)
                {
                    @this.ShipmentState = new ShipmentStates(@this.Strategy.Session).Created;
                }
            }
        }
    }
}
