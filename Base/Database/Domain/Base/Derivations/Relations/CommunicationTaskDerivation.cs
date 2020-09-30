// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class CommunicationTaskDerivation : DomainDerivation
    {
        public CommunicationTaskDerivation(M m) : base(m, new Guid("0001CEF2-6A6F-4DB7-A932-07F854C66478")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(M.CommunicationTask.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var communicationTask in matches.Cast<CommunicationTask>())
            {
                communicationTask.WorkItem = communicationTask.CommunicationEvent;

                communicationTask.Title = communicationTask.CommunicationEvent.WorkItemDescription;

                // Lifecycle
                if (!communicationTask.ExistDateClosed && communicationTask.CommunicationEvent.ExistActualEnd)
                {
                    communicationTask.DateClosed = communicationTask.Session().Now();
                }

                // Assignments
                var participants = communicationTask.ExistDateClosed ? Array.Empty<User>() : new[] { communicationTask.CommunicationEvent.FromParty as User };
                communicationTask.AssignParticipants(participants);
            }
        }
    }
}
