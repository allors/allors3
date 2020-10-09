// <copyright file="Context.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Collections.Generic;

    public class ContextFactory : IContextFactory
    {
        public ContextFactory(ClientDatabase database, Workspace workspace)
        {
            this.Database = database;
            this.Workspace = workspace;
            this.Contexts = new HashSet<Context>();
        }

        internal ClientDatabase Database { get; }

        internal Workspace Workspace { get; }

        internal ISet<Context> Contexts { get; }

        public IContext CreateContext() => new Context(this);


        internal void RegisterContext(Context context) => this.Contexts.Add(context);

        internal void UnregisterContext(Context context) => this.Contexts.Remove(context);
    }
}
