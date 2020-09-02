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

    public class CommunicationEventDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("6ABC8FDF-B4BC-40A2-9396-04292779E5F5");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.CommunicationEvent.Interface),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var communicationEventExtension in matches.Cast<CommunicationEvent>())
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

                communicationEventExtension.AddSecurityToken(new SecurityTokens(cycle.Session).DefaultSecurityToken);
                communicationEventExtension.AddSecurityToken(communicationEventExtension.Owner?.OwnerSecurityToken);

                var now = communicationEventExtension.Strategy.Session.Now();

                var parties = new[] { communicationEventExtension.FromParty, communicationEventExtension.ToParty, communicationEventExtension.Owner }.Distinct().ToArray();

                var organisation = parties.OfType<Person>()
                    .SelectMany(v => v.OrganisationContactRelationshipsWhereContact)
                    .Where(v => v.FromDate <= now && (!v.ExistThroughDate || v.ThroughDate >= now))
                    .Select(v => v.Organisation);

                communicationEventExtension.InvolvedParties = parties.Union(organisation).ToArray();
            }
        }
    }
}
