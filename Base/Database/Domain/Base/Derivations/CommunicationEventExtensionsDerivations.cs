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
    using Resources;

    public static partial class DabaseExtensions
    {
        public class CommunicationEventExtensionsCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdCommunicationEventExtensions = changeSet.Created.Select(session.Instantiate).OfType<CommunicationEvent>();

                foreach (var communicationEventExtension in createdCommunicationEventExtensions)
                {

                    if (!communicationEventExtension.ExistOwner && communicationEventExtension.Strategy.Session.GetUser() is Person owner)
                    {
                        communicationEventExtension.Owner = owner;
                    }

                    if (communicationEventExtension.ExistScheduledStart && communicationEventExtension.ExistScheduledEnd && communicationEventExtension.ScheduledEnd < communicationEventExtension.ScheduledStart)
                    {
                        validation.AddError($"Scheduled end date before scheduled start date: {communicationEventExtension}");
                    }

                    if (communicationEventExtension.ExistActualStart && communicationEventExtension.ExistActualEnd && communicationEventExtension.ActualEnd < communicationEventExtension.ActualStart)
                    {
                        validation.AddError($"Actual end date before actual start date: {communicationEventExtension}");
                    }

                    if (!communicationEventExtension.ExistCommunicationEventState)
                    {
                        if (!communicationEventExtension.ExistActualStart || (communicationEventExtension.ExistActualStart && communicationEventExtension.ActualStart > communicationEventExtension.Strategy.Session.Now()))
                        {
                            communicationEventExtension.CommunicationEventState = new CommunicationEventStates(communicationEventExtension.Strategy.Session).Scheduled;
                        }

                        if (communicationEventExtension.ExistActualStart && communicationEventExtension.ActualStart <= communicationEventExtension.Strategy.Session.Now() &&
                            ((communicationEventExtension.ExistActualEnd && communicationEventExtension.ActualEnd > communicationEventExtension.Strategy.Session.Now()) || !communicationEventExtension.ExistActualEnd))
                        {
                            communicationEventExtension.CommunicationEventState = new CommunicationEventStates(communicationEventExtension.Strategy.Session).InProgress;
                        }

                        if (communicationEventExtension.ExistActualEnd && communicationEventExtension.ActualEnd <= communicationEventExtension.Strategy.Session.Now())
                        {
                            communicationEventExtension.CommunicationEventState = new CommunicationEventStates(communicationEventExtension.Strategy.Session).Completed;
                        }
                    }

                    if (!communicationEventExtension.ExistInitialScheduledStart && communicationEventExtension.ExistScheduledStart)
                    {
                        communicationEventExtension.InitialScheduledStart = communicationEventExtension.ScheduledStart;
                    }

                    if (!communicationEventExtension.ExistInitialScheduledEnd && communicationEventExtension.ExistScheduledEnd)
                    {
                        communicationEventExtension.InitialScheduledEnd = communicationEventExtension.ScheduledEnd;
                    }

                    var openCommunicationTasks = communicationEventExtension.TasksWhereWorkItem
                        .OfType<CommunicationTask>()
                        .Where(v => !v.ExistDateClosed)
                        .ToArray();

                    if (communicationEventExtension.ExistActualEnd)
                    {
                        if (openCommunicationTasks.Length > 0)
                        {
                            openCommunicationTasks.First().DateClosed = communicationEventExtension.Strategy.Session.Now();
                        }
                    }
                    else
                    {
                        if (openCommunicationTasks.Length == 0)
                        {
                            new CommunicationTaskBuilder(communicationEventExtension.Strategy.Session).WithCommunicationEvent(communicationEventExtension).Build();
                        }
                    }
                }

            }
        }

        public static void CommunicationEventExtensionsRegisterDerivations(this IDatabase communicationEventExtension)
        {
            communicationEventExtension.DomainDerivationById[new Guid("5383cacf-553c-4f6b-86bc-d52bf5f6e420")] = new CommunicationEventExtensionsCreationDerivation();
        }
    }
}
