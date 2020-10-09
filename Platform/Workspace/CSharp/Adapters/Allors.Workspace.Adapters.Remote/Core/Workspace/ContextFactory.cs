// <copyright file="Context.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Collections.Generic;

    public class ContextFactory : IContextFactory
    {
        public ContextFactory(ClientDatabase database, InternalWorkspace internalWorkspace, IWorkspaceLifecycle lifecycle)
        {
            this.Database = database;
            this.InternalWorkspace = internalWorkspace;
            this.Lifecycle = lifecycle;
            this.Contexts = new HashSet<Context>();

            this.Lifecycle.OnInit(this);
        }

        internal ClientDatabase Database { get; }

        internal InternalWorkspace InternalWorkspace { get; }

        internal ISet<Context> Contexts { get; }

        public IWorkspaceLifecycle Lifecycle { get; }

        public IObjectFactory ObjectFactory => this.InternalWorkspace.ObjectFactory;

        public IContext CreateContext() => new Context(this, this.Lifecycle.CreateSessionScope());

        internal void RegisterContext(Context context) => this.Contexts.Add(context);

        internal void UnregisterContext(Context context) => this.Contexts.Remove(context);
    }
}
