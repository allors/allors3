// <copyright file="ICacheService.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.State
{
    using System;
    using Domain;

    public interface IPermissionsCache
    {
        IPermissionsCacheEntry Get(Guid classId);

        void Clear();
    }
}
