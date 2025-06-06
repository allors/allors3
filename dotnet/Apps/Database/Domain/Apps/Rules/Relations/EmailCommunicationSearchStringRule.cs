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

    public class EmailCommunicationSearchStringRule : Rule
    {
        public EmailCommunicationSearchStringRule(MetaPopulation m) : base(m, new Guid("0777930a-bf7c-4f48-aa02-182ff67b35a7")) =>
            this.Patterns = new Pattern[]
        {
            m.CommunicationEvent.RolePattern(v => v.InvolvedParties, m.EmailCommunication),
            m.Party.RolePattern(v => v.DisplayName, v => v.CommunicationEventsWhereInvolvedParty, m.EmailCommunication),
            m.CommunicationEvent.RolePattern(v => v.ContactMechanisms, m.EmailCommunication),
            m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.CommunicationEventsWhereContactMechanism, m.EmailCommunication),
            m.CommunicationEvent.RolePattern(v => v.WorkEfforts, m.EmailCommunication),
            m.WorkEffort.RolePattern(v => v.Name, v => v.CommunicationEventsWhereWorkEffort, m.EmailCommunication),
            m.CommunicationEvent.RolePattern(v => v.EventPurposes, m.EmailCommunication),
            m.CommunicationEvent.RolePattern(v => v.Description, m.EmailCommunication),
            m.CommunicationEvent.RolePattern(v => v.Subject, m.EmailCommunication),
            m.CommunicationEvent.RolePattern(v => v.Owner, m.EmailCommunication),
            m.CommunicationEvent.RolePattern(v => v.Priority, m.EmailCommunication),
            m.WorkItem.RolePattern(v => v.WorkItemDescription, m.EmailCommunication),
            m.EmailCommunication.RolePattern(v => v.FromEmail),
            m.EmailCommunication.RolePattern(v => v.ToEmail),
            m.EmailAddress.RolePattern(v => v.DisplayName, v => v.EmailCommunicationsWhereFromEmail),
            m.EmailAddress.RolePattern(v => v.DisplayName, v => v.EmailCommunicationsWhereToEmail),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<EmailCommunication>())
            {
                @this.DeriveEmailCommunicationSearchString(validation);
            }
        }
    }

    public static class EmailCommunicationSearchStringRuleExtensions
    {
        public static void DeriveEmailCommunicationSearchString(this EmailCommunication @this, IValidation validation)
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
                    @this.FromEmail?.DisplayName,
                    @this.ToEmail?.DisplayName,
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
