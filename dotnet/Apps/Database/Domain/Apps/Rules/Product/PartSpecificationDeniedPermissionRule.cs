// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class PartSpecificationDeniedPermissionRule : Rule
    {
        public PartSpecificationDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("6764a4ea-6335-4252-8957-93b352df8d7b")) =>
            this.Patterns = new Pattern[]
        {
            m.PartSpecification.RolePattern(v => v.TransitionalRevocations),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PartSpecification>())
            {
                @this.Revocations = @this.TransitionalRevocations;
            }
        }
    }
}
