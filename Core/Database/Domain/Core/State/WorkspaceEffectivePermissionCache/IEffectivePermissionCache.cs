// <copyright file="IBarcodeGenerator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Services
{
    using System.Collections.Generic;

    public interface IWorkspaceEffectivePermissionCache
    {
        void Clear(long accessControlId);

        ISet<long> Get(string workspace, long accessControlId);

        void Set(string workspace, long accessControlId, ISet<long> effectivePermissionIds);
    }
}
