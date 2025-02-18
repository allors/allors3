
// <copyright file="WorkRequirementFixedAssetNameRule.cs" company="Allors bv">
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

    public class WorkRequirementFixedAssetNameRule : Rule
    {
        public WorkRequirementFixedAssetNameRule(MetaPopulation m) : base(m, new Guid("3d4cbe92-c285-4869-9682-c38aa1ae2145")) =>
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
