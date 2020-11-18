// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Domain.Derivations;
    using Allors.Database.Meta;
    using Database.Derivations;

    public class ExpenseEntryDerivation : DomainDerivation
    {
        public ExpenseEntryDerivation(M m) : base(m, new Guid("765b3252-66a0-4358-8f7b-1765a1d8cf53")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.ExpenseEntry.WorkEffort),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<ExpenseEntry>())
            {

                validation.AssertAtLeastOne(@this, @this.M.ExpenseEntry.WorkEffort, @this.M.ExpenseEntry.EngagementItem);
            }
        }
    }
}
