// <copyright file="Configuration.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using Meta;
    using Signals;

    public class Configuration : Adapters.Configuration
    {
        public Configuration(string name, IMetaPopulation metaPopulation, ReflectionObjectFactory objectFactory, ISignalFactory signalFactory) : base(name, metaPopulation, objectFactory, signalFactory) { }
    }
}
