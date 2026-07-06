// <copyright file="Setup.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Database.Services;

    public partial class Setup
    {
        private void TestOnPrePrepare()
        {
        }

        private void TestOnPostPrepare()
        {
        }

        private void TestOnPreSetup()
        {
        }

        private void TestOnPostSetup(Config config)
        {
            if (this.Config.SetupSecurity)
            {
                this.transaction.Database.Services.Get<IPermissions>().Sync(this.transaction);
            }
        }
    }
}
