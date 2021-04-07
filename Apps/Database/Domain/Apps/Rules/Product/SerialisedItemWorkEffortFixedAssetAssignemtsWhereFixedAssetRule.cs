// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Derivations;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class SerialisedItemWorkEffortFixedAssetAssignemtsWhereFixedAssetRule : Rule
    {
        public SerialisedItemWorkEffortFixedAssetAssignemtsWhereFixedAssetRule(MetaPopulation m) : base(m, new Guid("195a4083-b835-4e84-94d5-07a40f20806c")) =>
            this.Patterns = new Pattern[]
            {
                new AssociationPattern(m.WorkEffortFixedAssetAssignment.FixedAsset),
                new RolePattern(m.WorkEffort, m.WorkEffort.WorkEffortState) { Steps = new IPropertyType[] { m.WorkEffort.WorkEffortFixedAssetAssignmentsWhereAssignment, m.WorkEffortFixedAssetAssignment.FixedAsset } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.OnWorkEffort = @this.WorkEffortFixedAssetAssignmentsWhereFixedAsset.Any(v => v.ExistAssignment
                            && (v.Assignment.WorkEffortState.IsCreated || v.Assignment.WorkEffortState.IsInProgress));

            }
        }
    }
}
