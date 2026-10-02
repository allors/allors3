// <copyright file="Users.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Linq;

    public partial class Users
    {
        // The user that an Entra identity stands for, or null when there is none. The identity is
        // the pair of the tenant id and the object id. The store cannot keep the pair to one user
        // across transactions that run at the same time (see UserEntraIdentityRule), so when more
        // than one user has it the oldest is the user: every server then finds the same one.
        public User FindByEntraIdentity(Guid tenantId, Guid objectId)
        {
            // No token carries an empty id.
            if (tenantId == Guid.Empty || objectId == Guid.Empty)
            {
                return null;
            }

            var extent = this.Extent();
            extent.Filter.AddEquals(this.Meta.EntraTenantId, tenantId);
            extent.Filter.AddEquals(this.Meta.EntraObjectId, objectId);
            return extent.OrderBy(v => v.Id).FirstOrDefault();
        }
    }
}
