// <copyright file="Setup.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    // The hooks of Plugin1 in the setup of a database: each records that it ran.
    public partial class Setup
    {
        private void Plugin1OnPrePrepare() => this.Plugin1Log.Record("Plugin1", "Setup.OnPrePrepare");

        private void Plugin1OnPostPrepare() => this.Plugin1Log.Record("Plugin1", "Setup.OnPostPrepare");

        private void Plugin1OnPreSetup() => this.Plugin1Log.Record("Plugin1", "Setup.OnPreSetup");

        private void Plugin1OnPostSetup(Config config) => this.Plugin1Log.Record("Plugin1", "Setup.OnPostSetup");

        private IPlugin1Log Plugin1Log => this.transaction.Database.Services.Get<IPlugin1Log>();
    }
}
