// <copyright file="TimeAndMaterialsServiceDisplayNameRule.cs" company="Allors bvba">
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

    public class TimeAndMaterialsServiceDisplayNameRule : Rule
    {
        public TimeAndMaterialsServiceDisplayNameRule(MetaPopulation m) : base(m, new Guid("a43bc227-eec2-4464-bf1e-50ba3acfe257")) =>
            this.Patterns = new Pattern[]
            {
                m.TimeAndMaterialsService.RolePattern(v => v.Name),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<TimeAndMaterialsService>())
            {
                @this.DeriveTimeAndMaterialsServiceDisplayNameRule(validation);
            }
        }
    }

    public static class TimeAndMaterialsServiceDisplayNameRuleExtensions
    {
        public static void DeriveTimeAndMaterialsServiceDisplayNameRule(this TimeAndMaterialsService @this, IValidation validation)
        {
            @this.DisplayName = @this.Name ?? "N/A";
        }
    }
}
