// <copyright file="IBarcodeGenerator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using Database.Security;

    public interface ISecurity
    {
        IVersionedGrants GetVersionedGrantIdsForUser(IUser user);

        IDictionary<IGrant, IVersionedPermissions> GetGrantPermissions(ITransaction transaction, IEnumerable<IGrant> grants);

        IDictionary<IGrant, IVersionedPermissions> GetGrantPermissions(ITransaction transaction, IEnumerable<IGrant> grants, string workspaceName);

        IDictionary<IRevocation, IVersionedPermissions> GetRevocationPermissions(ITransaction transaction, IEnumerable<IRevocation> revocations);

        IDictionary<IRevocation, IVersionedPermissions> GetRevocationPermissions(ITransaction transaction, IEnumerable<IRevocation> revocations, string workspaceName);
    }
}
