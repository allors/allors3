// <copyright file="FakeTransport.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Connection
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
    using Allors.Workspace.Meta;

    /// <summary>
    /// The transport to a <see cref="FakeServer"/>: no wire, the requests and responses pass as
    /// objects, with the System.Text.Json unit converter on both sides as the in-process
    /// transport has.
    /// </summary>
    public sealed class FakeTransport : ITransport
    {
        public FakeTransport(M m)
        {
            this.UnitConvert = new Allors.Protocol.Json.SystemTextJson.UnitConvert();
            this.Server = new FakeServer(m, this.UnitConvert);
        }

        public FakeServer Server { get; }

        public IUnitConvert UnitConvert { get; }

        public IObservable<ServerMessage> ServerMessages => null;

        public Task<PullResponse> PullAsync(PullRequest pullRequest) => Task.FromResult(this.Server.Pull(pullRequest));

        public Task<PullResponse> PullAsync(string name, object args) => throw new NotSupportedException("The fake server has no named pulls.");

        public Task<SyncResponse> SyncAsync(SyncRequest syncRequest) => Task.FromResult(this.Server.Sync(syncRequest));

        public Task<PushResponse> PushAsync(PushRequest pushRequest) => Task.FromResult(this.Server.Push(pushRequest));

        public Task<InvokeResponse> InvokeAsync(InvokeRequest invokeRequest) => Task.FromResult(this.Server.Invoke(invokeRequest));

        public Task<AccessResponse> AccessAsync(AccessRequest accessRequest) => Task.FromResult(this.Server.Access(accessRequest));

        public Task<PermissionResponse> PermissionAsync(PermissionRequest permissionRequest) => Task.FromResult(this.Server.Permission(permissionRequest));
    }
}
