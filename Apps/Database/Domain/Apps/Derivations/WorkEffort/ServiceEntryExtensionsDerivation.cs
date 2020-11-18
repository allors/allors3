// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Meta;
    using Database.Derivations;

    public class ServiceEntryExtensionsDerivation : DomainDerivation
    {
        public ServiceEntryExtensionsDerivation(M m) : base(m, new Guid("05a4a8be-6ffd-421f-b596-757852033bf7")) =>
            this.Patterns = new Pattern[]
        {
            //Not sure
            new ChangedPattern(m.ServiceEntry.DerivationTrigger),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<ServiceEntry>())
            {
                if (!@this.ExistDerivationTrigger)
                {
                    @this.DerivationTrigger = Guid.NewGuid();
                }
            }
        }
    }
}
