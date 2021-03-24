// <copyright file="IDatabaseScope.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using System.Security;
    using Configuration;
    using Derivations;
    using Domain;
    using Meta;

    public partial class WorkspaceContext : IWorkspaceContext
    {
        private readonly IRule[] rules = new IRule[]
        {

        };

        public M M { get; set; }

        public IDerivationFactory DerivationFactory { get; private set; }

        public void OnInit(IWorkspace workspace)
        {
            this.M = new M((MetaPopulation)workspace.MetaPopulation);
            this.DerivationFactory = new DefaultDerivationFactory(this.rules);
        }

        public void Dispose()
        {
        }

        public ISessionLifecycle CreateSessionContext() => new SessionContext();
    }
}
