// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

    public class FaxCommunicationSearchStringRule : Rule
    {
        public FaxCommunicationSearchStringRule(MetaPopulation m) : base(m, new Guid("ca4ea356-a08a-48a6-8fe4-7f86ab9fbbd0")) =>
            this.Patterns = new Pattern[]
        {
            m.CommunicationEvent.RolePattern(v => v.InvolvedParties, m.FaxCommunication),
            m.Party.RolePattern(v => v.DisplayName, v => v.CommunicationEventsWhereInvolvedParty, m.FaxCommunication),
            m.CommunicationEvent.RolePattern(v => v.ContactMechanisms, m.FaxCommunication),
            m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.CommunicationEventsWhereContactMechanism, m.FaxCommunication),
            m.CommunicationEvent.RolePattern(v => v.WorkEfforts, m.FaxCommunication),
            m.WorkEffort.RolePattern(v => v.Name, v => v.CommunicationEventsWhereWorkEffort, m.FaxCommunication),
            m.CommunicationEvent.RolePattern(v => v.EventPurposes, m.FaxCommunication),
            m.CommunicationEvent.RolePattern(v => v.Description, m.FaxCommunication),
            m.CommunicationEvent.RolePattern(v => v.Subject, m.FaxCommunication),
            m.CommunicationEvent.RolePattern(v => v.Owner, m.FaxCommunication),
            m.CommunicationEvent.RolePattern(v => v.Priority, m.FaxCommunication),
            m.WorkItem.RolePattern(v => v.WorkItemDescription, m.FaxCommunication),
            m.FaxCommunication.RolePattern(v => v.FaxNumber),
            m.TelecommunicationsNumber.RolePattern(v => v.DisplayName, v => v.FaxCommunicationsWhereFaxNumber),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<FaxCommunication>())
            {
                @this.DeriveFaxCommunicationSearchString(validation);
            }
        }
    }

    public static class FaxCommunicationSearchStringRuleExtensions
    {
        public static void DeriveFaxCommunicationSearchString(this FaxCommunication @this, IValidation validation)
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
                    @this.FaxNumber?.DisplayName,
                    @this.WorkItemDescription,
                };

            if (array.Any(s => !string.IsNullOrEmpty(s)))
            {
                @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
            else
            {
                @this.RemoveSearchString();
            }
        }
    }
}
