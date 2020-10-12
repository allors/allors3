// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class FaceToFaceCommunicationDerivation : DomainDerivation
    {
        public FaceToFaceCommunicationDerivation(M m) : base(m, new Guid("165A1691-F94C-40D2-B183-EFC764582784")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.FaceToFaceCommunication.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var faceToFaceCommunication in matches.Cast<FaceToFaceCommunication>())
            {
                faceToFaceCommunication.WorkItemDescription = $"Meeting with {faceToFaceCommunication.ToParty.PartyName} about {faceToFaceCommunication.Subject}";
            }
        }
    }
}
