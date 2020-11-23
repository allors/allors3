// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;
    using Derivations;

    public class CostCenterCategoryDerivation : DomainDerivation
    {
        public CostCenterCategoryDerivation(M m) : base(m, new Guid("1fa1c708-b002-41d7-bd3b-db14e4d018d1")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.CostCenterCategory.Description),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<CostCenterCategory>())
            {
                validation.AssertExists(@this, @this.Meta.Description);
            }
        }
    }
}
