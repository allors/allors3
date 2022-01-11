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

    public class FaceToFaceCommunicationSearchStringRule : Rule
    {
        public FaceToFaceCommunicationSearchStringRule(MetaPopulation m) : base(m, new Guid("d800e7ba-763b-46ad-a1af-d6547ee8f158")) =>
            this.Patterns = new Pattern[]
        {
            m.CommunicationEvent.RolePattern(v => v.InvolvedParties, m.FaceToFaceCommunication),
            m.Party.RolePattern(v => v.DisplayName, v => v.CommunicationEventsWhereInvolvedParty, m.FaceToFaceCommunication),
            m.CommunicationEvent.RolePattern(v => v.ContactMechanisms, m.FaceToFaceCommunication),
            m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.CommunicationEventsWhereContactMechanism, m.FaceToFaceCommunication),
            m.CommunicationEvent.RolePattern(v => v.WorkEfforts, m.FaceToFaceCommunication),
            m.WorkEffort.RolePattern(v => v.Name, v => v.CommunicationEventsWhereWorkEffort, m.FaceToFaceCommunication),
            m.CommunicationEvent.RolePattern(v => v.EventPurposes, m.FaceToFaceCommunication),
            m.CommunicationEvent.RolePattern(v => v.Description, m.FaceToFaceCommunication),
            m.CommunicationEvent.RolePattern(v => v.Subject, m.FaceToFaceCommunication),
            m.CommunicationEvent.RolePattern(v => v.Owner, m.FaceToFaceCommunication),
            m.CommunicationEvent.RolePattern(v => v.Priority, m.FaceToFaceCommunication),
            m.WorkItem.RolePattern(v => v.WorkItemDescription, m.FaceToFaceCommunication),
            m.FaceToFaceCommunication.RolePattern(v => v.Location),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<FaceToFaceCommunication>())
            {
                @this.DeriveFaceToFaceCommunicationSearchString(validation);
            }
        }
    }

    public static class FaceToFaceCommunicationSearchStringRuleExtensions
    {
        public static void DeriveFaceToFaceCommunicationSearchString(this FaceToFaceCommunication @this, IValidation validation)
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
                    @this.Location,
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
