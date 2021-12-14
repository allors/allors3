
// <copyright file="WorkRequirementFixedAssetNameRule.cs" company="Allors bvba">
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

    public class WorkRequirementFixedAssetNameRule : Rule
    {
        public WorkRequirementFixedAssetNameRule(MetaPopulation m) : base(m, new Guid("02be092e-04ca-4cce-9255-19b562ee6dd4")) =>
            this.Patterns = new Pattern[]
        {
            m.FixedAsset.RolePattern(v => v.DisplayName, v => v.WorkRequirementsWhereFixedAsset),
            m.WorkRequirement.RolePattern(v => v.FixedAsset),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkRequirement>())
            {
                @this.FixedAssetName = @this.FixedAsset?.DisplayName;
            }
        }
    }
}
