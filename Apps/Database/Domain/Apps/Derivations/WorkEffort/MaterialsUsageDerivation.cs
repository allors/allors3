// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Domain.Derivations;
    using Allors.Database.Meta;
    using Database.Derivations;

    public class MaterialsUsageDerivation : DomainDerivation
    {
        public MaterialsUsageDerivation(M m) : base(m, new Guid("13e6f997-e202-4b7a-a6a6-4037257ae4e5")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.MaterialsUsage.WorkEffort),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<MaterialsUsage>())
            {

                validation.AssertAtLeastOne(@this, @this.M.MaterialsUsage.WorkEffort, @this.M.MaterialsUsage.EngagementItem);
            }
        }
    }
}
