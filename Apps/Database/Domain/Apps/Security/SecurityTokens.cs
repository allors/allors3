// <copyright file="Singletons.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class SecurityTokens
    {
        protected override void AppsSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            var accessControls = new AccessControls(this.Transaction);

            merge(DefaultSecurityTokenId, v =>
            {
                if (setup.Config.SetupSecurity)
                {
                    v.AddAccessControl(accessControls.Employees);
                }
            });
        }
    }
}
