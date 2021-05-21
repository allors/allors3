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
        public Workspace(DatabaseConnection database) : base(database) => this.Lifecycle.OnInit(this);

        public new DatabaseConnection Database => (DatabaseConnection)base.Database;

        public override ISession CreateSession() => new Session(this, this.Lifecycle.CreateSessionContext());
    }
}
