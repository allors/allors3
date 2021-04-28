
// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class WorkTaskExecutedByRule : Rule
    {
        public WorkTaskExecutedByRule(MetaPopulation m) : base(m, new Guid("12794dc5-8a79-4983-b480-4324602ae717")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkTask.RolePattern(v => v.TakenBy),
            m.WorkTask.RolePattern(v => v.ExecutedBy),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkTask>())
            {
                if (!@this.ExistExecutedBy && @this.ExistTakenBy)
                {
                    @this.ExecutedBy = @this.TakenBy;
                }
            }
        }
    }
}
