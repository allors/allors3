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
    using Derivations.Rules;
    using Meta;

    public class LetterCorrespondenceRule : Rule
    {
        public LetterCorrespondenceRule(MetaPopulation m) : base(m, new Guid("7C1C3F73-2FE2-4713-8006-682E979E38CE")) =>
            this.Patterns = new Pattern[]
            {
                m.LetterCorrespondence.RolePattern(v => v.Subject),
                m.LetterCorrespondence.RolePattern(v => v.ToParty),
                m.Party.RolePattern(v => v.DisplayName, v => v.CommunicationEventsWhereToParty, m.LetterCorrespondence),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<LetterCorrespondence>())
            {
                @this.WorkItemDescription = $"Letter to {@this.ToParty?.DisplayName} about {@this.Subject}";
            }
        }
    }
}
