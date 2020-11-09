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

    public class CommunicationEventDerivation : DomainDerivation
    {
        public CommunicationEventDerivation(M m) : base(m, new Guid("6ABC8FDF-B4BC-40A2-9396-04292779E5F5")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.CommunicationEvent.Owner),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<CommunicationEvent>())
            {
                if (!@this.ExistOwner && @this.Strategy.Session.State().User is Person owner)
                {
                    @this.Owner = owner;
                }

                if (@this.ExistScheduledStart && @this.ExistScheduledEnd && @this.ScheduledEnd < @this.ScheduledStart)
                {
                    validation.AddError($"Scheduled end date before scheduled start date: {@this}");
                }

                if (@this.ExistActualStart && @this.ExistActualEnd && @this.ActualEnd < @this.ActualStart)
                {
                    validation.AddError($"Actual end date before actual start date: {@this}");
                }

                if (!@this.ExistCommunicationEventState)
                {
                    if (!@this.ExistActualStart || (@this.ExistActualStart && @this.ActualStart > @this.Strategy.Session.Now()))
                    {
                        @this.CommunicationEventState = new CommunicationEventStates(@this.Strategy.Session).Scheduled;
                    }

                    if (@this.ExistActualStart && @this.ActualStart <= @this.Strategy.Session.Now() &&
                        ((@this.ExistActualEnd && @this.ActualEnd > @this.Strategy.Session.Now()) || !@this.ExistActualEnd))
                    {
                        @this.CommunicationEventState = new CommunicationEventStates(@this.Strategy.Session).InProgress;
                    }

                    if (@this.ExistActualEnd && @this.ActualEnd <= @this.Strategy.Session.Now())
                    {
                        @this.CommunicationEventState = new CommunicationEventStates(@this.Strategy.Session).Completed;
                    }
                }

                if (!@this.ExistInitialScheduledStart && @this.ExistScheduledStart)
                {
                    @this.InitialScheduledStart = @this.ScheduledStart;
                }

                if (!@this.ExistInitialScheduledEnd && @this.ExistScheduledEnd)
                {
                    @this.InitialScheduledEnd = @this.ScheduledEnd;
                }

                var openCommunicationTasks = @this.TasksWhereWorkItem
                    .OfType<CommunicationTask>()
                    .Where(v => !v.ExistDateClosed)
                    .ToArray();

                if (@this.ExistActualEnd)
                {
                    if (openCommunicationTasks.Length > 0)
                    {
                        openCommunicationTasks.First().DateClosed = @this.Strategy.Session.Now();
                    }
                }
                else
                {
                    if (openCommunicationTasks.Length == 0)
                    {
                        new CommunicationTaskBuilder(@this.Strategy.Session).WithCommunicationEvent(@this).Build();
                    }
                }

                @this.AddSecurityToken(new SecurityTokens(cycle.Session).DefaultSecurityToken);
                @this.AddSecurityToken(@this.Owner?.OwnerSecurityToken);

                var now = @this.Strategy.Session.Now();

                var parties = new[] { @this.FromParty, @this.ToParty, @this.Owner }.Distinct().ToArray();

                var organisation = parties.OfType<Person>()
                    .SelectMany(v => v.OrganisationContactRelationshipsWhereContact)
                    .Where(v => v.FromDate <= now && (!v.ExistThroughDate || v.ThroughDate >= now))
                    .Select(v => v.Organisation);

                @this.InvolvedParties = parties.Union(organisation).ToArray();
            }
        }
    }
}
