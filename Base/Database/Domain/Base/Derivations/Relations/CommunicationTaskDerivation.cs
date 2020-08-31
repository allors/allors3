// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class CommunicationTaskCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdCommunicationTasks = changeSet.Created.Select(v=>v.GetObject()).OfType<CommunicationTask>();

                foreach(var communicationTask in createdCommunicationTasks)
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

        public static void CommunicationTaskRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("9a5c082a-3141-44b2-8bc2-8d43ef3da9f6")] = new CommunicationTaskCreationDerivation();
        }
    }
}
