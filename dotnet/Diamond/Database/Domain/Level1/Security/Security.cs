// <copyright file="Security.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    // The hooks of Level1 in the security setup of a database: each records that it ran.
    public partial class Security
    {
        private void Level1OnPreSetup() => this.Level1Log.Record("Level1", "Security.OnPreSetup");

        private void Level1OnPostSetup() => this.Level1Log.Record("Level1", "Security.OnPostSetup");

        private ILevel1Log Level1Log => this.transaction.Database.Services.Get<ILevel1Log>();
    }
}
