// <copyright file="WorkEffortDisplayNameRule.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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

    public class WorkEffortDisplayNameRule : Rule
    {
        public WorkEffortDisplayNameRule(MetaPopulation m) : base(m, new Guid("bf081089-42fe-42b3-b287-baa83f93f734")) =>
            this.Patterns = new Pattern[]
            {
                m.WorkEffort.RolePattern(v => v.WorkEffortNumber),
                m.WorkEffort.RolePattern(v => v.Name),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffort>())
            {
                @this.DeriveWorkEffortDisplayName(validation);
            }
        }
    }

    public static class WorkEffortDisplayNameRuleExtensions
    {
        public static void DeriveWorkEffortDisplayName(this WorkEffort @this, IValidation validation)
        {
            @this.DisplayName = $"{ @this.WorkEffortNumber}, { @this.Name}";
        }
    }
}
