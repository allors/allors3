// <copyright file="Security.v.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    // The dispatch of each phase to the hooks of the domains, base first: the order of
    // MetaPopulation.SortedDomains reversed, which SetupOrderTests checks.
    public partial class Security
    {
        private void OnPreSetup()
        {
            this.CoreOnPreSetup();
            this.Level1OnPreSetup();
            this.Level2OnPreSetup();
            this.Plugin1OnPreSetup();
            this.TestOnPreSetup();
        }

        private void OnPostSetup()
        {
            this.CoreOnPostSetup();
            this.Level1OnPostSetup();
            this.Level2OnPostSetup();
            this.Plugin1OnPostSetup();
            this.TestOnPostSetup();
        }
    }
}
