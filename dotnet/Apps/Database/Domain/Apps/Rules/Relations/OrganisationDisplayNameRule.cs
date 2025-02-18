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

    public class OrganisationDisplayNameRule : Rule
    {
        public OrganisationDisplayNameRule(MetaPopulation m) : base(m, new Guid("27c869fa-60ff-478e-abec-c42ff5ba606f")) =>
            this.Patterns = new Pattern[]
            {
                m.Organisation.RolePattern(v => v.Name),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Organisation>())
            {
                transaction.Prefetch(@this.PrefetchPolicy, @this);
                @this.DeriveOrganisationDisplayName(validation);
            }
        }
    }

    public static class OrganisationDisplayNameRuleExtensions
    {
        public static void DeriveOrganisationDisplayName(this Organisation @this, IValidation validation)
        {
            @this.DisplayName = @this.Name ?? "N/A";
        }
    }
}
