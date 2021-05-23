// <copyright file="CommunicationEvents.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class CommunicationEvents
    {
        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.CommunicationEventState);

        protected override void AppsSecure(Security config)
        {
            ObjectState scheduled = new CommunicationEventStates(this.Transaction).Scheduled;
            ObjectState cancelled = new CommunicationEventStates(this.Transaction).Cancelled;
            ObjectState closed = new CommunicationEventStates(this.Transaction).Completed;

            var reopenId = this.M.CommunicationEvent.Reopen;
            var closeId = this.M.CommunicationEvent.Close;
            var cancelId = this.M.CommunicationEvent.Cancel;

            config.Deny(this.ObjectType, scheduled, reopenId);
            config.Deny(this.ObjectType, closed, closeId, cancelId);
            config.Deny(this.ObjectType, cancelled, cancelId);

            config.Deny(this.ObjectType, closed, Operations.Write);
            config.Deny(this.ObjectType, cancelled, Operations.Write);
        }
    }
}
