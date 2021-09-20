// <copyright file="IBarcodeGenerator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Allors.Ranges;

    public interface IGrantCache
    {
        void Clear(long accessControlId);

        // Database
        IRange<long> GetPermissions(long accessControlId);

        void SetPermissions(long accessControlId, IRange<long> permissionIds);

        // Workspace
        IRange<long> GetPermissions(string workspace, long accessControlId);

        void SetPermissions(string workspace, long accessControlId, IRange<long> permissionIds);
    }
}
