// <copyright file="Configuration.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using Meta;
    using Numbers;

    public abstract class Configuration : IConfiguration
    {
        protected Configuration(string name, IMetaPopulation metaPopulation, ReflectionObjectFactory objectFactory, IWorkspaceLifecycle lifecycle)
        {
            this.Name = name;
            this.MetaPopulation = metaPopulation;
            this.ObjectFactory = objectFactory;
            this.Lifecycle = lifecycle;
            this.Numbers = new ArrayNumbers();
            this.workspaceIdGenerator = new WorkspaceIdGenerator();
        }

        public string Name { get; }

        public IMetaPopulation MetaPopulation { get; }

        IObjectFactory IConfiguration.ObjectFactory => this.ObjectFactory;
        public ReflectionObjectFactory ObjectFactory { get; }

        public IWorkspaceLifecycle Lifecycle { get; }

        public INumbers Numbers { get; }

        public WorkspaceIdGenerator workspaceIdGenerator { get; }
    }
}
