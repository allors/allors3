// <copyright file="IClass.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IObjectType type.</summary>

namespace Allors.Database.Meta
{
    using System;
    using System.Collections.Generic;

    public interface IClass : IComposite
    {
        Action<object, object>[] Actions(IMethodType methodType);

        IRoleType[] OverriddenRequiredRoleTypes { get; set; }

        IRoleType[] RequiredRoleTypes { get; }

        long CreatePermissionId { get; set; }

        IReadOnlyDictionary<Guid, long> ReadPermissionIdByRelationTypeId { get; set; }

        IReadOnlyDictionary<Guid, long> WritePermissionIdByRelationTypeId { get; set; }

        IReadOnlyDictionary<Guid, long> ExecutePermissionIdByMethodTypeId { get; set; }
    }
}
