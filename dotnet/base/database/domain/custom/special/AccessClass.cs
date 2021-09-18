// <copyright file="AccessClass.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class AccessClass
    {
        public void CustomDelegateAccess(DelegatedAccessObjectDelegateAccess method)
        {
            if (this.Block)
            {
                return;
            }

            var defaultSecurityToken = new SecurityTokens(this.Transaction()).DefaultSecurityToken;
            var initialSecurityToken = new SecurityTokens(this.Transaction()).InitialSecurityToken;

            method.SecurityTokens = new[] { defaultSecurityToken, initialSecurityToken };
        }
    }
}
