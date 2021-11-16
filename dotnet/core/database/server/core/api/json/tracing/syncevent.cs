// <copyright file="DatabaseController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Text;
    using Allors.Protocol.Json.Api.Sync;
    using Tracing;

    public class SyncEvent : Event, IDisposable
    {
        public SyncEvent(ISink sink, ITransaction transaction) : base(transaction) => this.Sink = sink;

        public void Dispose() => this.Sink.OnAfter(this);

        public ISink Sink { get; }

        public SyncRequest SyncRequest { get; set; }

        protected override void ToString(StringBuilder builder) => builder
            .Append(this.SyncRequest.x);
    }
}
