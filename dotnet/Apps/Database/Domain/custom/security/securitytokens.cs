// <copyright file="Singletons.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class SecurityTokens
    {
        protected override void CustomPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.Grant);

        protected override void CustomSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            var accessControls = new Grants(this.Transaction);

            merge(InitialSecurityTokenId, v =>
            {
                if (setup.Config.SetupSecurity)
                {
                    v.AddGrant(accessControls.CustomerContacts);
                }
            });

            merge(DefaultSecurityTokenId, v =>
            {
                if (setup.Config.SetupSecurity)
                {
                    v.AddGrant(accessControls.CustomerContacts);
                }
            });
        }
    }
}
