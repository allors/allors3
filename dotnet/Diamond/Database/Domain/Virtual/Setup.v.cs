// <copyright file="Setup.v.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    // The dispatch of each phase to the hooks of the domains, base first: the order of
    // MetaPopulation.SortedDomains reversed, which SetupOrderTests checks.
    public partial class Setup
    {
        private void OnPrePrepare()
        {
            this.CoreOnPrePrepare();
            this.Level1OnPrePrepare();
            this.Level2OnPrePrepare();
            this.Plugin1OnPrePrepare();
            this.TestOnPrePrepare();
        }

        private void OnPostPrepare()
        {
            this.CoreOnPostPrepare();
            this.Level1OnPostPrepare();
            this.Level2OnPostPrepare();
            this.Plugin1OnPostPrepare();
            this.TestOnPostPrepare();
        }

        private void OnPreSetup()
        {
            this.CoreOnPreSetup();
            this.Level1OnPreSetup();
            this.Level2OnPreSetup();
            this.Plugin1OnPreSetup();
            this.TestOnPreSetup();
        }

        private void OnPostSetup(Config config)
        {
            this.CoreOnPostSetup(config);
            this.Level1OnPostSetup(config);
            this.Level2OnPostSetup(config);
            this.Plugin1OnPostSetup(config);
            this.TestOnPostSetup(config);
        }
    }
}
