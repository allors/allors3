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

    public class WorkRequirementDisplayNameRule : Rule
    {
        public WorkRequirementDisplayNameRule(MetaPopulation m) : base(m, new Guid("805ba781-6ab9-4126-8217-50fd80938958")) =>
            this.Patterns = new Pattern[]
            {
                m.Requirement.RolePattern(v => v.RequirementNumber, m.WorkRequirement),
                m.Requirement.RolePattern(v => v.Description, m.WorkRequirement),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkRequirement>())
            {
                @this.DeriveWorkRequirementDisplayName(validation);
            }
        }
    }

    public static class WorkRequirementDisplayNameRuleExtensions
    {
        public static void DeriveWorkRequirementDisplayName(this WorkRequirement @this, IValidation validation) => @this.DisplayName = $"{@this.RequirementNumber}: {@this.Description}";
    }
}
