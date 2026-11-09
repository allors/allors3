// <copyright file="DefaultConfigurationRoot.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System.Runtime.CompilerServices;
    using Allors.Database.Configuration;

    /// <summary>
    /// Points <c>ALLORS_CONFIG_ROOT</c> at the <c>config/snapshot</c> template of the repository before any
    /// test runs when the variable is not set, so the suite runs on the Snapshot adapter by default.
    /// </summary>
    internal static class DefaultConfigurationRoot
    {
        [ModuleInitializer]
        internal static void Apply() => TestConfigurationRoot.Default();
    }
}
