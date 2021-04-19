// <copyright file="WorkEffortTotalSubContractedRevenueDerivation.cs" company="Allors bvba">
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

    public class WorkEffortTotalSubContractedRevenueRule : Rule
    {
        public WorkEffortTotalSubContractedRevenueRule(MetaPopulation m) : base(m, new Guid("102b3442-e1a6-4ff1-aec8-1620e345821c")) =>
            this.Patterns = new Pattern[]
            {
                m.WorkEffort.AssociationPattern(v => v.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment),
                m.WorkEffortPurchaseOrderItemAssignment.RolePattern(v => v.Quantity, v => v.Assignment),
                m.WorkEffortPurchaseOrderItemAssignment.RolePattern(v => v.UnitSellingPrice, v => v.Assignment),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkEffort>())
            {
                @this.TotalSubContractedRevenue = Rounder.RoundDecimal(@this.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment.Sum(v => v.Quantity * v.UnitSellingPrice), 2);
            }
        }
    }
}
