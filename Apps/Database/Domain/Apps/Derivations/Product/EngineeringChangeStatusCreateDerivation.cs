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

    public class EngineeringChangeStatusCreateDerivation : DomainDerivation
    {
        public EngineeringChangeStatusCreateDerivation(M m) : base(m, new Guid("506f735c-301f-4d0e-8457-70c7e8792d75")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.EngineeringChangeStatus.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<EngineeringChangeStatus>())
            {
                if (!@this.ExistStartDateTime)
                {
                    @this.StartDateTime = @this.Session().Now();
                }
            }
        }
    }
}
