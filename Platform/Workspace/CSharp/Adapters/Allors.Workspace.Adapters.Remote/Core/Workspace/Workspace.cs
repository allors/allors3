// <copyright file="Session.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Collections.Generic;

    public class Workspace : IWorkspace
    {
        public Workspace(Remote database, DatabaseOrigin databaseOrigin, IWorkspaceStateLifecycle stateLifecycle)
        {
            this.Database = database;
            this.DatabaseOrigin = databaseOrigin;
            this.StateLifecycle = stateLifecycle;
            this.Contexts = new HashSet<Session>();

            this.StateLifecycle.OnInit(this);
        }

        internal Remote Database { get; }

        internal DatabaseOrigin DatabaseOrigin { get; }

        internal ISet<Session> Contexts { get; }

        public IWorkspaceStateLifecycle StateLifecycle { get; }

        public IObjectFactory ObjectFactory => this.DatabaseOrigin.ObjectFactory;

        public ISession CreateSession() => new Session(this, this.StateLifecycle.CreateSessionState());

        internal void RegisterContext(Session session) => this.Contexts.Add(session);

        internal void UnregisterContext(Session session) => this.Contexts.Remove(session);
    }
}
