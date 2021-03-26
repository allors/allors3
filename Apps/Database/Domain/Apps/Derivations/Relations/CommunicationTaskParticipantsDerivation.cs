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

    public class CommunicationTaskParticipantsDerivation : DomainDerivation
    {
        public CommunicationTaskParticipantsDerivation(M m) : base(m, new Guid("888c676f-3a56-4a99-8da5-70c3b4c7f9f9")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.CommunicationTask.DateClosed),
                new RolePattern(m.CommunicationEvent.FromParty) { Steps = new IPropertyType[] {m.CommunicationEvent.CommunicationTasksWhereCommunicationEvent} },
                new RolePattern(m.CommunicationEvent.ToParty) { Steps = new IPropertyType[] {m.CommunicationEvent.CommunicationTasksWhereCommunicationEvent} },
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
