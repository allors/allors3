// <copyright file="RemoteWorkspace.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using Ranges;

    public class Workspace : Adapters.Workspace
    {
        private bool closed;
        internal Workspace(DatabaseConnection database, IWorkspaceServices services, IRanges<long> recordRanges) : base(database, services, recordRanges) { }
        public new DatabaseConnection DatabaseConnection => (DatabaseConnection)base.DatabaseConnection;
        public override ISession CreateSession() => this.DatabaseConnection.CreateSession(this);
        internal void Close()
        {
            lock (this.DatabaseConnection.State) this.closed = true;
        }
        internal void RequireUsable()
        {
            if (this.closed) throw new InvalidOperationException("This workspace is closed or its initialization failed. Create a new workspace on a ready connection.");
        }
    }
}
