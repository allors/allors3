// <copyright file="Configuration.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using Derivations;
    using Meta;

    public class Configuration : Adapters.Configuration
    {
        public Configuration(string name, IMetaPopulation metaPopulation, ReflectionObjectFactory objectFactory, IRule[] rules) : base(name, metaPopulation, objectFactory, rules) { }
    }
}
