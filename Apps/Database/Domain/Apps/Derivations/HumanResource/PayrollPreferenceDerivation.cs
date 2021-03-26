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

    public class PayrollPreferenceDerivation : DomainDerivation
    {
        public PayrollPreferenceDerivation(M m) : base(m, new Guid("7223fce9-2df6-48bd-89a2-261fff4772de")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PayrollPreference, m.PayrollPreference.Percentage),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PayrollPreference>())
            {
                validation.AssertAtLeastOne(@this, @this.M.PayrollPreference.Amount, @this.M.PayrollPreference.Percentage);
                validation.AssertExistsAtMostOne(@this, @this.M.PayrollPreference.Amount, @this.M.PayrollPreference.Percentage);
            }
        }
    }
}
