// <copyright file="Singletons.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class SecurityTokens
    {
        protected override void AppsPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.Grant);

        protected override void AppsSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            var grants = new Grants(this.Transaction);

            merge(DefaultSecurityTokenId, v =>
            {
                if (setup.Config.SetupSecurity)
                {
                    v.AddGrant(grants.Employees);
                }
            });
        }
    }
}
