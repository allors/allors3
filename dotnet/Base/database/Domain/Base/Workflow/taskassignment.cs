// <copyright file="TaskAssignment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class TaskAssignment
    {
        public void BaseOnPostDerive(ObjectOnPostDerive _)
        {
            if (!this.ExistSecurityTokens)
            {
                var defaultSecurityToken = new SecurityTokens(this.Transaction()).DefaultSecurityToken;
                this.SecurityTokens = new[] { defaultSecurityToken, this.User?.OwnerSecurityToken };
            }
        }

        public void BaseDelete(DeletableDelete _) => this.Notification?.Delete();
    }
}
