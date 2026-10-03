// <copyright file="EntraUserResolver.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Security
{
    using System.Security.Claims;
    using Database;
    using Database.Domain;
    using Services;

    // A signed-in principal of the Entra plug-in names its Entra identity, the tenant id and the
    // object id, whether it comes from the browser session or from a bearer token. The user is looked
    // up by that identity on every request: a stored object id would denote another object after a
    // database reset, and a user that was deleted is gone at its next request.
    public class EntraUserResolver : IUserResolver
    {
        public User Resolve(ClaimsPrincipal principal, ITransaction transaction)
        {
            var tenantId = principal.TenantId();
            var objectId = principal.ObjectId();
            return tenantId != null && objectId != null
                ? new Users(transaction).FindByEntraIdentity(tenantId.Value, objectId.Value)
                : null;
        }
    }
}
