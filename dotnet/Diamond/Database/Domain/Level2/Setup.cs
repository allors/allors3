// <copyright file="Setup.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    // The hooks of Level2 in the setup of a database: each records that it ran.
    public partial class Setup
    {
        private void Level2OnPrePrepare() => this.Level2Log.Record("Level2", "Setup.OnPrePrepare");

        private void Level2OnPostPrepare() => this.Level2Log.Record("Level2", "Setup.OnPostPrepare");

        private void Level2OnPreSetup() => this.Level2Log.Record("Level2", "Setup.OnPreSetup");

        private void Level2OnPostSetup(Config config) => this.Level2Log.Record("Level2", "Setup.OnPostSetup");

        private ILevel1Log Level2Log => this.transaction.Database.Services.Get<ILevel1Log>();
    }
}
