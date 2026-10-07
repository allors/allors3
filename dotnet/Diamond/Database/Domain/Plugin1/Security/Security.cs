// <copyright file="Security.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    // The hooks of Plugin1 in the security setup of a database: each records that it ran.
    public partial class Security
    {
        private void Plugin1OnPreSetup() => this.Plugin1Log.Record("Plugin1", "Security.OnPreSetup");

        private void Plugin1OnPostSetup() => this.Plugin1Log.Record("Plugin1", "Security.OnPostSetup");

        private IPlugin1Log Plugin1Log => this.transaction.Database.Services.Get<IPlugin1Log>();
    }
}
