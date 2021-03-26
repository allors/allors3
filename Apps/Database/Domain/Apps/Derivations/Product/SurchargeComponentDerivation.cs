// <copyright file="SurchargeComponentDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Database.Derivations;

    public class SurchargeComponentDerivation : DomainDerivation
    {
        public SurchargeComponentDerivation(M m) : base(m, new Guid("1C8B75D1-3288-4DB7-987E-7A64A3225891")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.SurchargeComponent.Price),
                new RolePattern(m.SurchargeComponent.Percentage),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SurchargeComponent>())
            {
                validation.AssertExistsAtMostOne(@this, this.M.SurchargeComponent.Price, this.M.SurchargeComponent.Percentage);
            }
        }
    }
}
