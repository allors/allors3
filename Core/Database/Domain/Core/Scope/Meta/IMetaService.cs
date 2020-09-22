// <copyright file="ITreeService.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Services
{
    using System;
    using Allors.Meta;

    public partial interface IMetaService
    {
        RoleType[] GetRequiredRoleTypes(IClass @class);

        RoleType[] GetUniqueRoleTypes(IClass @class);

        Type GetBuilderType(IClass @class);
    }
}
