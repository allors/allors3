// <copyright file="DatabaseConnection.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
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
    using Ranges;

    /// <summary>
    /// The in-process connection. It sends the same requests and receives the same responses as
    /// the HTTP connections, served by the server's <see cref="Api"/> on a transaction of the
    /// given database, as the given user, without a wire. A call runs on the calling thread and
    /// has completed when its task is returned; use a connection from one thread at a time.
    /// </summary>
    public class DatabaseConnection : Remote.DatabaseConnection
    {
        public DatabaseConnection(Remote.Configuration configuration, Func<IWorkspaceServices> servicesBuilder, IDatabase database, long userId, IdGenerator idGenerator, IRanges<long> ranges)
            : base(configuration, idGenerator, servicesBuilder, ranges)
        {
            this.Database = database;
            this.UserId = userId;
            this.UnitConvert = new UnitConvert();
        }

        public IDatabase Database { get; }

        public long UserId { get; }

        public override IUnitConvert UnitConvert { get; }

        public override Task<PullResponse> Pull(object args, string name) =>
            throw new NotSupportedException($"The in-process connection cannot pull '{name}': a named pull is a route of the server and has no in-process equivalent. Pull with Pull objects, or call a Procedure.");

        public override Task<PullResponse> Pull(PullRequest pullRequest) => this.Execute(api => api.Pull(pullRequest));

        public override Task<SyncResponse> Sync(SyncRequest syncRequest) => this.Execute(api => api.Sync(syncRequest));

        public override Task<PushResponse> Push(PushRequest pushRequest) => this.Execute(api => api.Push(pushRequest));

        public override Task<InvokeResponse> Invoke(InvokeRequest invokeRequest) => this.Execute(api => api.Invoke(invokeRequest));

        public override Task<AccessResponse> Access(AccessRequest accessRequest) => this.Execute(api => api.Access(accessRequest));

        public override Task<PermissionResponse> Permission(PermissionRequest permissionRequest) => this.Execute(api => api.Permission(permissionRequest));

        private Task<TResponse> Execute<TResponse>(Func<Api, TResponse> call)
        {
            // As the server controllers do: one transaction per request, the user set before the
            // Api is created, and the transaction only disposed, because the builders commit and
            // roll back themselves.
            using var transaction = this.Database.CreateTransaction();
            var user = (IUser)transaction.Instantiate(this.UserId);
            transaction.Services.Get<IUserService>().User = user;
            var api = new Api(transaction, this.Configuration.Name, CancellationToken.None);
            return Task.FromResult(call(api));
        }
    }
}
