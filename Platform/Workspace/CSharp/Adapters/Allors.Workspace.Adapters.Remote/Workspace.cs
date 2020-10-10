// <copyright file="Session.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using Meta;

    public class Workspace : IWorkspace
    {
        public Workspace(IMetaPopulation metaPopulation, Type instance, IWorkspaceStateLifecycle state, HttpClient httpClient)
        {
            this.MetaPopulation = metaPopulation;
            this.StateLifecycle = state;

            this.ObjectFactory = new ObjectFactory(this.MetaPopulation, instance);
            this.Database = new Database(this.MetaPopulation, httpClient);
            this.Sessions = new HashSet<Session>();

            this.StateLifecycle.OnInit(this);
        }

        public IMetaPopulation MetaPopulation { get; }

        public IWorkspaceStateLifecycle StateLifecycle { get; }

        IObjectFactory IWorkspace.ObjectFactory => this.ObjectFactory;

        internal ObjectFactory ObjectFactory { get; }

        internal Database Database { get; }

        internal ISet<Session> Sessions { get; }

        public ISession CreateSession() => new Session(this, this.StateLifecycle.CreateSessionState());

        internal void RegisterSession(Session session) => this.Sessions.Add(session);

        internal void UnregisterSession(Session session) => this.Sessions.Remove(session);
    }
}
