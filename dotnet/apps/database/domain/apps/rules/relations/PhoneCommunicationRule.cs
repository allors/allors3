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

    public class PhoneCommunicationRule : Rule
    {
        public PhoneCommunicationRule(MetaPopulation m) : base(m, new Guid("6C8EB5C8-1E8C-41CF-B127-53C88E200F65")) =>
            this.Patterns = new Pattern[]
            {
                m.PhoneCommunication.RolePattern(v => v.Subject),
                m.PhoneCommunication.RolePattern(v => v.ToParty),
                m.Party.RolePattern(v => v.PartyName, v => v.CommunicationEventsWhereToParty, m.PhoneCommunication),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PhoneCommunication>())
            {
                @this.WorkItemDescription = $"Call to {@this.ToParty?.PartyName} about {@this.Subject}";
            }
        }
    }
}
