// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Derivations;
    using Meta;
    using Derivations.Rules;

    public class AgreementProductApplicabilityRule : Rule
    {
        public AgreementProductApplicabilityRule(MetaPopulation m) : base(m, new Guid("43ca02e8-a35b-4249-bb48-ca980ac3e648")) =>
            this.Patterns = new Pattern[]
            {
                m.AgreementProductApplicability.RolePattern(v => v.Agreement),
                m.AgreementProductApplicability.RolePattern(v => v.AgreementItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<AgreementProductApplicability>())
            {
                validation.AssertExistsAtMostOne(@this, @this.Meta.Agreement, @this.Meta.AgreementItem);
                validation.AssertAtLeastOne(@this, @this.Meta.Agreement, @this.Meta.AgreementItem);
            }
        }
    }
}
