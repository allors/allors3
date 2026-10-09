// <copyright file="DatabaseController.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System.Text;
    using Allors.Protocol.Json.Api.Sync;

    public class SyncEvent : Event
    {
        public SyncEvent(ITransaction transaction) : base(transaction) { }

        public SyncRequest SyncRequest { get; set; }

        public SyncResponse SyncResponse { get; set; }

        protected override void ToString(StringBuilder builder) => builder
            .Append(this.SyncRequest.x);
    }
}
