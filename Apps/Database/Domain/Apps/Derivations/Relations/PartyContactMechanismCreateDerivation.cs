// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class PartyContactMechanismCreateDerivation : DomainDerivation
    {
        public PartyContactMechanismCreateDerivation(M m) : base(m, new Guid("68fd22c2-b510-49fc-9fa6-eec63c921a20")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.PartyContactMechanism.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<PartyContactMechanism>())
            {
                if (!@this.ExistFromDate)
                {
                    @this.FromDate = @this.Session().Now();
                }
            }
        }
    }
}
