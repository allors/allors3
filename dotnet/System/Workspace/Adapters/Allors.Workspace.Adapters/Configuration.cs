// <copyright file="Configuration.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using System;
    using Meta;
    using Signals;

    public abstract class Configuration : IConfiguration
    {
        protected Configuration(string name, IMetaPopulation metaPopulation, IObjectFactory objectFactory, Func<ISignalFactory> signalFactoryBuilder)
        {
            this.Name = name;
            this.MetaPopulation = metaPopulation;
            this.ObjectFactory = objectFactory;
            this.SignalFactoryBuilder = signalFactoryBuilder ?? throw new ArgumentNullException(nameof(signalFactoryBuilder));
        }

        public string Name { get; }

        public IMetaPopulation MetaPopulation { get; }

        public IObjectFactory ObjectFactory { get; }

        public Func<ISignalFactory> SignalFactoryBuilder { get; }
    }
}
