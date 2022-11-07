// <copyright file="Organisation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class OrganisationContactRelationship
    {
        public void CustomOnPostDerive(ObjectOnPostDerive method)
        {
            var transaction = this.Strategy.Transaction;

            this.SecurityTokens = new[]
            {
                new SecurityTokens(this.Transaction()).DefaultSecurityToken, this.Organisation?.ContactsSecurityToken
            };
        }
    }
}
