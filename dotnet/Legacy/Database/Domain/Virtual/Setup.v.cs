// <copyright file="Setup.v.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class Setup
    {
        private void OnPrePrepare()
        {
            this.CoreOnPrePrepare();
            this.LegacyOnPrePrepare();
            this.CustomOnPrePrepare();
        }

        private void OnPostPrepare()
        {
            this.CoreOnPostPrepare();
            this.LegacyOnPostPrepare();
            this.CustomOnPostPrepare();
        }

        private void OnPreSetup()
        {
            this.CoreOnPreSetup();
            this.LegacyOnPreSetup();
            this.CustomOnPreSetup();
        }

        private void OnPostSetup(Config config)
        {
            this.CoreOnPostSetup(config);
            this.LegacyOnPostSetup(config);
            this.CustomOnPostSetup(config);
        }
    }
}
