// <copyright file="Session.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Protocol.Database.Invoke;
    using Protocol.Database.Pull;
    using Protocol.Database.Push;
    using Protocol.Database.Sync;
    using Allors.Workspace.Data;
    using Meta;
    using Result = Allors.Workspace.Result;

    public class Session : ISession
    {
        private readonly DatabaseOrigin databaseOrigin;
        private readonly Remote database;

        public Session(Workspace workspace, ISessionStateLifecycle stateLifecycle)
        {
            this.Workspace = workspace;
            this.StateLifecycle = stateLifecycle;
            this.databaseOrigin = this.Workspace.DatabaseOrigin;
            this.database = this.Workspace.Database;
            this.SessionOrigin = new SessionOrigin(this, this.databaseOrigin);
            this.Workspace.RegisterContext(this);

            this.StateLifecycle.OnInit(this);
        }

        ~Session() => this.Workspace.UnregisterContext(this);

        IWorkspace ISession.Workspace => this.Workspace;

        public Workspace Workspace { get; }

        public SessionOrigin SessionOrigin { get; }

        public ISessionStateLifecycle StateLifecycle { get; }

        public ISessionObject Get(long id) => this.SessionOrigin.Get(id);

        public IEnumerable<ISessionObject> GetAssociation(ISessionObject @object, IAssociationType associationType) => this.SessionOrigin.GetAssociation(@object, associationType);

        public Task<InvokeResponse> Invoke(Method method, InvokeOptions options = null) => this.Invoke(new[] { method }, options);

        public Task<InvokeResponse> Invoke(Method[] methods, InvokeOptions options = null)
        {
            var invokeRequest = new InvokeRequest
            {
                I = methods.Select(v => new Invocation
                {
                    I = v.Object.Id.ToString(),
                    V = v.Object.Strategy.Version.ToString(),
                    M = v.MethodType.IdAsString,
                }).ToArray(),
                O = options,
            };

            return this.database.Invoke(invokeRequest);
        }

        public Task<InvokeResponse> Invoke(string service, object args) => this.database.Invoke(service, args);

        public async Task<Result> Load(params Pull[] pulls)
        {
            var pullRequest = new PullRequest { P = pulls.Select(v => v.ToJson()).ToArray() };
            var pullResponse = await this.database.Pull(pullRequest);
            var syncRequest = this.databaseOrigin.Diff(pullResponse);
            if (syncRequest.Objects.Length > 0)
            {
                await this.Load(syncRequest);
            }

            var result = new Result(this, pullResponse);
            return result;
        }

        public async Task<Result> Load(object args, string pullService = null)
        {
            if (args is Pull pull)
            {
                args = new PullRequest { P = new[] { pull.ToJson() } };
            }

            if (args is IEnumerable<Pull> pulls)
            {
                args = new PullRequest { P = pulls.Select(v => v.ToJson()).ToArray() };
            }

            var pullResponse = await this.database.Pull(pullService, args);
            var syncRequest = this.databaseOrigin.Diff(pullResponse);

            if (syncRequest.Objects.Length > 0)
            {
                await this.Load(syncRequest);
            }

            var result = new Result(this, pullResponse);
            return result;
        }

        public async Task<PushResponse> Save()
        {
            var saveRequest = this.SessionOrigin.PushRequest();
            var pushResponse = await this.database.Push(saveRequest);
            if (!pushResponse.HasErrors)
            {
                this.SessionOrigin.PushResponse(pushResponse);

                var objects = saveRequest.Objects.Select(v => v.I).ToArray();
                if (pushResponse.NewObjects != null)
                {
                    objects = objects.Union(pushResponse.NewObjects.Select(v => v.I)).ToArray();
                }

                var syncRequests = new SyncRequest
                {
                    Objects = objects,
                };

                await this.Load(syncRequests);

                this.SessionOrigin.Reset();
            }

            return pushResponse;
        }

        public void Reset() => this.SessionOrigin.Reset();

        public ISessionObject Create(IClass @class) => this.SessionOrigin.Create(@class);

        private async Task Load(SyncRequest syncRequest)
        {
            var syncResponse = await this.database.Sync(syncRequest);
            var securityRequest = this.databaseOrigin.Sync(syncResponse);

            if (securityRequest != null)
            {
                var securityResponse = await this.database.Security(securityRequest);
                securityRequest = this.databaseOrigin.Security(securityResponse);

                if (securityRequest != null)
                {
                    securityResponse = await this.database.Security(securityRequest);
                    this.databaseOrigin.Security(securityResponse);
                }
            }
        }
    }
}
