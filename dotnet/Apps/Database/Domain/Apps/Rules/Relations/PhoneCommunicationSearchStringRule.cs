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

    public class PhoneCommunicationSearchStringRule : Rule
    {
        public PhoneCommunicationSearchStringRule(MetaPopulation m) : base(m, new Guid("09eacde2-b6f6-4163-87f6-50c221312a03")) =>
            this.Patterns = new Pattern[]
        {
            m.CommunicationEvent.RolePattern(v => v.InvolvedParties, m.PhoneCommunication),
            m.Party.RolePattern(v => v.DisplayName, v => v.CommunicationEventsWhereInvolvedParty, m.PhoneCommunication),
            m.CommunicationEvent.RolePattern(v => v.ContactMechanisms, m.PhoneCommunication),
            m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.CommunicationEventsWhereContactMechanism, m.PhoneCommunication),
            m.CommunicationEvent.RolePattern(v => v.WorkEfforts, m.PhoneCommunication),
            m.WorkEffort.RolePattern(v => v.Name, v => v.CommunicationEventsWhereWorkEffort, m.PhoneCommunication),
            m.CommunicationEvent.RolePattern(v => v.EventPurposes, m.PhoneCommunication),
            m.CommunicationEvent.RolePattern(v => v.Description, m.PhoneCommunication),
            m.CommunicationEvent.RolePattern(v => v.Subject, m.PhoneCommunication),
            m.CommunicationEvent.RolePattern(v => v.Owner, m.PhoneCommunication),
            m.CommunicationEvent.RolePattern(v => v.Priority, m.PhoneCommunication),
            m.WorkItem.RolePattern(v => v.WorkItemDescription, m.PhoneCommunication),
            m.PhoneCommunication.RolePattern(v => v.PhoneNumber),
            m.TelecommunicationsNumber.RolePattern(v => v.DisplayName, v => v.PhoneCommunicationsWherePhoneNumber),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PhoneCommunication>())
            {
                @this.DerivePhoneCommunicationSearchString(validation);
            }
        }
    }

    public static class PhoneCommunicationSearchStringRuleExtensions
    {
        public static void DerivePhoneCommunicationSearchString(this PhoneCommunication @this, IValidation validation)
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
                    @this.PhoneNumber?.DisplayName,
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
