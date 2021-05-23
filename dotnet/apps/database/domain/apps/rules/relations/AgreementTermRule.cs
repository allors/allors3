// <copyright file="Domain.cs" company="Allors bvba">
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

    public class AgreementTermRule : Rule
    {
        public AgreementTermRule(MetaPopulation m) : base(m, new Guid("2F28CF03-571A-4F7B-B71C-D8ACEBC734AC")) =>
            this.Patterns = new Pattern[]
            {
                m.AgreementTerm.RolePattern(v => v.TermType),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<AgreementTerm>())
            {
                cycle.Validation.AssertAtLeastOne(@this, this.M.AgreementTerm.TermType, this.M.AgreementTerm.Description);
            }
        }
    }
}
