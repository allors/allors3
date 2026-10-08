// <copyright file="CountingTransport.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System;
    using System.Threading.Tasks;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Invoke;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.Security;
    using Allors.Protocol.Json.Api.Sync;
    using Allors.Workspace.Connection;

    /// <summary>
    /// A transport around another that counts the calls and keeps the last request of each
    /// kind, so that a test can say what a pull did and did not ask the server.
    /// </summary>
    public sealed class CountingTransport : ITransport
    {
        private readonly ITransport inner;

        public CountingTransport(ITransport inner) => this.inner = inner;

        public int PullCount { get; private set; }

        public int SyncCount { get; private set; }

        public int PushCount { get; private set; }

        public int InvokeCount { get; private set; }

        public int AccessCount { get; private set; }

        public int PermissionCount { get; private set; }

        public SyncRequest LastSyncRequest { get; private set; }

        public AccessRequest LastAccessRequest { get; private set; }

        public PermissionRequest LastPermissionRequest { get; private set; }

        public IUnitConvert UnitConvert => this.inner.UnitConvert;

        public IObservable<ServerMessage> ServerMessages => this.inner.ServerMessages;

        public Task<PullResponse> PullAsync(PullRequest pullRequest)
        {
            this.PullCount++;
            return this.inner.PullAsync(pullRequest);
        }

        public Task<PullResponse> PullAsync(string name, object args)
        {
            this.PullCount++;
            return this.inner.PullAsync(name, args);
        }

        public Task<SyncResponse> SyncAsync(SyncRequest syncRequest)
        {
            this.SyncCount++;
            this.LastSyncRequest = syncRequest;
            return this.inner.SyncAsync(syncRequest);
        }

        public Task<PushResponse> PushAsync(PushRequest pushRequest)
        {
            this.PushCount++;
            return this.inner.PushAsync(pushRequest);
        }

        public Task<InvokeResponse> InvokeAsync(InvokeRequest invokeRequest)
        {
            this.InvokeCount++;
            return this.inner.InvokeAsync(invokeRequest);
        }

        public Task<AccessResponse> AccessAsync(AccessRequest accessRequest)
        {
            this.AccessCount++;
            this.LastAccessRequest = accessRequest;
            return this.inner.AccessAsync(accessRequest);
        }

        public Task<PermissionResponse> PermissionAsync(PermissionRequest permissionRequest)
        {
            this.PermissionCount++;
            this.LastPermissionRequest = permissionRequest;
            return this.inner.PermissionAsync(permissionRequest);
        }
    }
}
