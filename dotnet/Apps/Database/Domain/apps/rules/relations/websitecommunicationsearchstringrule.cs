// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class WebSiteCommunicationSearchStringRule : Rule
    {
        public WebSiteCommunicationSearchStringRule(MetaPopulation m) : base(m, new Guid("95cbc3ad-c88d-48b3-80f6-bd69fda48cba")) =>
            this.Patterns = new Pattern[]
        {
            m.CommunicationEvent.RolePattern(v => v.InvolvedParties, m.WebSiteCommunication),
            m.Party.RolePattern(v => v.DisplayName, v => v.CommunicationEventsWhereInvolvedParty.CommunicationEvent, m.WebSiteCommunication),
            m.CommunicationEvent.RolePattern(v => v.ContactMechanisms, m.WebSiteCommunication),
            m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.CommunicationEventsWhereContactMechanism.CommunicationEvent, m.WebSiteCommunication),
            m.CommunicationEvent.RolePattern(v => v.WorkEfforts, m.WebSiteCommunication),
            m.WorkEffort.RolePattern(v => v.Name, v => v.CommunicationEventsWhereWorkEffort.CommunicationEvent, m.WebSiteCommunication),
            m.CommunicationEvent.RolePattern(v => v.EventPurposes, m.WebSiteCommunication),
            m.CommunicationEvent.RolePattern(v => v.Description, m.WebSiteCommunication),
            m.CommunicationEvent.RolePattern(v => v.Subject, m.WebSiteCommunication),
            m.CommunicationEvent.RolePattern(v => v.Owner, m.WebSiteCommunication),
            m.CommunicationEvent.RolePattern(v => v.Priority, m.WebSiteCommunication),
            m.WorkItem.RolePattern(v => v.WorkItemDescription, m.WebSiteCommunication),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WebSiteCommunication>())
            {
                var array = new string[] {
                    @this.ExistInvolvedParties ? string.Join(" ", @this.InvolvedParties?.Select(v => v.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistContactMechanisms ? string.Join(" ", @this.ContactMechanisms?.Select(v => v.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistWorkEfforts ? string.Join(" ", @this.WorkEfforts?.Select(v => v.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistEventPurposes ? string.Join(" ", @this.EventPurposes?.Select(v => v.Name ?? string.Empty).ToArray()) : null,
                    @this.Description,
                    @this.Subject,
                    @this.Owner?.DisplayName,
                    @this.Priority?.Name,
                    @this.WorkItemDescription,
                };

                if (array.Any(s => !string.IsNullOrEmpty(s)))
                {
                    @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
                }
            }
        }
    }
}
