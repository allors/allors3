// <copyright file="Setup.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Services;

    // The hooks of the concrete domain in the setup of a database: each records that it ran, and
    // the last one creates the permissions.
    public partial class Setup
    {
        private void TestOnPrePrepare() => this.TestLog.Record("Test", "Setup.OnPrePrepare");

        private void TestOnPostPrepare() => this.TestLog.Record("Test", "Setup.OnPostPrepare");

        private void TestOnPreSetup() => this.TestLog.Record("Test", "Setup.OnPreSetup");

        private void TestOnPostSetup(Config config)
        {
            this.TestLog.Record("Test", "Setup.OnPostSetup");

            // The concrete domain knows every class of the population, so it is the one that
            // creates the permissions before security is applied.
            if (config.SetupSecurity)
            {
                this.transaction.Database.Services.Get<IPermissions>().Sync(this.transaction);
            }
        }

        private ILevel1Log TestLog => this.transaction.Database.Services.Get<ILevel1Log>();
    }
}
