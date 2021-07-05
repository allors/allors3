// <copyright file="LocalWorkspace.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using Numbers;

    public class Workspace : Adapters.Workspace
    {
        public Workspace(DatabaseConnection database, IWorkspaceServices services, INumbers numbers) : base(database, services, numbers, new WorkspaceIdGenerator()) => this.Services.OnInit(this);

        public new DatabaseConnection DatabaseConnection => (DatabaseConnection)base.DatabaseConnection;

        public long UserId => this.DatabaseConnection.UserId;

        public override ISession CreateSession() => new Session(this, this.Services.CreateSessionServices());
    }
}
