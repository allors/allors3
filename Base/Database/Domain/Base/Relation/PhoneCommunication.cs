// <copyright file="PhoneCommunication.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public partial class PhoneCommunication
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.PhoneCommunication, this.M.PhoneCommunication.CommunicationEventState),
        };

        //public void BaseOnDerive(ObjectOnDerive method) => this.WorkItemDescription = $"Call to {this.ToParty?.PartyName} about {this.Subject}";
    }
}
