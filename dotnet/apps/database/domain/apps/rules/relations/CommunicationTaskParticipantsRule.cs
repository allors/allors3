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

    public class CommunicationTaskParticipantsRule : Rule
    {
        public CommunicationTaskParticipantsRule(MetaPopulation m) : base(m, new Guid("888c676f-3a56-4a99-8da5-70c3b4c7f9f9")) =>
            this.Patterns = new Pattern[]
            {
                m.CommunicationTask.RolePattern(v => v.DateClosed),
                m.CommunicationEvent.RolePattern(v => v.FromParty, v => v.CommunicationTasksWhereCommunicationEvent),
                m.CommunicationEvent.RolePattern(v => v.ToParty, v => v.CommunicationTasksWhereCommunicationEvent),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<CommunicationTask>())
            {
                var assigned = new List<User>();
                if (@this.CommunicationEvent.ExistFromParty && @this.CommunicationEvent.FromParty is User userFrom && userFrom.ExistUserProfile)
                {
                    assigned.Add(userFrom);
                }

                if (@this.CommunicationEvent.ExistToParty && @this.CommunicationEvent.ToParty is User userTo && userTo.ExistUserProfile)
                {
                    assigned.Add(userTo);
                }

                var participants = @this.ExistDateClosed ? Array.Empty<User>() : assigned.ToArray();
                @this.AssignParticipants(participants);
            }
        }
    }
}
