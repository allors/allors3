// <copyright file="SecurityTokens.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class SecurityTokens
    {
        // An object without security tokens of its own takes its grants from the default security token,
        // and while it is new in a transaction from the initial security token. Core makes the setup of
        // security tokens run after the setup of grants.
        protected override void CustomSetup(Setup setup)
        {
            if (setup.Config.SetupSecurity)
            {
                this.DefaultSecurityToken.AddGrant(new Grants(this.Transaction).Workers);
            }
        }
    }
}
