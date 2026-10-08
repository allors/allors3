// <copyright file="DatabaseController.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using Allors.Protocol.Json.Api.Invoke;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.Sync;
    using Tracing;

    public static class SinkExtensions
    {
        public static InvokeEvent OnInvoke(this ISink @this, ITransaction transaction, InvokeRequest invokeRequest) => @this != null ? new InvokeEvent(transaction) { InvokeRequest = invokeRequest } : null;

        public static PullEvent OnPull(this ISink @this, ITransaction transaction, PullRequest pullRequest) => @this != null ? new PullEvent(transaction) { PullRequest = pullRequest } : null;

        public static PushEvent OnPush(this ISink @this, ITransaction transaction, PushRequest pushRequest) => @this != null ? new PushEvent(transaction) { PushRequest = pushRequest } : null;

        public static SyncEvent OnSync(this ISink @this, ITransaction transaction, SyncRequest syncRequest) => @this != null ? new SyncEvent(transaction) { SyncRequest = syncRequest } : null;
    }
}
