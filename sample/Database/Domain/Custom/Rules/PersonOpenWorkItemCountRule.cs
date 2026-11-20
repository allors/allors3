// <copyright file="PersonOpenWorkItemCountRule.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Derivations.Rules;
    using Meta;

    public class PersonOpenWorkItemCountRule : Rule
    {
        public PersonOpenWorkItemCountRule(MetaPopulation m) : base(m, new Guid("9b52bd7f-65af-4fdf-a62b-a6a74dc81a4b")) =>
            this.Patterns = new Pattern[]
            {
                // A person gained or lost a work item.
                m.Person.AssociationPattern(v => v.WorkItemsWhereAssignee),

                // A work item was done or reopened: the rule receives its assignee.
                m.WorkItem.RolePattern(v => v.Done, v => v.Assignee),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var person in matches.Cast<Person>())
            {
                person.OpenWorkItemCount = person.WorkItemsWhereAssignee.Count(v => !v.Done);
            }
        }
    }
}
