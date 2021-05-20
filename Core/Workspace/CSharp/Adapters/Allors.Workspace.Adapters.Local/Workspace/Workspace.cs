// <copyright file="LocalWorkspace.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using Allors.Database;
    using Meta;

    public class Workspace : Adapters.Workspace
    {
        public Workspace(string name, long userId, IMetaPopulation metaPopulation, Type instance, IWorkspaceLifecycle state, IDatabase wrappedDatabase) : base(name, metaPopulation, instance, state)
        {
            this.Database = new Database(this.MetaPopulation, wrappedDatabase);
            this.UserId = userId;
            this.Lifecycle.OnInit(this);
        }

        public long UserId { get; }

        internal Database Database { get; }

        public override ISession CreateSession() => new Session(this, this.Lifecycle.CreateSessionContext());
    }
}
