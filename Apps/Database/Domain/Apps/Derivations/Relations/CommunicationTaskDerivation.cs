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

    public class CommunicationTaskDerivation : DomainDerivation
    {
        public CommunicationTaskDerivation(M m) : base(m, new Guid("0001CEF2-6A6F-4DB7-A932-07F854C66478")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.CommunicationTask.CommunicationEvent),
                new RolePattern(m.CommunicationEvent.WorkItemDescription) { Steps = new IPropertyType[] {m.CommunicationEvent.CommunicationTasksWhereCommunicationEvent} },
                new RolePattern(m.CommunicationEvent.ActualEnd) { Steps = new IPropertyType[] {m.CommunicationEvent.CommunicationTasksWhereCommunicationEvent} },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<CommunicationTask>())
            {
                @this.WorkItem = @this.CommunicationEvent;

                @this.Title = @this.CommunicationEvent.WorkItemDescription;

                // Lifecycle
                if (!@this.ExistDateClosed && @this.CommunicationEvent.ExistActualEnd)
                {
                    @this.DateClosed = @this.Transaction().Now();
                }
            }
        }
    }
}
