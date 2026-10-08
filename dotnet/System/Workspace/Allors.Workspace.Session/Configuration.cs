// <copyright file="Configuration.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Session
{
    using Connection;
    using Derivations;
    using Meta;

    /// <summary>
    /// The configuration of a workspace: the name and the meta population come from its
    /// connection, the object factory and the rules are the workspace's own.
    /// </summary>
    internal sealed class Configuration : IConfiguration
    {
        private readonly IDatabaseConnection connection;

        internal Configuration(IDatabaseConnection connection, IObjectFactory objectFactory, IRule[] rules)
        {
            this.connection = connection;
            this.ObjectFactory = objectFactory;
            this.Rules = rules;
        }

        public string Name => this.connection.WorkspaceName;

        public IMetaPopulation MetaPopulation => this.connection.MetaPopulation;

        public IObjectFactory ObjectFactory { get; }

        public IRule[] Rules { get; }
    }
}
