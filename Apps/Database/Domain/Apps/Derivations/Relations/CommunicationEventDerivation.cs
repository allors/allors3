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

    public class CommunicationEventDerivation : DomainDerivation
    {
        public CommunicationEventDerivation(M m) : base(m, new Guid("6ABC8FDF-B4BC-40A2-9396-04292779E5F5")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.CommunicationEvent, m.CommunicationEvent.Owner),
                new RolePattern(m.CommunicationEvent, m.CommunicationEvent.ActualStart),
                new RolePattern(m.CommunicationEvent, m.CommunicationEvent.ActualEnd),
                new RolePattern(m.CommunicationEvent, m.CommunicationEvent.ScheduledStart),
                new RolePattern(m.CommunicationEvent, m.CommunicationEvent.ScheduledEnd),
                new RolePattern(m.CommunicationEvent, m.CommunicationEvent.InitialScheduledStart),
                new RolePattern(m.CommunicationEvent, m.CommunicationEvent.InitialScheduledEnd),
                new RolePattern(m.CommunicationEvent, m.CommunicationEvent.FromParty),
                new RolePattern(m.CommunicationEvent, m.CommunicationEvent.ToParty),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<CommunicationEvent>())
            {
                if (@this.ExistScheduledStart && @this.ExistScheduledEnd && @this.ScheduledEnd < @this.ScheduledStart)
                {
                    validation.AddError($"Scheduled end date before scheduled start date: {@this}");
                }

                if (@this.ExistActualStart && @this.ExistActualEnd && @this.ActualEnd < @this.ActualStart)
                {
                    validation.AddError($"Actual end date before actual start date: {@this}");
                }

                //TODO: Begin Run Asynchronously in the Background
                if (!@this.ExistActualStart || (@this.ActualStart > @this.Strategy.Transaction.Now()))
                {
                    @this.CommunicationEventState = new CommunicationEventStates(@this.Strategy.Transaction).Scheduled;
                }

                if (@this.ExistActualStart && @this.ActualStart <= @this.Strategy.Transaction.Now() &&
                    ((@this.ExistActualEnd && @this.ActualEnd > @this.Strategy.Transaction.Now()) || !@this.ExistActualEnd))
                {
                    @this.CommunicationEventState = new CommunicationEventStates(@this.Strategy.Transaction).InProgress;
                }

                if (@this.ExistActualEnd && @this.ActualEnd <= @this.Strategy.Transaction.Now())
                {
                    @this.CommunicationEventState = new CommunicationEventStates(@this.Strategy.Transaction).Completed;
                }

                // End Run Asynchronously in the Background
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

                if (!@this.ExistActualEnd && openCommunicationTasks.Length == 0)
                {
                    new CommunicationTaskBuilder(@this.Strategy.Transaction).WithCommunicationEvent(@this).Build();
                }

                var parties = new[] { @this.FromParty, @this.ToParty, @this.Owner }.Distinct().ToArray();

                var organisation = parties.OfType<Person>()
                    .SelectMany(v => v.OrganisationContactRelationshipsWhereContact)
                    .Where(v => (@this.ExistScheduledStart && v.FromDate <= @this.ScheduledStart && (!v.ExistThroughDate || v.ThroughDate >= @this.ScheduledEnd))
                                || (@this.ExistActualStart && v.FromDate <= @this.ActualStart && (!v.ExistThroughDate || v.ThroughDate >= @this.ActualEnd)))
                    .Select(v => v.Organisation);

                @this.InvolvedParties = parties.Union(organisation).ToArray();
            }
        }
    }
}
