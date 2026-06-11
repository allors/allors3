// <copyright file="Setup.v.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class Setup
    {
        private void OnPrePrepare()
        {
            this.CoreOnPrePrepare();
            this.IdentityOnPrePrepare();
            this.BaseOnPrePrepare();
            this.AppsOnPrePrepare();
            this.CustomOnPrePrepare();
        }

        private void OnPostPrepare()
        {
            this.CoreOnPostPrepare();
            this.IdentityOnPostPrepare();
            this.BaseOnPostPrepare();
            this.AppsOnPostPrepare();
            this.CustomOnPostPrepare();
        }

        private void OnPreSetup()
        {
            this.CoreOnPreSetup();
            this.IdentityOnPreSetup();
            this.BaseOnPreSetup();
            this.AppsOnPreSetup();
            this.CustomOnPreSetup();
        }

        private void OnPostSetup(Config config)
        {
            this.CoreOnPostSetup(config);
            this.IdentityOnPostSetup(config);
            this.BaseOnPostSetup(config);
            this.AppsOnPostSetup(config);
            this.CustomOnPostSetup(config);
        }
    }
}
