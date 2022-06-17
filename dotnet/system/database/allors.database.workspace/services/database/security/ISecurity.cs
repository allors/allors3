// <copyright file="IBarcodeGenerator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Database.Security;

    public interface ISecurity
    {
        IVersionedGrant[] GetVersionedGrants(ITransaction transaction, IUser user, ISecurityToken[] securityTokens);

        IVersionedGrant[] GetVersionedGrants(ITransaction transaction, IUser user, ISecurityToken[] securityTokens, string workspaceName);

        IVersionedGrant[] GetVersionedGrants(ITransaction transaction, IUser user, IGrant[] grants, string workspaceName);

        IVersionedRevocation[] GetVersionedRevocations(ITransaction transaction, IUser user, IRevocation[] revocations);

        IVersionedRevocation[] GetVersionedRevocations(ITransaction transaction, IUser user, IRevocation[] revocations, string workspaceName);
    }
}
