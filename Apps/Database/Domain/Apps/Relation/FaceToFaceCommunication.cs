// <copyright file="FaceToFaceCommunication.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public partial class FaceToFaceCommunication
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.FaceToFaceCommunication, this.M.FaceToFaceCommunication.CommunicationEventState),
        };

        //public void AppsOnDerive(ObjectOnDerive method) => this.WorkItemDescription = $"Meeting with {this.ToParty.PartyName} about {this.Subject}";
    }
}
