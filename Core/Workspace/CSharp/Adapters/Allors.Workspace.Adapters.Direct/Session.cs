// <copyright file="Session.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Direct
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Data;
    using Meta;
    using Protocol.Direct;

    public class Session : ISession
    {
        public Session(Workspace workspace, ISessionLifecycle sessionLifecycle)
        {
            this.Workspace = workspace;
            this.SessionLifecycle = sessionLifecycle;
            this.Workspace.RegisterSession(this);

            this.SessionLifecycle.OnInit(this);
        }

        ~Session() => this.Workspace.UnregisterSession(this);

        IWorkspace ISession.Workspace => this.Workspace;
        internal Workspace Workspace { get; }

        public ISessionLifecycle SessionLifecycle { get; }

        public T Create<T>() where T : class, IObject => throw new System.NotImplementedException();

        public IObject Create(IClass @class) => throw new System.NotImplementedException();

        public T Instantiate<T>(T @object) where T : IObject => throw new System.NotImplementedException();

        public IObject Instantiate(long id) => throw new System.NotImplementedException();

        public IEnumerable<IObject> Instantiate(IEnumerable<long> ids) => throw new System.NotImplementedException();

        public void Reset() => throw new System.NotImplementedException();

        public void Refresh(bool merge = false) => throw new System.NotImplementedException();

        public Task<ICallResult> Call(Method method, CallOptions options = null) => throw new System.NotImplementedException();

        public Task<ICallResult> Call(Method[] methods, CallOptions options = null) => throw new System.NotImplementedException();

        public Task<ICallResult> Call(string service, object args) => throw new System.NotImplementedException();

        public Task<ILoadResult> Load(params Pull[] pulls)
        {
            using var databaseSession = this.Workspace.Database.CreateSession();
            var visitor = new ToDatabaseVisitor(databaseSession);

            var loadResults = new LoadResult(this.Workspace);
            
            foreach (var pull in pulls)
            {
                var databasePull = visitor.Visit(pull);
                // TODO: Load
            }
            
            return Task.FromResult<ILoadResult>(loadResults);
        }

        public Task<ILoadResult> Load(string service, object args) => throw new System.NotImplementedException();

        public Task<ISaveResult> Save() => throw new System.NotImplementedException();
    }
}
