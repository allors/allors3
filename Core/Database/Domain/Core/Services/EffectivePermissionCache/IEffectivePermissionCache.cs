// <copyright file="IBarcodeGenerator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;

    public interface IEffectivePermissionCache
    {
        void Clear(long accessControlId);

        ISet<long> Get(long accessControlId);

        void Set(long accessControlId, ISet<long> effectivePermissionIds);
    }
}
