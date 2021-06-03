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
    using Derivations.Rules;

    public class CommunicationEventRule : Rule
    {
        public CommunicationEventRule(MetaPopulation m) : base(m, new Guid("6ABC8FDF-B4BC-40A2-9396-04292779E5F5")) =>
            this.Patterns = new Pattern[]
            {
                m.CommunicationEvent.RolePattern(v => v.Owner),
                m.CommunicationEvent.RolePattern(v => v.ActualStart),
                m.CommunicationEvent.RolePattern(v => v.ActualEnd),
                m.CommunicationEvent.RolePattern(v => v.ScheduledStart),
                m.CommunicationEvent.RolePattern(v => v.ScheduledEnd),
                m.CommunicationEvent.RolePattern(v => v.InitialScheduledStart),
                m.CommunicationEvent.RolePattern(v => v.InitialScheduledEnd),
                m.CommunicationEvent.RolePattern(v => v.FromParty),
                m.CommunicationEvent.RolePattern(v => v.ToParty),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<CommunicationEvent>())
            {
                if (@this.ExistScheduledStart && @this.ExistScheduledEnd && @this.ScheduledEnd < @this.ScheduledStart)
                {
                    // TODO: Move text to Resources
                    validation.AddError(@this, this.M.CommunicationEvent.ScheduledEnd, $"Scheduled end date before scheduled start date");
                }

                if (@this.ExistActualStart && @this.ExistActualEnd && @this.ActualEnd < @this.ActualStart)
                {
                    // TODO: Move text to Resources
                    validation.AddError(@this, this.M.CommunicationEvent.ActualEnd, $"Actual end date before actual start date");
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

                var openCommunicationTasks = @this.CommunicationTasksWhereCommunicationEvent
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
