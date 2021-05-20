// <copyright file="RemoteWorkspace.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Net.Http;
    using Meta;

    public class Workspace : Adapters.Workspace
    {
        public Workspace(string name, IMetaPopulation metaPopulation, Type instance, IWorkspaceLifecycle state, HttpClient httpClient) : base(name, metaPopulation, instance, state)
        {
            this.RemoteDatabase = new Database(this.MetaPopulation, httpClient, new WorkspaceIdGenerator(), this.Numbers);
            this.Lifecycle.OnInit(this);
        }

        public override Adapters.Database Database => this.RemoteDatabase;

        internal Database RemoteDatabase { get; }

        public override ISession CreateSession() => new Session(this, this.Lifecycle.CreateSessionContext());
    }
}
