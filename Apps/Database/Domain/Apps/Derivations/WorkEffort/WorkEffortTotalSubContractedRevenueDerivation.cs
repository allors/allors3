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

    public class WorkEffortTotalSubContractedRevenueDerivation : DomainDerivation
    {
        public WorkEffortTotalSubContractedRevenueDerivation(M m) : base(m, new Guid("102b3442-e1a6-4ff1-aec8-1620e345821c")) =>
            this.Patterns = new Pattern[]
            {
                new AssociationPattern(m.WorkEffortPurchaseOrderItemAssignment.Assignment),
                new RolePattern(m.WorkEffortPurchaseOrderItemAssignment, m.WorkEffortPurchaseOrderItemAssignment.Quantity) { Steps = new IPropertyType[] { m.WorkEffortPurchaseOrderItemAssignment.Assignment } },
                new RolePattern(m.WorkEffortPurchaseOrderItemAssignment, m.WorkEffortPurchaseOrderItemAssignment.UnitSellingPrice) { Steps = new IPropertyType[] { m.WorkEffortPurchaseOrderItemAssignment.Assignment } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkEffort>())
            {
                @this.TotalSubContractedRevenue = Math.Round(@this.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment.Sum(v => v.Quantity * v.UnitSellingPrice), 2);
            }
        }
    }
}
