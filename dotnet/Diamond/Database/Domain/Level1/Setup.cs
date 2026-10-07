// <copyright file="Setup.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    // The hooks of Level1 in the setup of a database: each records that it ran.
    public partial class Setup
    {
        private void Level1OnPrePrepare() => this.Level1Log.Record("Level1", "Setup.OnPrePrepare");

        private void Level1OnPostPrepare() => this.Level1Log.Record("Level1", "Setup.OnPostPrepare");

        private void Level1OnPreSetup() => this.Level1Log.Record("Level1", "Setup.OnPreSetup");

        private void Level1OnPostSetup(Config config) => this.Level1Log.Record("Level1", "Setup.OnPostSetup");

        private ILevel1Log Level1Log => this.transaction.Database.Services.Get<ILevel1Log>();
    }
}
