// <copyright file="Security.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    // The hooks of Level2 in the security setup of a database: each records that it ran.
    public partial class Security
    {
        private void Level2OnPreSetup() => this.Level2Log.Record("Level2", "Security.OnPreSetup");

        private void Level2OnPostSetup() => this.Level2Log.Record("Level2", "Security.OnPostSetup");

        private ILevel1Log Level2Log => this.transaction.Database.Services.Get<ILevel1Log>();
    }
}
