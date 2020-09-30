// <copyright file="AccessControlList.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System.Collections.Generic;
    using Allors.Meta;

    /// <summary>
    /// List of permissions for an object/user combination.
    /// </summary>
    public interface IAccessControlList
    {
        AccessControl[] AccessControls { get; }

        HashSet<long> DeniedPermissionIds { get; }

        Object Object { get; }

        bool CanRead(IPropertyType propertyType);

        bool CanRead(IRoleType roleType);

        bool CanWrite(IRoleType roleType);

        bool CanRead(IAssociationType roleType);

        bool CanExecute(IMethodType methodType);
    }
}
