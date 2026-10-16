// <copyright file="Configuration.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using Derivations;
    using Meta;

    public abstract class Configuration : IConfiguration
    {
        private readonly IRule[] rules;

        protected Configuration(string name, IMetaPopulation metaPopulation, IObjectFactory objectFactory, IRule[] rules)
        {
            this.Name = name;
            this.MetaPopulation = metaPopulation;
            this.ObjectFactory = objectFactory;
            this.rules = rules == null ? null : (IRule[])rules.Clone();
        }

        public string Name { get; }

        public IMetaPopulation MetaPopulation { get; }

        public IObjectFactory ObjectFactory { get; }

        public IRule[] Rules => this.rules == null ? null : (IRule[])this.rules.Clone();
    }
}
