// <copyright file="TimeSheet.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class TimeSheet
    {
        public void AppsDelegateAccess(DelegatedAccessControlledObjectDelegateAccess method)
        {
            if (method.SecurityTokens == null)
            {
                var securityTokens = new[] { new SecurityTokens(this.Session()).DefaultSecurityToken, this.Worker.OwnerSecurityToken };
                method.SecurityTokens = securityTokens;
            }
        }

        public void AppsDelete(DeletableDelete method)
        {
            if (this.ExistTimeEntries)
            {
                throw new Exception("Cannot delete TimeSheet due to associated TimeEntry details");
            }
        }
    }
}
