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
    using Resources;

    public class WorkTaskCurrencyRule : Rule
    {
        public WorkTaskCurrencyRule(MetaPopulation m) : base(m, new Guid("7c758786-8230-43c9-8386-883caebded2e")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkEffort.RolePattern(v => v.Currency),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkTask>())
            {
                if (@this.ExistCurrentVersion
                    && !@this.Currency.Equals(@this.CurrentVersion.Currency)
                    && @this.WorkEffortInventoryAssignmentsWhereAssignment.Any(v => v.ExistAssignedUnitSellingPrice))
                {
                    validation.AddError(@this, @this.Meta.Currency, ErrorMessages.InvalidWorkTaskCurrency);
                }
            }
        }
    }
}
