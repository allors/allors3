// <copyright file="WorkEffortInventoryAssignmentCostOfGoodsSoldDerivation.cs" company="Allors bvba">
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

    public class WorkEffortInventoryAssignmentWorkeffortNumberRule : Rule
    {
        public WorkEffortInventoryAssignmentWorkeffortNumberRule(MetaPopulation m) : base(m, new Guid("21a0c06c-0e47-4d8d-84a5-2e54b8e7f145")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkEffortInventoryAssignment.RolePattern(v => v.InventoryItem),
            m.WorkEffortInventoryAssignment.RolePattern(v => v.Quantity),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortInventoryAssignment>())
            {
                @this.DeriveWorkEffortInventoryAssignmentWorkeffortNumber(validation);
            }
        }
    }

    public static class WorkEffortInventoryAssignmentWorkeffortNumberRuleExtensions
    {
        public static void DeriveWorkEffortInventoryAssignmentWorkeffortNumber(this WorkEffortInventoryAssignment @this, IValidation validation) => @this.WorkEffortNumber = @this.Assignment?.WorkEffortNumber;
    }
}
