// <copyright file="IMetaCache.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using Meta;

    public partial interface IMetaCache
    {
        RoleType[] GetRequiredRoleTypes(IClass @class);

        RoleType[] GetUniqueRoleTypes(IClass @class);

        Type GetBuilderType(IClass @class);
    }
}
