// <copyright file="Person.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class Person
    {
        public void CustomOnPostDerive(ObjectOnPostDerive method)
        {
            var transaction = this.Strategy.Transaction;

            this.SecurityTokens = new[]
            {
                new SecurityTokens(this.Transaction()).DefaultSecurityToken, this.OwnerSecurityToken
            };
        }
    }
}
