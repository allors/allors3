// <copyright file="LocalTransport.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection.Local
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Allors.Database;
    using Allors.Database.Protocol.Json;
    using Allors.Database.Security;
    using Allors.Database.Services;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Invoke;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.Security;
    using Allors.Protocol.Json.Api.Sync;
    using Allors.Protocol.Json.SystemTextJson;

    /// <summary>
    /// The in-process transport. It sends the same requests and receives the same responses as
    /// the HTTP transports, served by the server's <see cref="Api"/> on a transaction of the
    /// given database, as the given user, without a wire. A call runs on the calling thread and
    /// has completed when its task is returned.
    /// </summary>
    public class LocalTransport : ITransport
    {
        public LocalTransport(IDatabase database, long userId, string workspaceName)
        {
            this.Database = database;
            this.UserId = userId;
            this.WorkspaceName = workspaceName;
            this.UnitConvert = new UnitConvert();
        }

        public IDatabase Database { get; }

        public long UserId { get; }

        /// <summary>
        /// The name of the workspace the <see cref="Api"/> serves; over HTTP the server maps it
        /// from the host.
        /// </summary>
        public string WorkspaceName { get; }

        public IUnitConvert UnitConvert { get; }

        public IObservable<ServerMessage> ServerMessages => null;

        public Task<PullResponse> PullAsync(string name, object args) =>
            throw new NotSupportedException($"The in-process transport cannot pull '{name}': a named pull is a route of the server and has no in-process equivalent. Pull with Pull objects, or call a Procedure.");

        public Task<PullResponse> PullAsync(PullRequest pullRequest) => this.Execute(api => api.Pull(pullRequest));

        public Task<SyncResponse> SyncAsync(SyncRequest syncRequest) => this.Execute(api => api.Sync(syncRequest));

        public Task<PushResponse> PushAsync(PushRequest pushRequest) => this.Execute(api => api.Push(pushRequest));

        public Task<InvokeResponse> InvokeAsync(InvokeRequest invokeRequest) => this.Execute(api => api.Invoke(invokeRequest));

        public Task<AccessResponse> AccessAsync(AccessRequest accessRequest) => this.Execute(api => api.Access(accessRequest));

        public Task<PermissionResponse> PermissionAsync(PermissionRequest permissionRequest) => this.Execute(api => api.Permission(permissionRequest));

        private Task<TResponse> Execute<TResponse>(Func<Api, TResponse> call)
        {
            // As the server controllers do: one transaction per request, the user set before the
            // Api is created, and the transaction only disposed, because the builders commit and
            // roll back themselves.
            using var transaction = this.Database.CreateTransaction();
            var user = (IUser)transaction.Instantiate(this.UserId);
            transaction.Services.Get<IUserService>().User = user;
            var api = new Api(transaction, this.WorkspaceName, CancellationToken.None);
            return Task.FromResult(call(api));
        }
    }
}
