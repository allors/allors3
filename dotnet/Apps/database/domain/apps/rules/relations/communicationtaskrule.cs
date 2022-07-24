// <copyright file="Domain.cs" company="Allors bvba">
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

    public class CommunicationTaskRule : Rule
    {
        public CommunicationTaskRule(MetaPopulation m) : base(m, new Guid("0001CEF2-6A6F-4DB7-A932-07F854C66478")) =>
            this.Patterns = new Pattern[]
            {
                m.CommunicationTask.RolePattern(v => v.CommunicationEvent),
                m.CommunicationEvent.RolePattern(v => v.WorkItemDescription, v => v.CommunicationTasksWhereCommunicationEvent),
                m.CommunicationEvent.RolePattern(v => v.ActualEnd, v => v.CommunicationTasksWhereCommunicationEvent),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<CommunicationTask>())
            {
                @this.WorkItem = @this.CommunicationEvent;

                @this.Title = @this.CommunicationEvent.WorkItemDescription;

                // Services
                if (!@this.ExistDateClosed && @this.CommunicationEvent.ExistActualEnd)
                {
                    @this.DateClosed = @this.Transaction().Now();
                }
            }
        }
    }
}
