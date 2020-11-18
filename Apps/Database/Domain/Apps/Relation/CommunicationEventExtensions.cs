// <copyright file="CommunicationEventExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public static partial class CommunicationEventExtensions
    {
        public static void AppsDelete(this CommunicationEvent @this, DeletableDelete method)
        {
            foreach (Task task in @this.TasksWhereWorkItem)
            {
                task.Delete();
            }
        }

        public static void AppsClose(this CommunicationEvent @this, CommunicationEventClose method) => @this.CommunicationEventState = new CommunicationEventStates(@this.Strategy.Session).Completed;

        public static void AppsReopen(this CommunicationEvent @this, CommunicationEventReopen method) => @this.CommunicationEventState = new CommunicationEventStates(@this.Strategy.Session).Scheduled;

        public static void AppsCancel(this CommunicationEvent @this, CommunicationEventCancel method) => @this.CommunicationEventState = new CommunicationEventStates(@this.Strategy.Session).Cancelled;
    }
}
