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

    public class CustomerReturnCreateDerivation : DomainDerivation
    {
        public CustomerReturnCreateDerivation(M m) : base(m, new Guid("bef5a3cb-4252-4e28-85f3-627eba200b98")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.CustomerReturn.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<CustomerReturn>())
            {
                if (!@this.ExistShipmentState)
                {
                    @this.ShipmentState = new ShipmentStates(@this.Strategy.Session).Received;
                }
            }
        }
    }
}
