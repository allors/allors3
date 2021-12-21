// <copyright file="WorkTask.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class WorkTask
    {
        public void CustomOnPostDerive(ObjectOnPostDerive method)
        {
            this.SecurityTokens = new[]
            {
                this.Strategy.IsNewInTransaction ? new SecurityTokens(this.strategy.Transaction).InitialSecurityToken : new SecurityTokens(this.strategy.Transaction).DefaultSecurityToken
            };

            if (this.Customer is Organisation customer)
            {
                this.AddSecurityToken(customer.ContactsSecurityToken);
            }
        }
    }
}
