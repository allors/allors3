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

    public class FaceToFaceCommunicationRule : Rule
    {
        public FaceToFaceCommunicationRule(MetaPopulation m) : base(m, new Guid("165A1691-F94C-40D2-B183-EFC764582784")) =>
            this.Patterns = new Pattern[]
            {
                m.FaceToFaceCommunication.RolePattern(v => v.Subject),
                m.FaceToFaceCommunication.RolePattern(v => v.ToParty),
                m.Party.RolePattern(v => v.PartyName, v => v.CommunicationEventsWhereToParty, m.FaceToFaceCommunication),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<FaceToFaceCommunication>())
            {
                @this.WorkItemDescription = $"Meeting with {@this.ToParty?.PartyName} about {@this.Subject}";
            }
        }
    }
}
