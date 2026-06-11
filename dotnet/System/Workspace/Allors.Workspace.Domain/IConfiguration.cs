// <copyright file="IConfiguration.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace
{
    using System;
    using Meta;
    using Signals;

    public interface IConfiguration
    {
        string Name { get; }

        IMetaPopulation MetaPopulation { get; }

        IObjectFactory ObjectFactory { get; }

        /// <summary>
        /// Invoked once per session to create that session's <see cref="ISignalFactory"/>.
        /// Per-session factories keep each session's reactive graph and effect scheduler
        /// isolated; the default engine is single-threaded and must not be shared across
        /// sessions that run on different threads (e.g. Blazor Server circuits).
        /// </summary>
        Func<ISignalFactory> SignalFactoryBuilder { get; }
    }
}
