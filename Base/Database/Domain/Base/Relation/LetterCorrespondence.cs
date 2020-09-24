// <copyright file="LetterCorrespondence.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using Allors.Meta;

    public partial class LetterCorrespondence
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.LetterCorrespondence, this.M.LetterCorrespondence.CommunicationEventState),
        };

        //public void BaseOnDerive(ObjectOnDerive method) => this.WorkItemDescription = $"Letter to {this.ToParty.PartyName} about {this.Subject}";
    }
}
