// <copyright file="IDatabaseScope.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using Meta;

    public partial class WorkspaceContext : IWorkspaceContext
    {
        public M M { get; private set; }

        public void Dispose()
        {
        }

        public void OnInit(IWorkspace workspace) => this.M = new M((MetaPopulation)workspace.MetaPopulation);

        public ISessionLifecycle CreateSessionContext() => new SessionContext();
    }
}