// <copyright file="Session.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Collections.Generic;
    using Meta;

    public class Workspace : IWorkspace
    {
        public Workspace(Database database, IWorkspaceStateLifecycle stateLifecycle)
        {
            this.Database = database;
            this.StateLifecycle = stateLifecycle;

            this.Contexts = new HashSet<Session>();

            this.StateLifecycle.OnInit(this);
        }

        internal Database Database { get; }

        internal ISet<Session> Contexts { get; }

        public IWorkspaceStateLifecycle StateLifecycle { get; }

        public IMetaPopulation MetaPopulation => this.ObjectFactory.MetaPopulation;

        public IObjectFactory ObjectFactory => this.Database.ObjectFactory;

        public ISession CreateSession() => new Session(this, this.StateLifecycle.CreateSessionState());

        internal void RegisterContext(Session session) => this.Contexts.Add(session);

        internal void UnregisterContext(Session session) => this.Contexts.Remove(session);
    }
}
