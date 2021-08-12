// <copyright file="WorkEffortGrandTotalDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Meta;
    using Allors.Database.Derivations;
    using Derivations.Rules;

    public class WorkEffortGrandTotalRule : Rule
    {
        public WorkEffortGrandTotalRule(MetaPopulation m) : base(m, new Guid("f3c42add-acb8-49f4-9e1f-5c67378355f2")) =>
            this.Patterns = new[]
            {
                m.WorkEffort.RolePattern(v => v.TotalLabourRevenue),
                m.WorkEffort.RolePattern(v => v.TotalMaterialRevenue),
                m.WorkEffort.RolePattern(v => v.TotalSubContractedRevenue),
                m.WorkEffort.RolePattern(v => v.TotalOtherRevenue),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkEffort>())
            {
                @this.GrandTotal = Rounder.RoundDecimal(@this.TotalLabourRevenue + @this.TotalMaterialRevenue + @this.TotalSubContractedRevenue + @this.TotalOtherRevenue, 2);
            }
        }
    }
}
