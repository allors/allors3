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

    public class FaceToFaceCommunicationRule : Rule
    {
        public FaceToFaceCommunicationRule(M m) : base(m, new Guid("165A1691-F94C-40D2-B183-EFC764582784")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.FaceToFaceCommunication, m.FaceToFaceCommunication.Subject),
                new RolePattern(m.FaceToFaceCommunication, m.FaceToFaceCommunication.ToParty),
                new RolePattern(m.Party, m.Party.PartyName) { Steps = new IPropertyType[] { m.Party.CommunicationEventsWhereToParty}, OfType = m.FaceToFaceCommunication.Class },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<FaceToFaceCommunication>())
            {
                @this.WorkItemDescription = $"Meeting with {@this.ToParty?.PartyName} about {@this.Subject}";
            }
        }
    }
}
