// <copyright file="LocalWorkspace.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using Numbers;

    public class Workspace : Adapters.Workspace
    {
        public Workspace(DatabaseConnection database, IWorkspaceLifecycle lifecycle, INumbers numbers) : base(database, lifecycle, numbers, new WorkspaceIdGenerator()) => this.Lifecycle.OnInit(this);

        public long UserId => ((DatabaseConnection)this.Database).UserId;

        public override ISession CreateSession() => new Session(this, this.Lifecycle.CreateSessionContext());
    }
}
