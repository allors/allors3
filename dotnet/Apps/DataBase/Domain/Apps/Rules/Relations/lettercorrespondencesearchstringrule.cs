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

    public class LetterCorrespondenceSearchStringRule : Rule
    {
        public LetterCorrespondenceSearchStringRule(MetaPopulation m) : base(m, new Guid("b62b2eb8-fcc5-487b-9c70-67a2fe4fa31f")) =>
            this.Patterns = new Pattern[]
        {
            m.CommunicationEvent.RolePattern(v => v.InvolvedParties, m.LetterCorrespondence),
            m.Party.RolePattern(v => v.DisplayName, v => v.CommunicationEventsWhereInvolvedParty, m.LetterCorrespondence),
            m.CommunicationEvent.RolePattern(v => v.ContactMechanisms, m.LetterCorrespondence),
            m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.CommunicationEventsWhereContactMechanism, m.LetterCorrespondence),
            m.CommunicationEvent.RolePattern(v => v.WorkEfforts, m.LetterCorrespondence),
            m.WorkEffort.RolePattern(v => v.Name, v => v.CommunicationEventsWhereWorkEffort, m.LetterCorrespondence),
            m.CommunicationEvent.RolePattern(v => v.EventPurposes, m.LetterCorrespondence),
            m.CommunicationEvent.RolePattern(v => v.Description, m.LetterCorrespondence),
            m.CommunicationEvent.RolePattern(v => v.Subject, m.LetterCorrespondence),
            m.CommunicationEvent.RolePattern(v => v.Owner, m.LetterCorrespondence),
            m.CommunicationEvent.RolePattern(v => v.Priority, m.LetterCorrespondence),
            m.WorkItem.RolePattern(v => v.WorkItemDescription, m.LetterCorrespondence),
            m.LetterCorrespondence.RolePattern(v => v.PostalAddress),
            m.PostalAddress.RolePattern(v => v.DisplayName, v => v.LetterCorrespondencesWherePostalAddress),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<LetterCorrespondence>())
            {
                @this.DeriveLetterCorrespondenceSearchString(validation);
            }
        }
    }

    public static class LetterCorrespondenceSearchStringRuleExtensions
    {
        public static void DeriveLetterCorrespondenceSearchString(this LetterCorrespondence @this, IValidation validation)
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
                    @this.PostalAddress?.DisplayName,
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
